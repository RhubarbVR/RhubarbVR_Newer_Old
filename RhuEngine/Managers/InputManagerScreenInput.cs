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

			public ScreenInput(InputManager inputManager) {
				InputManager = inputManager;
				UnFreeMouse();
			}
			public void FreeMouse() {
				Engine.MainEngine.MouseFree = false;
				MouseFree = true;
			}
			public void UnFreeMouse() {
				Engine.MainEngine.MouseFree = true;
				MouseFree = false;
			}

			public Matrix _camPos = Matrix.Identity;

			public void Step() {
				if (InputManager.GetInputAction(InputTypes.UnlockMouse).JustActivated()) {
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
			}
		}

		public ScreenInput screenInput;

	}
}
