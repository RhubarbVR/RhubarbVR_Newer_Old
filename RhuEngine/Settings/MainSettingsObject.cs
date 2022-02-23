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
	}
}
