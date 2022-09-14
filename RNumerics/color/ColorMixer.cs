using System;
using System.Collections.Generic;


namespace RNumerics
{
	public static class ColorMixer
	{

		public static Colorf Lighten(in Colorf baseColor, in float fValueMult = 1.25f) {
			var baseHSV = new ColorHSV(baseColor);
			baseHSV.v = MathUtil.Clamp(baseHSV.v * fValueMult, 0.0f, 1.0f);
			return baseHSV.ConvertToRGB();
		}

		public static Colorf Darken(in Colorf baseColor, in float fValueMult = 0.75f) {
			var baseHSV = new ColorHSV(baseColor);
			baseHSV.v *= fValueMult;
			return baseHSV.ConvertToRGB();
		}


		public static Colorf CopyHue(in Colorf BaseColor, in Colorf TakeHue, in float fBlendAlpha) {
			var baseHSV = new ColorHSV(BaseColor);
			var takeHSV = new ColorHSV(TakeHue);
			baseHSV.h = takeHSV.h;
			baseHSV.s = MathUtil.Lerp(baseHSV.s, takeHSV.s, fBlendAlpha);
			baseHSV.v = MathUtil.Lerp(baseHSV.v, takeHSV.v, fBlendAlpha);
			return baseHSV.ConvertToRGB();
		}

	}
}
