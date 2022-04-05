using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using StereoKit;
using RhuEngine;
using RhuEngine.Linker;
using RNumerics;
using System.Numerics;

namespace RStereoKit
{
	public class SkFontLoader
	{
		public Font Regular { get; private set; }
		public Font Bold { get; private set; }
		public Font Italic { get; private set; }
		public Font Oblique { get; private set; }
		public Font Strikeout { get; private set; }
		public Font Underline { get; private set; }

		public ConcurrentDictionary<(Colorf,FontStyle),TextStyle> StyleList = new ConcurrentDictionary<(Colorf, FontStyle), TextStyle>();
		public SkFontLoader(Font font) {
			Regular = font;
			GetTextStyle(Colorf.White,FontStyle.Regular);
		}

		public TextStyle GetTextStyle(Colorf color,FontStyle fontStyle) {
			if(StyleList.TryGetValue((color, fontStyle),out var textStyle)) {
				return textStyle;
			}
			else {
				var Font = Regular;
				switch (fontStyle) {
					case FontStyle.Regular:
						Font = Regular ?? Font.Default;
						break;
					case FontStyle.Bold:
						Font = Bold ?? Regular;
						break;
					case FontStyle.Italic:
						Font = Italic ?? Font.Default;
						break;
					case FontStyle.oblique:
						Font = Oblique ?? Font.Default;
						break;
					case FontStyle.Strikeout:
						Font = Strikeout ?? Font.Default;
						break;
					case FontStyle.Underline:
						Font = Underline ?? Font.Default;
						break;
					default:
						break;
				}
				var style = Text.MakeStyle(Font, 1, new Color(color.r, color.g, color.b, color.a));
				StyleList.TryAdd((color, fontStyle), style);
				return style;
			}
		}
	}
	public class SKFont : IRFont
	{
		public RFont Default => new RFont(new SkFontLoader(Font.Default));
	}

	public class SkRText : IRText
	{
		public void Add(string id,string v, RNumerics.Matrix p) {
			Text.Add(v, new StereoKit.Matrix(p.m));
		}

		public void Add(string id, char c, RNumerics.Matrix p, Colorf color, RFont rFont,FontStyle fontStyle, Vector2f textCut) {
			Text.Add(c.ToString(), p.m, (Vec2)(Vector2)textCut, StereoKit.TextFit.Clip, ((SkFontLoader)rFont.Instances).GetTextStyle(color, fontStyle),StereoKit.TextAlign.BottomLeft, StereoKit.TextAlign.BottomLeft);
		}

		public Vector2f Size(RFont rFont, char c,FontStyle fontStyle) {
			return (Vector2f)Text.Size(c.ToString(), ((SkFontLoader)rFont.Instances).GetTextStyle(Colorf.White, fontStyle)).v;
		}
	}
}
