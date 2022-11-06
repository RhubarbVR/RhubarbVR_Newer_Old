using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Components;
using RhuEngine.Linker;

using RNumerics;

namespace RhuEngine.Input
{
	public interface IMouseInputDriver: IInputDevice
	{
		public bool GetIsDown(MouseKeys key);

		public Vector2f MousePos { get; }

		public Vector2f MouseDelta { get; }

		public Vector2f ScrollDelta { get; }

		public void LockMouse();
		public void UnLockMouse();

		public void HideMouse();
		public void UnHideMouse();
		void SetCurrsor(RCursorShape currsor, RTexture2D rTexture2D);
	}
}
