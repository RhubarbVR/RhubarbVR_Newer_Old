using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

using RhuSettings;

using RNumerics;

namespace RhuEngine.Settings
{
	public class UISettingsObject : SettingsObject
	{
		public class ColorSetting : SettingsObject
		{
			[SettingsField("Red")]
			public float R;
			[SettingsField("Green")]
			public float G;
			[SettingsField("Blue")]
			public float B;

			public Colorf GetColor(float alpha) {
				return new Colorf(R, G, B, alpha);
			}
			public ColorSetting(Colorf color) {
				R = color.r;
				G = color.g;
				B = color.b;
			}

			public ColorSetting(float r, float g, float b) {
				R = r;
				G = g;
				B = b;
			}

			public ColorSetting() {

			}
		}
		[SettingsField("Primary")]
		public ColorSetting Primary = new (Colorf.Black);
		[SettingsField("Secondary")]
		public ColorSetting Secondary = new (Colorf.RhubarbRed);
		[SettingsField("Tertiary")]
		public ColorSetting Tertiary = new (Colorf.RhubarbGreen);
		[SettingsField("Quaternary")]
		public ColorSetting Quaternary = new (Colorf.Violet);

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
