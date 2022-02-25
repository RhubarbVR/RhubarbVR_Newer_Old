using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Managers;

using RhuSettings;

using StereoKit;

namespace RhuEngine.Settings
{
	public class ControllerButtonSettingsObject : SettingsObject
	{
		[SettingsField()]
		public InputManager.InputTypes Pressed = InputManager.InputTypes.None;
		[SettingsField()]
		public InputManager.InputTypes Hold = InputManager.InputTypes.None;
		public ControllerButtonSettingsObject(InputManager.InputTypes pressed, InputManager.InputTypes hold = InputManager.InputTypes.None) {
			Pressed = pressed;
			Hold = hold;
		}

		public ControllerButtonSettingsObject() {

		}
	}

	public class ControllerAixSettingsObject : SettingsObject
	{
		[SettingsField()]
		public InputManager.InputTypes positiveX = InputManager.InputTypes.None;
		[SettingsField()]
		public InputManager.InputTypes negevitveX = InputManager.InputTypes.None;
		public ControllerAixSettingsObject(InputManager.InputTypes main) {
			positiveX = main;
		}
		public ControllerAixSettingsObject() {

		}
	}

	public class ControllerAixsSettingsObject : SettingsObject
	{
		[SettingsField()]
		public InputManager.InputTypes positiveX = InputManager.InputTypes.None;
		[SettingsField()]
		public InputManager.InputTypes negevitveX = InputManager.InputTypes.None;
		[SettingsField()]
		public InputManager.InputTypes positiveY = InputManager.InputTypes.None;
		[SettingsField()]
		public InputManager.InputTypes negevitveY = InputManager.InputTypes.None;

		public ControllerAixsSettingsObject(InputManager.InputTypes up, InputManager.InputTypes down, InputManager.InputTypes left, InputManager.InputTypes right) {
			positiveY = up;
			negevitveY = down;
			positiveX = left;
			negevitveX = right;
		}
		public ControllerAixsSettingsObject() {

		}
	}

	public class ControllerInputSettingsObject : SettingsObject
	{
		[SettingsField("Stick Locker: usefull for vive wand users")]
		public bool StickLocker = false;

		[SettingsField("Grip")]
		public ControllerAixSettingsObject Grip = new(InputManager.InputTypes.Grab);

		[SettingsField("Stick Press")]
		public ControllerButtonSettingsObject StickPress = new(InputManager.InputTypes.StickLocker);

		[SettingsField("X1")]
		public ControllerButtonSettingsObject X1 = new(InputManager.InputTypes.ContextMenu, InputManager.InputTypes.OpenDash);
		
		[SettingsField("X2")]
		public ControllerButtonSettingsObject X2 = new(InputManager.InputTypes.Secondary);
		
		[SettingsField("Stick")]
		public ControllerAixsSettingsObject Stick = new(InputManager.InputTypes.Forward, InputManager.InputTypes.Back, InputManager.InputTypes.Left, InputManager.InputTypes.Right);
		
		[SettingsField("Trigger")]
		public ControllerAixSettingsObject Trigger = new(InputManager.InputTypes.Primary);

		public float GetInputFloatFromController(InputManager.InputTypes inputType, Controller controller) {
			if (Trigger.positiveX == inputType) {
				return controller.trigger;
			}
			if (Trigger.negevitveX == inputType) {
				return -controller.trigger;
			}
			if (Grip.positiveX == inputType) {
				return controller.grip;
			}
			if (Grip.negevitveX == inputType) {
				return -controller.grip;
			}
			if (StickPress.Pressed == inputType) {
				return controller.stickClick.IsJustActive()? 1:0;
			}
			if (StickPress.Hold == inputType) {
				//NotSetup;
			}
			if (X1.Pressed == inputType) {
				return controller.x1.IsJustActive() ? 1 : 0;
			}
			if (X1.Hold == inputType) {
				//NotSetup;
			}
			if (X2.Pressed == inputType) {
				return controller.x2.IsJustActive() ? 1 : 0;
			}
			if (X1.Hold == inputType) {
				//NotSetup;
			}
			if (InputManager.InputTypes.StickLocker != inputType) {
				if (!StickLocker || GetInputFloatFromController(InputManager.InputTypes.StickLocker,controller) >= 0.9f) {
					if (Stick.positiveX == inputType) {
						return controller.stick.YX.x;
					}
					if (Stick.negevitveX == inputType) {
						return -controller.stick.YX.x;
					}
					if (Stick.positiveY == inputType) {
						return controller.stick.YX.y;
					}
					if (Stick.negevitveY == inputType) {
						return -controller.stick.YX.y;
					}
				}
			}
			return 0f;
		}

		public ControllerInputSettingsObject(bool isNotMain) {
			if (!isNotMain) {
				return;
			}
			Stick = new(InputManager.InputTypes.ObjectPush, InputManager.InputTypes.ObjectPull, InputManager.InputTypes.RotateLeft, InputManager.InputTypes.RotateRight);
		}
		public ControllerInputSettingsObject() {

		}
	}
}
