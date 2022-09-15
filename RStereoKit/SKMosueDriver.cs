using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Input;
using RhuEngine.Linker;

using RNumerics;

using StereoKit;

using Key = StereoKit.Key;

namespace RStereoKit
{
	public sealed class SKMosueDriver : IMouseInputDriver
	{
		public Vector2f MousePos => (Vector2f)Input.Mouse.pos.v;

		public Vector2f MouseDelta => (Vector2f)Input.Mouse.posChange.v;

		public Vector2f ScrollDelta => new Vector2f(0f, Input.Mouse.scrollChange);

		public bool GetIsDown(MouseKeys key) {
			return Input.Key((Key)key).IsActive();
		}

		public void HideMouse() {
		}

		public void LockMouse() {
		}

		public void UnHideMouse() {
		}

		public void UnLockMouse() {
		}
	}
}
