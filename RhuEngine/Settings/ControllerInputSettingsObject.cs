using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

using RhuSettings;


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
		[SettingsField("Time in seconds")]
		public float HoldTime = 0.5f;

		public bool HeldTime => IsHolding && (DateTime.UtcNow - TimeLastPress).Seconds > HoldTime;

		public bool HeldForTime { get; private set; }

		public bool IsHolding = false;

		public DateTime TimeLastPress = DateTime.UtcNow;

		public void UpdateCheck(bool isClickingThisFrame) {
			if (HeldForTime) {
				HeldForTime = false;
				TimeLastPress = DateTime.UtcNow;
			}
			if (isClickingThisFrame) {
				if (!IsHolding) {
					IsHolding = true;
					TimeLastPress = DateTime.UtcNow;
				}
				if (HeldTime & !HeldForTime) {
					HeldForTime = true;
				}
			}
			else {
				IsHolding = false;
			}
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

		public void UpdateController(IRController controller) {
			X1.UpdateCheck(controller.X1.IsActive());
			X2.UpdateCheck(controller.X2.IsActive());
			StickPress.UpdateCheck(controller.StickClick.IsActive());
		}

		public float GetInputFloatFromController(InputManager.InputTypes inputType, IRController controller) {
			var buttons = 0f;
			if (Trigger.positiveX == inputType) {
				buttons += controller.Trigger;
			}
			if (Trigger.negevitveX == inputType) {
				buttons += -controller.Trigger;
			}
			if (Grip.positiveX == inputType) {
				buttons += controller.Grip;
			}
			if (Grip.negevitveX == inputType) {
				buttons += -controller.Grip;
			}
			if (StickPress.Pressed == inputType) {
				buttons += controller.StickClick.IsActive()? 1:0;
			}
			if (StickPress.Hold == inputType) {
				buttons += StickPress.HeldForTime ? 1 : 0;
			}
			if (X1.Pressed == inputType) {
				buttons += controller.X1.IsActive() ? 1 : 0;
			}
			if (X1.Hold == inputType) {
				buttons += X1.HeldForTime ? 1 : 0;
			}
			if (X2.Pressed == inputType) {
				buttons += controller.X2.IsActive() ? 1 : 0;
			}
			if (X2.Hold == inputType) {
				buttons += X2.HeldForTime ? 1 : 0;
			}
			if (InputManager.InputTypes.StickLocker != inputType) {
				if (!(StickLocker != (controller.ModelEnum == KnownControllers.Vive)) || GetInputFloatFromController(InputManager.InputTypes.StickLocker,controller) >= 0.9f) {
					if (Stick.positiveX == inputType) {
						buttons += controller.Stick.YX.x;
					}
					if (Stick.negevitveX == inputType) {
						buttons += -controller.Stick.YX.x;
					}
					if (Stick.positiveY == inputType) {
						buttons += controller.Stick.YX.y;
					}
					if (Stick.negevitveY == inputType) {
						buttons += -controller.Stick.YX.y;
					}
				}
			}
			return buttons;
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
