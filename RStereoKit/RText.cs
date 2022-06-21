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
		public readonly Font Font;
		public ConcurrentDictionary<Colorf,TextStyle> StyleList = new ConcurrentDictionary<Colorf, TextStyle>();
		public SkFontLoader(Font font) {
			Font = font;
			GetTextStyle(Colorf.White);
		}

		public TextStyle GetTextStyle(Colorf color) {
			if(StyleList.TryGetValue(color,out var textStyle)) {
				return textStyle;
			}
			else {
				var style = Text.MakeStyle(Font, 1, new Color(color.r, color.g, color.b, color.a));
				StyleList.TryAdd(color, style);
				return style;
			}
		}
	}
	public class SKFont : IRFont
	{
		public RFont MainFont => new RFontRoot(new RenderFont(new SkFontLoader(Font.Default)));

		public bool CharExsets(RenderFont renderFont, char c) {
			return Text.Size(c.ToString(), ((SkFontLoader)renderFont.Fontist).GetTextStyle(Colorf.White)).v != Vec2.Zero.v;
		}

		public Vector2f TextSize(RenderFont renderFont, char c) {
			return (Vector2f)Text.Size(c.ToString(), ((SkFontLoader)renderFont.Fontist).GetTextStyle(Colorf.White)).v;
		}
	}

	public class SkRText : IRText
	{
		public void Add(string id,string v, RNumerics.Matrix p) {
			Text.Add(v, new StereoKit.Matrix(p.m));
		}

		public void Add(string id,string group, char c, RNumerics.Matrix p, Colorf color, RenderFont rFont, Vector2f textCut) {
			var textsize = Text.Size(c.ToString(), ((SkFontLoader)rFont.Fontist).GetTextStyle(Colorf.White));
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
			Text.Add(c.ToString(), StereoKit.Matrix.T((textCut == Vector2f.Zero)?new Vec3(0, - (textsize.y - 1), 0): Vec3.Zero) * (StereoKit.Matrix)p.m, textsize, StereoKit.TextFit.Clip, ((SkFontLoader)rFont.Fontist).GetTextStyle(color), TextAlign.BottomLeft, textAlien, offsetX, offsetY);
		}
	}
}
