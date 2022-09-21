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

namespace RhuEngine.Managers
{
	public sealed partial class InputManager : IManager
	{
		private Engine _engine;

		private readonly List<IInputDevice> _inputDevices = new();
		private IInputSystem[] _inputSystems = Array.Empty<IInputSystem>();

		public void LoadInputDriver<T>() where T : IInputDevice, new() {
			LoadInputDriver(new T());
		}

		public void LoadInputDriver(IInputDevice inputDriver) {
			_inputDevices.Add(inputDriver);
			foreach (var item in _inputSystems) {
				item.LoadDevice(inputDriver);
			}
		}
		public void RemoveInputDriver(IInputDevice inputDriver) {
			_inputDevices.Remove(inputDriver);
			foreach (var item in _inputSystems) {
				item.RemoveDevice(inputDriver);
			}
			if (inputDriver is IDisposable disposable) {
				disposable.Dispose();
			}
		}

		public KeyboardSystem KeyboardSystem { get; private set; }
		public MouseSystem MouseSystem { get; private set; }
		public MicSystem MicSystem { get; private set; }

		public Matrix HeadMatrix { get; set; }

		public void Dispose() {
		}

		public void Init(Engine engine) {
			_engine = engine;
			KeyboardSystem = new KeyboardSystem(this);
			MouseSystem = new MouseSystem(this);
			screenInput = new ScreenInput(this);
			MicSystem = new MicSystem(this);
			_inputSystems = new IInputSystem[] {
				KeyboardSystem,
				MouseSystem,
				MicSystem,
			};
			LoadInputActions();
			_engine.EngineLink.LoadInput(this);
			SettingsUpdateInputActions();
			_engine.SettingsUpdate += SettingsUpdateInputActions;
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

		public Handed GetHand(bool main) {
			return main
				? _engine.MainSettings.InputSettings.RightHanded ? Handed.Right : Handed.Left
				: _engine.MainSettings.InputSettings.RightHanded ? Handed.Left : Handed.Right;
		}
		private float GetActionStringValue(string v) {
			return Math.Min(GetActionStringValue(v, Handed.Left) + GetActionStringValue(v, Handed.Right) + GetActionStringValue(v, Handed.Max), 1f);
		}

		private readonly Dictionary<(string, Handed), Func<float>> _actions = new();

		private static float ReturnZero() {
			return 0f;
		}

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
						_actions.Add(lookValue, ReturnZero);
						return 0f;
					}
					var keyAction = () => (KeyboardSystem.IsKeyDown(keydata) || MouseSystem.IsMouseKeyDown((MouseKeys)keydata)) ? 1f : 0f;
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
			_actions.Add(lookValue, ReturnZero);
			return 0f;
		}
		public void Step() {

		}
	}
}
