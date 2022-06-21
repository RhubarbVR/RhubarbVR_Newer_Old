using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public static class FontManager {
		public static RenderFont GetFont(RFont rFont, Rune c, FontStyle fontStyle) {
			foreach (var item in rFont.GetFallBackArray()) {
				var font = item.GetFontFromStyle(fontStyle);
				if (RFont.inst.CharExsets(font, c)) {
					return font;
				}
			}
			return null;
		}

		public static Vector2f Size(RFont rFont, Rune c, FontStyle fontStyle) {
			foreach (var item in rFont.GetFallBackArray()) {
				var font = item.GetFontFromStyle(fontStyle);
				if(RFont.inst.CharExsets(font, c)) {
					return RFont.inst.TextSize(font, c);
				}
			}
			return Vector2f.Zero;
		}
	}
	public interface IRFont
	{
		public RFont MainFont { get; }

		public Vector2f TextSize(RenderFont renderFont, Rune c);

		public bool CharExsets(RenderFont renderFont, Rune c);
	}

	public abstract class RFont
	{
		public static IRFont inst;

		public static RFont MainFont => inst.MainFont;

		public abstract IEnumerable<RFontStyleManager> GetFallBackArray();
	}



	public class RenderFont
	{
		public object Fontist;

		public RenderFont(object fontist) {
			Fontist = fontist;
		}
	}
	public enum FontStyle
	{
		Black,
		BlackItalic,
		Bold,
		BoldItalic,
		ExtraBold,
		ExtraBoldItalic,
		ExtraLight,
		ExtraLightItalic,
		Italic,
		Light,
		LightItalic,
		Medium,
		MediumItalic,
		Regular,
		SemiBold,
		SemiBoldItalic,
		Thin,
		ThinItalic,
		Oblique,
		Strikeout,
		Underline,
		UnderlineItalic,
		UnderlineBold,
		UnderlineLight,
		UnderlineLightItalic,
	}

	public class RFontStyleManager
	{
		public RenderFont GetFontFromStyle(FontStyle fontStyle) {
			return fontStyle switch {
				FontStyle.Black => Black ?? Regular,
				FontStyle.BlackItalic => BlackItalic ?? Regular,
				FontStyle.Bold => Bold ?? Regular,
				FontStyle.BoldItalic => BoldItalic ?? Regular,
				FontStyle.ExtraBold => ExtraBold ?? Regular,
				FontStyle.ExtraBoldItalic => ExtraBoldItalic ?? Regular,
				FontStyle.ExtraLight => ExtraLight ?? Regular,
				FontStyle.ExtraLightItalic => ExtraLightItalic ?? Regular,
				FontStyle.Italic => Italic ?? Regular,
				FontStyle.Light => Light ?? Regular,
				FontStyle.LightItalic => LightItalic ?? Regular,
				FontStyle.Medium => Medium ?? Regular,
				FontStyle.MediumItalic => MediumItalic ?? Regular,
				FontStyle.Regular => Regular ?? Regular,
				FontStyle.SemiBold => SemiBold ?? Regular,
				FontStyle.SemiBoldItalic => SemiBoldItalic ?? Regular,
				FontStyle.Thin => Thin ?? Regular,
				FontStyle.ThinItalic => ThinItalic ?? Regular,
				FontStyle.Oblique => Oblique ?? Regular,
				FontStyle.Strikeout => Strikeout ?? Regular,
				FontStyle.Underline => Underline ?? Regular,
				FontStyle.UnderlineItalic => UnderlineItalic ?? Regular,
				FontStyle.UnderlineBold => UnderlineBold ?? Regular,
				FontStyle.UnderlineLight => UnderlineLight ?? Regular,
				FontStyle.UnderlineLightItalic => UnderlineLightItalic ?? Regular,
				_ => Regular,
			};
		}

		public RenderFont Black;

		public RenderFont BlackItalic;

		public RenderFont Bold;

		public RenderFont BoldItalic;

		public RenderFont ExtraBold;

		public RenderFont ExtraBoldItalic;

		public RenderFont ExtraLight;

		public RenderFont ExtraLightItalic;

		public RenderFont Italic;

		public RenderFont Light;

		public RenderFont LightItalic;

		public RenderFont Medium;

		public RenderFont MediumItalic;

		public RenderFont Regular;

		public RenderFont SemiBold;

		public RenderFont SemiBoldItalic;

		public RenderFont Thin;

		public RenderFont ThinItalic;

		public RenderFont Oblique;

		public RenderFont Strikeout;

		public RenderFont Underline;

		public RenderFont UnderlineItalic;

		public RenderFont UnderlineBold;
		
		public RenderFont UnderlineLight;

		public RenderFont UnderlineLightItalic;

		public RFontStyleManager(RenderFont fontInst) {
			Regular = fontInst;
		}
	}

	public class RFontRoot : RFont
	{

		public RFontStyleManager FontInst;

		public override IEnumerable<RFontStyleManager> GetFallBackArray() {
			yield return FontInst;
		}

		public RFontRoot(RFontStyleManager fontInst) {
			FontInst = fontInst;
		}

		public RFontRoot(RenderFont fontInst) {
			FontInst = new RFontStyleManager(fontInst);
		}
	}

	public class RFontFallBack : RFont
	{
		public List<RFont> FallBacks = new List<RFont>();

		public override IEnumerable<RFontStyleManager> GetFallBackArray() {
			foreach (var item in FallBacks) {
				foreach (var fontinst in item.GetFallBackArray()) {
					yield return fontinst;
				}
			}
		}
	}
}
