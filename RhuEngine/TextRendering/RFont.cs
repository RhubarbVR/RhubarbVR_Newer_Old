using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RhuEngine
{
	public class RText
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
			texture2D = TargetFont.RenderText(_text);
			FontRectangle = TargetFont.GetSizeOfText(_text);
			AspectRatio = FontRectangle.Width / FontRectangle.Height;
			UpdatedTexture?.Invoke();
		}
	}


	public class RFont
	{
		public const float FONTSIZE = 96f;
		public FontCollection Collection { get; set; }
		public TextOptions TextOptions { get; set; }
		public RFont(Font mainFont,FontCollection fallBacks) {
			Collection = fallBacks;
			TextOptions = new TextOptions(mainFont) {
				Dpi = FONTSIZE,
				FallbackFontFamilies = Collection.Families.ToArray(),
			};
		}
		public RFont(Font mainFont) {
			TextOptions = new TextOptions(mainFont) {
				Dpi = 96,
			};
		}

		public readonly List<Rune> LoadedRunes = new();

		public FontRectangle GetSizeOfText(string text) {
			return TextMeasurer.Measure(text, TextOptions);
		}

		public RTexture2D RenderText(string text) {
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
