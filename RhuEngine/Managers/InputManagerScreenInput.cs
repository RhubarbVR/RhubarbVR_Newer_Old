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
using Microsoft.Extensions.ObjectPool;

namespace RhuEngine.Managers
{
	public sealed partial class InputManager : IManager
	{

		public class ScreenInput
		{
			public Vector3f pos = new(0, 1.84f, 0);

			public Vector2f yawpitch;
			public const float PITCH = 85.5f;
			public const float SPEED = -0.5f;

			public bool MouseFree { get; set; }
			public InputManager InputManager { get; }

			public event Action<bool> MouseStateUpdate;

			public ScreenInput(InputManager inputManager) {
				InputManager = inputManager;
				UnFreeMouse();
			}
			public void FreeMouse() {
				Engine.MainEngine.MouseFree = false;
				MouseFree = true;
				MouseStateUpdate?.Invoke(true);
			}
			public void UnFreeMouse() {
				Engine.MainEngine.MouseFree = true;
				MouseFree = false;
				MouseStateUpdate?.Invoke(false);
			}

			public Matrix _camPos = Matrix.Identity;
			public Matrix HeadPos { get; set; } = Matrix.Identity;

			public void Step() {
				if (InputManager.GetInputAction(InputTypes.UnlockMouse).JustActivated() && !InputManager._engine.HasKeyboard) {
					if (MouseFree) {
						UnFreeMouse();
					}
					else {
						FreeMouse();
					}
				}

				if (!MouseFree) {
					yawpitch += InputManager.MouseSystem.MouseDelta * SPEED;
					yawpitch.y = MathUtil.Clamp(yawpitch.y, -PITCH, PITCH);
				}
				var headData = Matrix.TR(pos, Quaternionf.CreateFromEuler(yawpitch.x, yawpitch.y, 0));
				RRenderer.LocalCam = headData;
				if (MouseFree) {
					var mousepos = InputManager.MouseSystem.MousePos;
					var size = new Vector2f(InputManager._engine.windowManager.MainWindow?.Width ?? 640, InputManager._engine.windowManager.MainWindow?.Height ?? 640);
					var x = (2.0f * mousepos.x / size.x) - 1.0f;
					var y = (2.0f * mousepos.y / size.y) - 1.0f;
					var ar = size.x / size.y;
					var tan = (float)Math.Tan(RRenderer.Fov * Math.PI / 360);
					var vectforward = new Vector3f(-x * tan * ar, y * tan, 1);
					var vectup = new Vector3f(0, 1, 0);
					HeadPos = Matrix.R(Quaternionf.LookRotation(vectforward, vectup)) * headData;
				}
				else {
					HeadPos = headData;
				}
			}
		}

		public ScreenInput screenInput;

	}
}
