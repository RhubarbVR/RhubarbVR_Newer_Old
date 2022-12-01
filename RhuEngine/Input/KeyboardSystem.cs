using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

namespace RhuEngine.Input
{
	public sealed class VirtualKeyboardInput : IKeyboardInputDriver
	{
		public string TypeDelta { get; set; }

		public HashSet<Key> PressingKeys = new();

		public bool GetIsDown(Key key) {
			return PressingKeys.Contains(key);
		}

		private void UpdateData() {
			TypeDelta = null;
			PressingKeys.Clear();
		}

		public void Update() {
			RUpdateManager.ExecuteOnStartOfFrame(UpdateData);
		}
	}

	public sealed class KeyboardSystem : IInputSystem
	{
		public InputManager InputManager { get; set; }

		public VirtualKeyboardInput virtualKeyboard;

		public KeyboardSystem(InputManager inputManager) {
			InputManager = inputManager;
			virtualKeyboard = new();
			_stateChangeTime = new double[_keys.Length];
			LoadDevice(virtualKeyboard);
		}
		private readonly List<IKeyboardInputDriver> _keyboardDrivers = new();

		public static readonly Key[] _keys = (Key[])Enum.GetValues(typeof(Key));
		private readonly List<Key> _lastFrame = new();
		private readonly List<Key> _thisFrame = new();
		private readonly double[] _stateChangeTime;
		public string TypeDelta { get; private set; }

		public double GetStateChangeTime(Key key) {
			return _stateChangeTime[Array.IndexOf(_keys, key)];
		}

		public bool IsKeyJustUp(Key key) {
			return _lastFrame.Contains(key) & !_thisFrame.Contains(key);
		}
		public bool IsKeyJustDown(Key key) {
			return !_lastFrame.Contains(key) & _thisFrame.Contains(key);
		}
		public bool IsKeyDown(Key key) {
			return _thisFrame.Contains(key);
		}
		public bool IsKeyUp(Key key) {
			return !_thisFrame.Contains(key);
		}

		public void RemoveDevice(IInputDevice inputDevice) {
			if (inputDevice is IKeyboardInputDriver keyboard) {
				_keyboardDrivers.Remove(keyboard);
			}
		}

		public void LoadDevice(IInputDevice inputDevice) {
			if (inputDevice is IKeyboardInputDriver keyboard) {
				_keyboardDrivers.Add(keyboard);
			}
		}

		public void Update() {
			virtualKeyboard.Update();
			_lastFrame.Clear();
			_lastFrame.AddRange(_thisFrame);
			_thisFrame.Clear();
			TypeDelta = null;
			foreach (var item in _keyboardDrivers) {
				foreach (var key in _keys) {
					if (item.GetIsDown(key)) {
						_thisFrame.Add(key);
					}
				}
				TypeDelta += item.TypeDelta;
			}

			for (var i = 0; i < _keys.Length; i++) {
				var key = _keys[i];
				if (_lastFrame.Contains(key) != _thisFrame.Contains(key)) {
					_stateChangeTime[i] = 0;
				}
				else {
					_stateChangeTime[i] += RTime.Elapsed;
				}
			}
		}
	}
}
