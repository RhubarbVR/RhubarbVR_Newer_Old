using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RhuEngine.AssetSystem;
using RhuEngine.AssetSystem.AssetProtocals;
using System.IO;
using System.Threading.Tasks;
using RhuEngine.Settings;
using RhuEngine.Linker;
using RNumerics;
using RhuEngine.Input;
using Microsoft.Extensions.ObjectPool;

namespace RhuEngine.Managers
{
	public sealed partial class InputManager : IManager
	{

		public UserInputAction MoveSpeed { get; private set; }
		public UserInputAction Back { get; private set; }
		public UserInputAction Forward { get; private set; }
		public UserInputAction Left { get; private set; }
		public UserInputAction Right { get; private set; }
		public UserInputAction Jump { get; private set; }
		public UserInputAction FlyUp { get; private set; }
		public UserInputAction FlyDown { get; private set; }
		public UserInputAction RotateLeft { get; private set; }
		public UserInputAction RotateRight { get; private set; }
		public UserInputAction ObjectPull { get; private set; }
		public UserInputAction ObjectPush { get; private set; }
		public UserInputAction OpenDash { get; private set; }
		public UserInputAction ChangeWorld { get; private set; }
		public UserInputAction ContextMenu { get; private set; }
		public UserInputAction Primary { get; private set; }
		public UserInputAction Secondary { get; private set; }
		public UserInputAction Grab { get; private set; }
		public UserInputAction UnlockMouse { get; private set; }
		public UserInputAction ObserverOpen { get; private set; }

		public UserInputAction GetInputAction(InputTypes inputTypes) {
			return inputTypes switch {
				InputTypes.MoveSpeed => MoveSpeed,
				InputTypes.Back => Back,
				InputTypes.Forward => Forward,
				InputTypes.Left => Left,
				InputTypes.Right => Right,
				InputTypes.Jump => Jump,
				InputTypes.FlyUp => FlyUp,
				InputTypes.FlyDown => FlyDown,
				InputTypes.RotateLeft => RotateLeft,
				InputTypes.RotateRight => RotateRight,
				InputTypes.ObjectPull => ObjectPull,
				InputTypes.ObjectPush => ObjectPush,
				InputTypes.OpenDash => OpenDash,
				InputTypes.ChangeWorld => ChangeWorld,
				InputTypes.ContextMenu => ContextMenu,
				InputTypes.Primary => Primary,
				InputTypes.Secondary => Secondary,
				InputTypes.Grab => Grab,
				InputTypes.UnlockMouse => UnlockMouse,
				InputTypes.ObserverOpen => ObserverOpen,
				_ => null,
			};
		}

		private void LoadInputActions() {
			MoveSpeed = new UserInputAction(this);
			Back = new UserInputAction(this);
			Forward = new UserInputAction(this);
			Left = new UserInputAction(this);
			Right = new UserInputAction(this);
			Jump = new UserInputAction(this);
			FlyUp = new UserInputAction(this);
			FlyDown = new UserInputAction(this);
			RotateLeft = new UserInputAction(this);
			RotateRight = new UserInputAction(this);
			ObjectPull = new UserInputAction(this);
			ObjectPush = new UserInputAction(this);
			OpenDash = new UserInputAction(this);
			ChangeWorld = new UserInputAction(this);
			ContextMenu = new UserInputAction(this);
			Primary = new UserInputAction(this);
			Secondary = new UserInputAction(this);
			Grab = new UserInputAction(this);
			UnlockMouse = new UserInputAction(this);
			ObserverOpen = new UserInputAction(this);
		}

		private void UpdateInputActions() {
			MoveSpeed.Update();
			Back.Update();
			Forward.Update();
			Left.Update();
			Right.Update();
			Jump.Update();
			FlyUp.Update();
			FlyDown.Update();
			RotateLeft.Update();
			RotateRight.Update();
			ObjectPull.Update();
			ObjectPush.Update();
			OpenDash.Update();
			ChangeWorld.Update();
			ContextMenu.Update();
			Primary.Update();
			Secondary.Update();
			Grab.Update();
			UnlockMouse.Update();
			ObserverOpen.Update();
		}

		private void SettingsUpdateInputActions() {
			if(_engine.MainSettings is null) {
				return;
			}
			if(MoveSpeed is null) {
				return;
			}
			MoveSpeed.BindedAction = _engine.MainSettings.InputSettings.Sprint;
			Back.BindedAction = _engine.MainSettings.InputSettings.Back;
			Forward.BindedAction = _engine.MainSettings.InputSettings.Forward;
			Left.BindedAction = _engine.MainSettings.InputSettings.Left;
			Right.BindedAction = _engine.MainSettings.InputSettings.Right;
			Jump.BindedAction = _engine.MainSettings.InputSettings.Jump;
			FlyUp.BindedAction = _engine.MainSettings.InputSettings.FlyUp;
			FlyDown.BindedAction = _engine.MainSettings.InputSettings.FlyDown;
			RotateLeft.BindedAction = _engine.MainSettings.InputSettings.RotateLeft;
			RotateRight.BindedAction = _engine.MainSettings.InputSettings.RotateRight;
			ObjectPull.BindedAction = _engine.MainSettings.InputSettings.ObjectPull;
			ObjectPush.BindedAction = _engine.MainSettings.InputSettings.ObjectPush;
			OpenDash.BindedAction = _engine.MainSettings.InputSettings.OpenDash;
			ChangeWorld.BindedAction = _engine.MainSettings.InputSettings.ChangeWorld;
			ContextMenu.BindedAction = _engine.MainSettings.InputSettings.ContextMenu;
			Primary.BindedAction = _engine.MainSettings.InputSettings.Primary;
			Secondary.BindedAction = _engine.MainSettings.InputSettings.Secondary;
			Grab.BindedAction = _engine.MainSettings.InputSettings.Grab;
			UnlockMouse.BindedAction = _engine.MainSettings.InputSettings.UnlockMouse;
			ObserverOpen.BindedAction = _engine.MainSettings.InputSettings.ObserverOpen;
		}

		public sealed class UserInputAction
		{
			public InputManager InputManager { get; set; }

			public InputAction BindedAction = new();

			public string[][] InputActions => BindedAction.Input;

			private float _lastFrameLeft;
			private float _thisFrameLeft;

			private float _lastFrameRight;
			private float _thisFrameRight;

			private float _lastFrameOther;
			private float _thisFrameOther;

			private float _lastFrame;
			private float _thisFrame;

			private void ProsessUpdate(ref float lastFrame,ref float thisFrame,Handed hand) {
				lastFrame = thisFrame;
				thisFrame = 0f;
				for (var i = 0; i < InputActions.Length; i++) {
					var currentValue = 1f;
					var currentAction = InputActions[i];
					if (currentAction.Length == 0) {
						currentValue = 0f;
					}
					for (var c = 0; c < currentAction.Length; c++) {
						currentValue *= InputManager.GetActionStringValue(currentAction[c], hand);
					}
					thisFrame += currentValue;
				}
				thisFrame = Math.Min(1f, thisFrame);
			}

			public void Update() {
				ProsessUpdate(ref _lastFrameLeft, ref _thisFrameLeft, Handed.Left);
				ProsessUpdate(ref _lastFrameRight, ref _thisFrameRight, Handed.Right);
				ProsessUpdate(ref _lastFrameOther, ref _thisFrameOther, Handed.Max);
				_lastFrame = _lastFrameLeft + _lastFrameOther + _lastFrameRight;
				_thisFrame = _thisFrameLeft + _thisFrameOther + _thisFrameRight;
				_lastFrame = Math.Min(1f, _lastFrame);
				_thisFrame = Math.Min(1f, _thisFrame);
			}

			public float RightDeltaValue() {
				return _thisFrameRight - _lastFrameRight;
			}
			public float RightRawValue() {
				return _thisFrameRight;
			}
			public bool RightActivated() {
				return _thisFrameRight != 0;
			}
			public bool RightJustActivated() {
				return _lastFrameRight == 0 & _thisFrameRight != 0;
			}

			public float OtherDeltaValue() {
				return _thisFrameOther - _lastFrameOther;
			}
			public float OtherRawValue() {
				return _thisFrameOther;
			}
			public bool OtherActivated() {
				return _thisFrameOther != 0;
			}
			public bool OtherJustActivated() {
				return _lastFrameOther == 0 & _thisFrameOther != 0;
			}


			public float LeftDeltaValue() {
				return _thisFrameLeft - _lastFrameLeft;
			}
			public float LeftRawValue() {
				return _thisFrameLeft;
			}
			public bool LeftActivated() {
				return _thisFrameLeft != 0;
			}
			public bool LeftJustActivated() {
				return _lastFrameLeft == 0 & _thisFrameLeft != 0;
			}



			public float DeltaValue() {
				return _thisFrame - _lastFrame;
			}
			public float RawValue() {
				return _thisFrame;
			}
			public bool Activated() {
				return _thisFrame != 0;
			}
			public bool JustActivated() {
				return _lastFrame == 0 & _thisFrame != 0;
			}

			public float HandedValue(Handed handed) {
				return handed switch {
					Handed.Left => LeftRawValue(),
					Handed.Right => RightRawValue(),
					_ => OtherRawValue(),
				};
			}

			public UserInputAction(InputManager inputManager) {
				InputManager = inputManager;
			}
		}

	}
}
