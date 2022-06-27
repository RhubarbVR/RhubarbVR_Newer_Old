using System;
using System.Collections.Generic;
using System.Text;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class MainSettingsObject:SettingsObject
	{
		[SettingsField("Input Settings")]
		public InputSettingsObject InputSettings = new();
		[SettingsField("UI Settings")]
		public UISettingsObject UISettings = new();

		[SettingsField("Three Letter Language Name")]
		public string ThreeLetterLanguageName = null;
		[SettingsField()]
		public string MainMic = null;
		[SettingsField("KeyboardLayout")]
		public int KeyboardLayoutID = -1;
	}
}
