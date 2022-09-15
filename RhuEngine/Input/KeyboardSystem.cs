using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

namespace RhuEngine.Input
{
	public sealed class KeyboardSystem : IInputSystem
	{
		public InputManager InputManager { get; set; }

		public KeyboardSystem(InputManager inputManager) {
			InputManager = inputManager;
		}
		private readonly List<IKeyboardInputDriver> _keyboardDrivers = new();

		private readonly Key[] _keys = (Key[])Enum.GetValues(typeof(Key));
		private readonly List<Key> _lastFrame = new();
		private readonly List<Key> _thisFrame = new();
		public string TypeDelta { get; private set; }
		public bool IsKeyJustDown(Key key) {
			return !_lastFrame.Contains(key) & _thisFrame.Contains(key);
		}
		public bool IsKeyDown(Key key) {
			return _thisFrame.Contains(key);
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
		}
	}
}
