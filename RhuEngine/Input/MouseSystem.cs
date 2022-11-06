using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.Managers;

using RNumerics;

namespace RhuEngine.Input
{
	public sealed class MouseSystem : IInputSystem
	{
		public InputManager InputManager { get; set; }

		public MouseSystem(InputManager inputManager) {
			InputManager = inputManager;
		}

		private IMouseInputDriver _mouseInputDriver;

		public Vector2f MousePos => _mouseInputDriver?.MousePos ?? Vector2f.Zero;

		public Vector2f MouseDelta => _mouseInputDriver?.MouseDelta ?? Vector2f.Zero;

		public Vector2f ScrollDelta => _mouseInputDriver?.ScrollDelta ?? Vector2f.Zero;

		private bool _mouseLock;
		private bool _mouseHidden;

		public bool MouseHidden
		{
			get => _mouseHidden;
			set {
				if (_mouseHidden != value) {
					_mouseHidden = value;
					if (value) {
						_mouseInputDriver?.HideMouse();
					}
					else {
						_mouseInputDriver?.UnHideMouse();
					}
				}
			}
		}

		public bool MouseLocked
		{
			get => _mouseLock;
			set {
				if (_mouseLock != value) {
					_mouseLock = value;
					if (value) {
						_mouseInputDriver?.LockMouse();
					}
					else {
						_mouseInputDriver?.UnLockMouse();
					}
				}
			}
		}



		private readonly MouseKeys[] _keys = (MouseKeys[])Enum.GetValues(typeof(MouseKeys));
		private readonly List<MouseKeys> _lastFrame = new();
		private readonly List<MouseKeys> _thisFrame = new();
		public bool IsMouseKeyJustDown(MouseKeys key) {
			return !_lastFrame.Contains(key) & _thisFrame.Contains(key);
		}
		public bool IsMouseKeyDown(MouseKeys key) {
			return _thisFrame.Contains(key);
		}
		public void RemoveDevice(IInputDevice inputDevice) {
			if(_mouseInputDriver == inputDevice) {
				_mouseInputDriver = null;
			}
		}

		public void LoadDevice(IInputDevice inputDevice) {
			if(inputDevice is IMouseInputDriver mouse) {
				_mouseInputDriver = _mouseInputDriver is null ? mouse : throw new InvalidOperationException("Mouse already added");
				if (_mouseLock) {
					_mouseInputDriver.LockMouse();
				}
				else {
					_mouseInputDriver.UnLockMouse();
				}
				if (_mouseHidden) {
					_mouseInputDriver.HideMouse();
				}
				else {
					_mouseInputDriver.UnHideMouse();
				}
			}
		}

		public void Update() {
			_lastFrame.Clear();
			_lastFrame.AddRange(_thisFrame);
			_thisFrame.Clear();
			if (_mouseInputDriver is null) {
				return;	
			}
			foreach (var key in _keys) {
				if (_mouseInputDriver.GetIsDown(key)) {
					_thisFrame.Add(key);
				}
			}
		}

		public void SetCurrsor(RCursorShape currsor,RTexture2D rTexture2D = null) {
			_mouseInputDriver?.SetCurrsor(currsor, rTexture2D);
		}
	}
}
