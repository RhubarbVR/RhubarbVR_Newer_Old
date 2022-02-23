using System;
using System.Collections.Generic;
using System.Text;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class InputSettingsObject : SettingsObject
	{
		[SettingsField("If your primary hand is the left hand")]
		public bool RightHanded = true;

		[SettingsField("Keyboard Settings")]
		public KeyboardInputSettingsObject KeyboardInputSettings = new();

		[SettingsField("GamePad Settings")]
		public GamePadInputSettingsObject GamePadInputSettings = new();

		[SettingsField("Main Controller Settings")]
		public ControllerInputSettingsObject MainControllerInputSettings = new ();

		[SettingsField("Secondary Controller Settings")]
		public ControllerInputSettingsObject SecondaryControllerInputSettings = new(true);
	}
}
