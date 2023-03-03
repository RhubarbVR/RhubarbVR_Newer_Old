using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using RhubarbVR.Bindings.TextureBindings;

using RhuEngine.Components;
using RhuEngine.Input;
using RhuEngine.Linker;

using RNumerics;

using Key = RhuEngine.Linker.Key;
using GInput = GDExtension.Input;

namespace RhubarbVR.Bindings.Input
{

	public class GodotMouse : IMouseInputDriver
	{
		public Vector2f MousePos => EngineRunnerHelpers._.MousePos;

		public Vector2f MouseDelta => EngineRunnerHelpers._.MouseDelta;

		public Vector2f ScrollDelta => EngineRunnerHelpers._.MouseScrollDelta;

		public bool GetIsDown(MouseKeys key) {
			return key switch {
				MouseKeys.MouseLeft => GInput.IsMouseButtonPressed(MouseButton.Left),
				MouseKeys.MouseRight => GInput.IsMouseButtonPressed(MouseButton.Right),
				MouseKeys.MouseCenter => GInput.IsMouseButtonPressed(MouseButton.Middle),
				MouseKeys.MouseForward => GInput.IsMouseButtonPressed(MouseButton.Xbutton1),
				MouseKeys.MouseBack => GInput.IsMouseButtonPressed(MouseButton.Xbutton2),
				_ => GInput.IsMouseButtonPressed(MouseButton.None),
			};
		}

		public bool HideMous = false;

		public bool CenterMous = false;

		private void UpdateMouseMode() {
			GInput.Singleton.MouseModeValue = HideMous
				? CenterMous ? GInput.MouseMode.Captured : GInput.MouseMode.Hidden
				: CenterMous ? GInput.MouseMode.Confined : GInput.MouseMode.Visible;
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

		public void SetCurrsor(RCursorShape currsor, RTexture2D rTexture2D) {
			if (rTexture2D is null) {
				GInput.SetDefaultCursorShape((GInput.CursorShape)currsor);
			}
			else {
				if (rTexture2D.Inst is GodotTexture2D godotTexture) {
					GInput.SetCustomMouseCursor(godotTexture.Texture, (GInput.CursorShape)currsor);
				}
			}
		}
	}
}
