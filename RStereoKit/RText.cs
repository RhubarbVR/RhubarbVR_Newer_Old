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
		public Font Font { get; private set; }

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
		public RFont Default => new RFont(new SkFontLoader(Font.Default));
	}

	public class SkRText : IRText
	{
		public void Add(string id,string v, RNumerics.Matrix p) {
			Text.Add(v, new StereoKit.Matrix(p.m));
		}

		public void Add(string id, char c, RNumerics.Matrix p, Colorf color, RFont rFont, Vector2f textCut) {
			Text.Add(c.ToString(), p.m, (Vec2)(Vector2)textCut, StereoKit.TextFit.Clip, ((SkFontLoader)rFont.Instances).GetTextStyle(color),StereoKit.TextAlign.BottomLeft, StereoKit.TextAlign.BottomLeft);
		}

		public Vector2f Size(RFont rFont, char c) {
			return (Vector2f)Text.Size(c.ToString(), ((SkFontLoader)rFont.Instances).GetTextStyle(Colorf.White)).v;
		}
	}
}
