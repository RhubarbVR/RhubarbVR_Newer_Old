using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Input;
using RhuEngine.Linker;

using RNumerics;

using Key = RhuEngine.Linker.Key;
namespace RhubarbVR.Bindings.Input
{
	using Input = Godot.Input;

	public class GodotMouse : IMouseInputDriver
	{
		public Vector2f MousePos => EngineRunner._.MousePos;

		public Vector2f MouseDelta => EngineRunner._.MouseDelta;

		public Vector2f ScrollDelta
		{
			get {

				var weely = (Input.IsMouseButtonPressed(MouseButton.WheelUp) ? 1f : 0f) - (Input.IsMouseButtonPressed(MouseButton.WheelDown) ? 1f : 0f);
				var weelx = (Input.IsMouseButtonPressed(MouseButton.WheelRight) ? 1f : 0f) - (Input.IsMouseButtonPressed(MouseButton.WheelLeft) ? 1f : 0f);
				return new Vector2f(weelx, weely);
			}
		}

		public bool GetIsDown(MouseKeys key) {
			return key switch {
				MouseKeys.MouseLeft => Input.IsMouseButtonPressed(MouseButton.Left),
				MouseKeys.MouseRight => Input.IsMouseButtonPressed(MouseButton.Right),
				MouseKeys.MouseCenter => Input.IsMouseButtonPressed(MouseButton.Middle),
				MouseKeys.MouseForward => Input.IsMouseButtonPressed(MouseButton.Xbutton1),
				MouseKeys.MouseBack => Input.IsMouseButtonPressed(MouseButton.Xbutton2),
				_ => Input.IsMouseButtonPressed(MouseButton.None),
			};
		}

		public bool HideMous = false;

		public static bool CenterMous = false;

		private void UpdateMouseMode() {
			if (HideMous) {
				if (CenterMous) {
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}
				else {
					Input.MouseMode = Input.MouseModeEnum.Hidden;
				}
			}
			else {
				if (CenterMous) {
					Input.MouseMode = Input.MouseModeEnum.Confined;
				}
				else {
					Input.MouseMode = Input.MouseModeEnum.Visible;
				}
			}
		}

		public void HideMouse() {
			HideMous = true;
			UpdateMouseMode();
		}

		public void LockMouse() {
			CenterMous = true;
			UpdateMouseMode();
		}

		public void UnHideMouse() {
			HideMous = false;
			UpdateMouseMode();
		}

		public void UnLockMouse() {
			CenterMous = false;
			UpdateMouseMode();
		}
	}
}
