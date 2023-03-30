using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "User" })]
	public sealed partial class NoClipLocomotion : LocomotionModule
	{
		[Default(1f)]
		public readonly Sync<float> MovementSpeed;
		[Default(50f)]
		public readonly Sync<float> RotationSpeed;
		[Default(2f)]
		public readonly Sync<float> MaxSprintSpeed;
		[Default(80f)]
		public readonly Sync<float> MaxSprintRotationSpeed;
		[Default(true)]
		public readonly Sync<bool> AllowMultiplier;

		protected override void OnAttach() {
			base.OnAttach();
			locmotionName.Value = "No Clip";
		}

		private void ProcessController(bool isMain) {
			if (UserRoot.head.Target is null) {
				return;
			}
			if (Engine.inputManager.GetHand(isMain) == Handed.Right) {
				if (WorldManager.PrivateSpaceManager.Right.IsAnyLaserGrabbed) {
					return;
				}
			}
			else {
				if (WorldManager.PrivateSpaceManager.Left.IsAnyLaserGrabbed) {
					return;
				}
			}
			var speed = AllowMultiplier ? MathUtil.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var tempRight = InputManager.GetInputAction(InputTypes.Right).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsed;
			var tempLeft = InputManager.GetInputAction(InputTypes.Left).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsed;
			if (Engine.inputManager.GetHand(isMain) == Handed.Right) {
				tempRight = 0;
				tempLeft = 0;
			}
			var tempFlyUp = InputManager.GetInputAction(InputTypes.FlyUp).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsed;
			var tempFlyDown = InputManager.GetInputAction(InputTypes.FlyDown).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsed;
			var tempForward = InputManager.GetInputAction(InputTypes.Forward).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsed;
			var tempBack = InputManager.GetInputAction(InputTypes.Back).HandedValue(InputManager.GetHand(isMain)) * RTime.Elapsed;
			var pos = new Vector3f(0, 0, -tempForward + tempBack) * speed;
			var Rotspeed = AllowMultiplier ? MathUtil.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var rotateRight = InputManager.GetInputAction(InputTypes.RotateLeft).HandedValue(InputManager.GetHand(isMain));
			var rotateLeft = InputManager.GetInputAction(InputTypes.RotateRight).HandedValue(InputManager.GetHand(isMain));
			if (Engine.MainSettings.InputSettings.MovmentSettings.SnapTurn && Engine.inputManager.GetHand(isMain) == Handed.Right) {
				var snaped = (rotateLeft > 0.6) || (rotateRight > 0.6);
				var snapLeft = (rotateRight - rotateLeft) < 0;
				rotateLeft = 0;
				rotateRight = 0;
				if (snaped) {
					if (!_snapLastFrame) {
						if (snapLeft) {
							rotateLeft = Engine.MainSettings.InputSettings.MovmentSettings.SnapAmount;
						}
						else {
							rotateRight = Engine.MainSettings.InputSettings.MovmentSettings.SnapAmount;
						}
					}
				}
				_snapLastFrame = snaped;
			}
			else {
				rotateRight *= RTime.ElapsedF;
				rotateLeft *= RTime.ElapsedF;
			}
			var AddToMatrix = Matrix.T(pos);
			var handPos = InputManager.XRInputSystem.GetHand(Engine.inputManager.GetHand(isMain))[Input.XRInput.TrackerPos.Aim];
			ProcessGlobalRotToUserRootMovement(AddToMatrix, Matrix.TR(handPos.Position, handPos.Rotation) * UserRootEnity.GlobalTrans);
			var posHead = new Vector3f(tempRight - tempLeft, tempFlyUp - tempFlyDown, 0) * speed;
			ProcessGlobalRotToUserRootMovement(Matrix.T(posHead), UserRoot.head.Target.GlobalTrans);
			var otherHandNotUsable = !InputManager.XRInputSystem.GetHand(Engine.inputManager.GetHand(!isMain))[Input.XRInput.TrackerPos.Default].HasPos;
			if (Engine.inputManager.GetHand(isMain) == Handed.Right) {
				otherHandNotUsable |= WorldManager.PrivateSpaceManager.Left.IsAnyLaserGrabbed;
			}
			else {
				otherHandNotUsable |= WorldManager.PrivateSpaceManager.Right.IsAnyLaserGrabbed;
			}
			if (isMain || otherHandNotUsable) {
				var headPos = Matrix.T(UserRoot.head.Target.position.Value) * UserRootEnity.GlobalTrans;
				var headLocal = headPos * UserRootEnity.GlobalTrans.Inverse;
				var newHEadPos = Matrix.R((Quaternionf)Quaterniond.CreateFromEuler((rotateRight - rotateLeft) * Rotspeed, 0, 0)) * headPos;
				SetUserRootGlobal(headLocal.Inverse * newHEadPos);
			}
		}

		private bool _snapLastFrame;

		private void ProcessHeadBased() {
			var speed = AllowMultiplier ? MathUtil.Lerp(MovementSpeed, MaxSprintSpeed, MoveSpeed) : MovementSpeed;
			var left = (Engine.inputManager.GetInputAction(InputTypes.Left).LeftRawValue() + Engine.inputManager.GetInputAction(InputTypes.Left).RightRawValue()) * RTime.ElapsedF;
			var right = (Engine.inputManager.GetInputAction(InputTypes.Right).LeftRawValue() + Engine.inputManager.GetInputAction(InputTypes.Right).OtherRawValue()) * RTime.ElapsedF;
			var pos = new Vector3f(right - left, FlyUp - FlyDown, Back - Forward) * speed;
			var Rotspeed = AllowMultiplier ? MathUtil.Lerp(RotationSpeed, MaxSprintRotationSpeed, MoveSpeed) : RotationSpeed;
			var rotateLeft = (Engine.inputManager.GetInputAction(InputTypes.RotateLeft).RightRawValue() + Engine.inputManager.GetInputAction(InputTypes.RotateLeft).RightRawValue());
			var rotateRight = (Engine.inputManager.GetInputAction(InputTypes.RotateRight).RightRawValue() + Engine.inputManager.GetInputAction(InputTypes.RotateRight).OtherRawValue());
			if (Engine.MainSettings.InputSettings.MovmentSettings.SnapTurn) {
				var snaped = (rotateLeft > 0.6) || (rotateRight > 0.6);
				var snapLeft = (rotateRight - rotateLeft) < 0;
				rotateLeft = 0;
				rotateRight = 0;
				if (snaped) {
					if (!_snapLastFrame) {
						if (snapLeft) {
							rotateLeft = Engine.MainSettings.InputSettings.MovmentSettings.SnapAmount;
						}
						else {
							rotateRight = Engine.MainSettings.InputSettings.MovmentSettings.SnapAmount;
						}
					}
				}
				_snapLastFrame = snaped;
			}
			else {
				rotateRight *= RTime.ElapsedF;
				rotateLeft *= RTime.ElapsedF;
			}
			var rot = Quaternionf.CreateFromEuler((rotateLeft - rotateRight) * Rotspeed, 0, 0);
			var AddToMatrix = Matrix.T(pos);
			ProcessGlobalRotToUserRootMovement(AddToMatrix, LocalUser.userRoot?.Target.head.Target?.GlobalTrans ?? UserRootEnity.GlobalTrans);
			if (!WorldManager.PrivateSpaceManager.Head.IsAnyLaserGrabbed) {
				var headPos = Matrix.T(UserRoot.head.Target.position.Value) * UserRootEnity.GlobalTrans;
				var headLocal = headPos * UserRootEnity.GlobalTrans.Inverse;
				var newHEadPos = Matrix.R(rot) * headPos;
				SetUserRootGlobal(headLocal.Inverse * newHEadPos);
			}
		}

		public override void ProcessMovement() {
			if (UserRootEnity is null) {
				return;
			}
			if (!Engine.IsInVR) {
				ProcessHeadBased();
			}
			else {
				if (Engine.MainSettings.InputSettings.MovmentSettings.HeadBasedMovement) {
					ProcessHeadBased();
				}
				else {
					ProcessController(false);
					ProcessController(true);
				}
			}

		}
	}
}
