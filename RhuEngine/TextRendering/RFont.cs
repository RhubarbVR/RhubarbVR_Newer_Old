using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.TextRendering;

using RNumerics;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RhuEngine
{
	public sealed class RText : IDisposable
	{
		public RText(RFont rFont) {
			TargetFont = rFont;
		}
		public RFont TargetFont;

		public RTexture2D texture2D;

		public event Action UpdatedTexture;

		public FontRectangle FontRectangle;

		public float AspectRatio;


		private string _text;

		public string Text
		{
			get => _text;
			set {
				if (_text != value) {
					_text = value;
					TextUpdated();
				}
			}
		}

		private void TextUpdated() {
			if (TargetFont is null) {
				throw new Exception("Need a font to Make text");
			}
			texture2D = TargetFont?.RenderText(_text);
			FontRectangle = TargetFont?.GetSizeOfText(_text) ?? new FontRectangle();
			AspectRatio = FontRectangle.Width / FontRectangle.Height;
			UpdatedTexture?.Invoke();
		}

		public void Dispose() {
			TargetFont = null;
			UpdatedTexture = null;
			texture2D?.Dispose();
			texture2D = null;
		}
	}


	public sealed class RFont
	{
		public const float FONTSIZE = 96f;
		public FontCollection Collection { get; set; }
		public TextOptions TextOptions { get; set; }

		public event Action UpdateAtlas;

		public RFont(Font mainFont, FontCollection fallBacks) {
			Collection = fallBacks;
			TextOptions = new TextOptions(mainFont) {
				Dpi = FONTSIZE,
				TextAlignment = TextAlignment.Center,
				FallbackFontFamilies = Collection.Families.ToArray(),
			};
		}


		public RFont(Font mainFont) {
			if (mainFont is null) {
				TextOptions = null;
				return;
			}
			TextOptions = new TextOptions(mainFont) {
				Dpi = FONTSIZE,
			};
		}

		public readonly List<FontAtlisPart> fontAtlisParts = new();


		public (RMaterial mit, RTexture2D texture, Vector2f bottomleft, Vector2f topright) GetGlygh(Rune rune) {
			if (TextOptions is null) {
				return (null, RTexture2D.White, Vector2f.Zero, Vector2f.Zero);
			}
			lock (this) {
				foreach (var item in fontAtlisParts) {
					var glyih = item.GetGlygh(rune);
					if (glyih != null) {
						return glyih.GetValueOrDefault();
					}
				}
				RLog.Info("Ran out of room adding another text texture");
				var fontAtlis = new FontAtlisPart(this);
				fontAtlisParts.Add(fontAtlis);
				UpdateAtlas?.Invoke();
				return fontAtlis.GetGlygh(rune).GetValueOrDefault();
			}
		}

		public FontRectangle GetSizeOfText(string text) {
			if (TextOptions is null) {
				return new FontRectangle();
			}
			lock (this) {
				return TextMeasurer.Measure(text, TextOptions);
			}
		}

		public Dictionary<Rune, FontRectangle> CachedRuneSize = new();

		public Dictionary<(Rune, Rune), float> CachedXAdvancesSize = new();

		public float GetXAdvances(Rune item, Rune nextitem) {
			if (TextOptions is null) {
				return 0f;
			}
			lock (this) {
				if (CachedXAdvancesSize.ContainsKey((item, nextitem))) {
					return CachedXAdvancesSize[(item, nextitem)];
				}
				else {
					var returnvalue = (TextMeasurer.Measure(item.ToString() + nextitem.ToString(), TextOptions).Width - TextMeasurer.MeasureBounds(nextitem.ToString(), TextOptions).Width) / (FONTSIZE * 2);
					CachedXAdvancesSize.Add((item, nextitem), returnvalue);
					return returnvalue;
				}
			}
		}

		public FontRectangle GetSizeOfRune(Rune rune) {
			if (TextOptions is null) {
				return new FontRectangle();
			}
			lock (this) {
				if (CachedRuneSize.ContainsKey(rune)) {
					return CachedRuneSize[rune];
				}
				else {
					var returnvalue = TextMeasurer.Measure(rune.ToString(), TextOptions);
					CachedRuneSize.Add(rune, returnvalue);
					return returnvalue;
				}
			}
		}


		public RTexture2D RenderText(string text) {
			if(TextOptions is null) {
				return RTexture2D.White;
			}
			lock (this) {
				var size = TextMeasurer.Measure(text, TextOptions);
				if (size == FontRectangle.Empty) {
					return RTexture2D.White;
				}
				using var img = new Image<Rgba32>((int)size.Width, (int)size.Height);
				img.Mutate(x => x.DrawText(TextOptions, text, Color.White));
				return new ImageSharpTexture(img).CreateTextureAndDisposes();
			}
		}


	}
}
