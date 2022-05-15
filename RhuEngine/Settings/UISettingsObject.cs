using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

using RhuSettings;


namespace RhuEngine.Settings
{
	public class UISettingsObject : SettingsObject
	{
		[SettingsField("Rectangle Rounding")]
		public float RectRounding;

		[SettingsField("Rectangle Rounding Steps quality of rounding")]
		public int RectRoundingSteps = 5;

		[SettingsField("Dash Rounding Steps quality of rounding")]
		public int DashRoundingSteps = 10;

		[SettingsField("Dash Top Offset")]
		public float TopOffset = 2;

		[SettingsField("Front Bind Angle")]
		public float FrontBindAngle = 135f;

		[SettingsField("Front Bind Radus")]
		public float FrontBindRadus = 7f;
		[SettingsField("Dash Offset Forward")]
		public float DashOffsetForward = 0.1f;
		[SettingsField("Dash Offset Down")]
		public float DashOffsetDown = 0.1f;
	}
}
