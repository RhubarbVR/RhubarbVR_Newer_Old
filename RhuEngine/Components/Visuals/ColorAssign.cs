using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Visuals" })]
	public sealed class ColorAssign : Component
	{
		public enum ColorSelection
		{
			Primary,
			Secondary,
			Tertiary,
			Quaternary
		}

		[OnChanged(nameof(UpdateColor))]
		public readonly Linker<Colorf> TargetColor;

		[OnChanged(nameof(UpdateColor))]
		public readonly Sync<ColorSelection> Color;

		[OnChanged(nameof(UpdateColor))]
		public readonly Sync<float> ColorShif;
		[Default(1f)]
		[OnChanged(nameof(UpdateColor))]
		public readonly Sync<float> Alpha;

		protected override void OnLoaded() {
			base.OnLoaded();
			Engine.SettingsUpdate += UpdateColor;
			UpdateColor();
		}

		public void UpdateColor() {
			if (TargetColor.Linked) {
				var mainColor = Color.Value switch {
					ColorSelection.Primary => Engine.MainSettings.UISettings.Primary.GetColor(Alpha),
					ColorSelection.Secondary => Engine.MainSettings.UISettings.Secondary.GetColor(Alpha),
					ColorSelection.Tertiary => Engine.MainSettings.UISettings.Tertiary.GetColor(Alpha),
					ColorSelection.Quaternary => Engine.MainSettings.UISettings.Quaternary.GetColor(Alpha),
					_ => Colorf.White,
				};
				var hsv = new ColorHSV(mainColor);
				var lightVal = -(hsv.v - 0.5f);
				hsv.v += lightVal * ColorShif.Value;
				TargetColor.LinkedValue = hsv.ConvertToRGB();
			}
		}
	}
}
