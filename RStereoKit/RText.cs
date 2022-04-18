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
			var textsize = (Vec2)(Vector2)Size(rFont, c, fontStyle);
			var textAlien = TextAlign.BottomLeft;
			var offsetX = 0f;
			var offsetY = 0f;
			if (textCut != Vector2f.Zero) {
				textAlien = TextAlign.BottomLeft;
				if (textCut.x > 0) {
					textsize.v.X -= textCut.x;
				}
				else {
					textsize.v.X += textCut.x;
					textAlien = TextAlign.TopRight;
				}
				if (textCut.y > 0) {
					offsetY = textCut.y;
					textsize.v.Y -= textCut.y;
				}
				else {
					textsize.v.Y += textCut.y;
					if(textAlien == TextAlign.TopRight) {
						textAlien = TextAlign.BottomRight;
					}
					if (textAlien == TextAlign.TopLeft) {
						textAlien = TextAlign.BottomLeft;
					}
				}
			}
			else {
				textAlien = TextAlign.TopLeft;
				textsize += new Vec2(1);
			}
			Text.Add(c.ToString(), StereoKit.Matrix.T((textCut == Vector2f.Zero)?new Vec3(0, - (textsize.y - 1), 0): Vec3.Zero) * (StereoKit.Matrix)p.m, textsize, StereoKit.TextFit.Clip, ((SkFontLoader)rFont.Instances).GetTextStyle(color, fontStyle), TextAlign.BottomLeft, textAlien, offsetX, offsetY);
		}

		public Vector2f Size(RFont rFont, char c,FontStyle fontStyle) {
			return (Vector2f)Text.Size(c.ToString(), ((SkFontLoader)rFont.Instances).GetTextStyle(Colorf.White, fontStyle)).v;
		}
	}
}
