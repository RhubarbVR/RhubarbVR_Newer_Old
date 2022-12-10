using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using RhuEngine.Settings;
using RhuEngine.Linker;
using RNumerics;
using RhuEngine.Input;
using RhuEngine.Input.XRInput;

namespace RhuEngine.Managers
{
	/// <summary>
	/// The input manager.
	/// </summary>
	public sealed partial class InputManager : IManager
	{
		private Engine _engine;

		private event Action OnInputManagerLoaded;

		public void OnLoaded(Action action) {
			if (IsLoaded) {
				action();
			}
			else {
				OnInputManagerLoaded += action;
			}
		}


		private readonly List<IInputDevice> _inputDevices = new();
		private IInputSystem[] _inputSystems = Array.Empty<IInputSystem>();

		/// <summary>
		/// Loads the input driver.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void LoadInputDriver<T>() where T : IInputDevice, new() {
			LoadInputDriver(new T());
		}
		/// <summary>
		/// Loads the input driver.
		/// </summary>
		/// <param name="inputDriver">Input driver.</param>
		public void LoadInputDriver(IInputDevice inputDriver) {
			_inputDevices.Add(inputDriver);
			foreach (var item in _inputSystems) {
				item.LoadDevice(inputDriver);
			}
		}
		/// <summary>
		/// Removes the input driver.
		/// </summary>
		/// <param name="inputDriver">Input driver.</param>
		public void RemoveInputDriver(IInputDevice inputDriver) {
			_inputDevices.Remove(inputDriver);
			foreach (var item in _inputSystems) {
				item.RemoveDevice(inputDriver);
			}
			if (inputDriver is IDisposable disposable) {
				disposable.Dispose();
			}
		}
		/// <summary>
		/// the input system for all xr devices (controllers, camera, etc)
		/// </summary>
		public XRInputSystem XRInputSystem { get; private set; }
		/// <summary>
		/// The manager for keyboard inputs.
		/// </summary>
		public KeyboardSystem KeyboardSystem { get; private set; }
		/// <summary>
		/// The mouse system for the engine.
		/// </summary>
		public MouseSystem MouseSystem { get; private set; }
		/// <summary>
		/// A <see cref="MicSystem"/> instance that can be used to access the microphone.
		/// </summary>
		public MicSystem MicSystem { get; private set; }
		/// <summary>
		/// Gets the head position of the screen.
		/// </summary>
		public Matrix ScreenHeadMatrix => screenInput.HeadPos;

		public void Dispose() {
		}
		/// <summary>
		/// Is the input manager is loaded
		/// </summary>
		public bool IsLoaded { get; private set; } = false;

		/// <summary>
		/// Initializes the InputManager.
		/// </summary>
		public void Init(Engine engine) {
			_engine = engine;
			XRInputSystem = new XRInputSystem(this);
			KeyboardSystem = new KeyboardSystem(this);
			MouseSystem = new MouseSystem(this);
			MicSystem = new MicSystem(this);
			_inputSystems = new IInputSystem[] {
				XRInputSystem,
				KeyboardSystem,
				MouseSystem,
				MicSystem,
			};
			screenInput = new ScreenInput(this);
			LoadInputActions();
			_engine.EngineLink.LoadInput(this);
			SettingsUpdateInputActions();
			_engine.SettingsUpdate += SettingsUpdateInputActions;
			OnInputManagerLoaded?.Invoke();
			IsLoaded = true;
		}
		public void RenderStep() {
			foreach (var item in _inputSystems) {
				item.Update();
			}
			UpdateInputActions();
			if ((!_engine.IsInVR) && _engine.EngineLink.CanInput) {
				screenInput.Step();
			}
		}

		/// <summary>
		/// Gets the handed from the RightHanded bool is settings.
		/// </summary>
		public Handed GetHand(bool main) {
			return main
				? _engine.MainSettings.InputSettings.RightHanded ? Handed.Right : Handed.Left
				: _engine.MainSettings.InputSettings.RightHanded ? Handed.Left : Handed.Right;
		}

		/// <summary>
		/// Returns a value from -1 to 1 for a given input string.
		/// </summary>
		public float GetActionStringValue(string v) {
			return Math.Min(GetActionStringValue(v, Handed.Left) + GetActionStringValue(v, Handed.Right) + GetActionStringValue(v, Handed.Max), 1f);
		}

		private readonly Dictionary<(string, Handed), Func<float>> _actions = new();

		private static float ReturnZero() {
			return 0f;
		}

		/// <summary>
		/// parses input value
		/// 
		/// Return value is between 0 and 1
		/// 
		/// Input formats: 
		/// 
		/// Key.KeyName
		/// 
		/// Mouse.Scroll.Axis
		/// 
		/// XR.Main.ButtonName
		/// XR.Main.AxisName.Direction
		/// XR.Left.AxisName.Direction
		/// XR.Right.AxisName.Direction
		/// XR.Secondary.AxisName.Direction
		/// XR.F.AxisName.Direction
		/// 
		/// Direction is: x, x-, y, y-
		/// 
		/// F is replaced with the index of the device.
		///
		/// </summary>
		private float GetActionStringValue(string v, Handed handed) {
			var lookValue = (v, handed);
			if (_actions.TryGetValue(lookValue, out var action)) {
				return action();
			}
			if (v is null) {
				_actions.Add(lookValue, ReturnZero);
				return 0f;
			}
			var data = v.ToLower().Split('.');
			if (data.Length == 0) {
				_actions.Add(lookValue, ReturnZero);
				return 0f;
			}
			var device = data[0];
			if (data.Length == 1) {
				_actions.Add(lookValue, ReturnZero);
				return 0f;
			}
			var second = data[1];
			if (handed == Handed.Max) {
				if (device == "key") {
					if (!Enum.TryParse<Key>(second, true, out var keydata)) {
						if(Enum.TryParse<MouseKeys>(second,true,out var mouseCe)) {
							var mouseKeyAction = () => MouseSystem.IsMouseKeyDown(mouseCe) ? 1f : 0f;
							_actions.Add(lookValue, mouseKeyAction);
							return mouseKeyAction();
						}
						_actions.Add(lookValue, ReturnZero);
						return 0f;
					}
					var keyAction = () => KeyboardSystem.IsKeyDown(keydata) ? 1f : 0f;
					_actions.Add(lookValue, keyAction);
					return keyAction();
				}
			}
			if (data.Length == 2) {
				_actions.Add(lookValue, ReturnZero);
				return 0f;
			}
			var thread = data[2];
			if (handed == Handed.Max) {

				if (device == "mouse") {
					if (second == "scroll") {
						if (thread is "x") {
							var scrollAction = () => MouseSystem.ScrollDelta.x;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
						else if (thread is "x-" or "-x") {
							var scrollAction = () => -MouseSystem.ScrollDelta.x;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
						if (thread is "y") {
							var scrollAction = () => MouseSystem.ScrollDelta.y;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
						else if (thread is "y-" or "-y") {
							var scrollAction = () => -MouseSystem.ScrollDelta.y;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
					}
					if (second == "pos") {
						if (thread is "x") {
							var scrollAction = () => MouseSystem.MouseDelta.x;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
						else if (thread is "x-" or "-x") {
							var scrollAction = () => -MouseSystem.MouseDelta.x;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
						if (thread is "y") {
							var scrollAction = () => MouseSystem.MouseDelta.y;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
						else if (thread is "y-" or "-y") {
							var scrollAction = () => -MouseSystem.MouseDelta.y;
							_actions.Add(lookValue, scrollAction);
							return scrollAction();
						}
					}
				}
			}
			if (handed != Handed.Max) {

				var dir = "";
				if (data.Length > 3) {
					dir = data[3];
				}
				if (device == "xr") {
					Func<ITrackerDevice> target = null;
					if (second == "main") {
						target = () => XRInputSystem.GetHand(GetHand(true));
					}
					if (second == "secondary") {
						target = () => XRInputSystem.GetHand(GetHand(false));
					}
					if (int.TryParse(second, out var selectedIndex)) {
						target = () => XRInputSystem.Trackers.Count <= selectedIndex ? null : XRInputSystem.Trackers[selectedIndex];
					}
					if (second == "left" || handed == Handed.Left) {
						target = () => XRInputSystem.GetHand(Handed.Left);
					}
					if (second == "right" || handed == Handed.Right) {
						target = () => XRInputSystem.GetHand(Handed.Right);
					}
					if (target is not null) {
						var inputAction = () => {
							var targetDevice = target();
							if (targetDevice is null) {
								return 0f;
							}
							if (targetDevice.HasBoolInput(thread)) {
								return targetDevice.BoolInput(thread) ? 1f : 0f;
							}
							if (targetDevice.HasDoubleInput(thread)) {
								return (float)targetDevice.DoubleInput(thread);
							}
							if (targetDevice.HasVectorInput(thread)) {
								var data = targetDevice.VectorInput(thread);
								if (dir is "x") {
									return data.x;
								}
								else if (dir is "x-" or "-x") {
									return -data.x;
								}
								if (dir is "y") {
									return data.y;
								}
								else if (dir is "y-" or "-y") {
									return -data.y;
								}
							}
							return 0f;
						};
						_actions.Add(lookValue, inputAction);
						return inputAction();
					}
				}
			}
			_actions.Add(lookValue, ReturnZero);
			return 0f;
		}
		public void Step() {

		}
	}
}
