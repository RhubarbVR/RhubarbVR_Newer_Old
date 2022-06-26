using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RhuEngine.Linker;
using RNumerics;
using System;

namespace RhuEngine
{
	public class ImageSharpTexture : IDisposable
	{
		public Image<Rgba32> Image { get; }
		public bool Srgb { get; }

		public int Width => Image.Width;

		public int Height => Image.Height;

		public ImageSharpTexture(string path) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(path)) { }
		public ImageSharpTexture(string path, bool srgb) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(path), srgb) { }
		public ImageSharpTexture(Stream stream) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(stream)) { }
		public ImageSharpTexture(Stream stream, bool srgb) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(stream), srgb) { }
		public ImageSharpTexture(Image<Rgba32> image) : this(image, true) { }
		public ImageSharpTexture(Image<Rgba32> image, bool srgb) {
			Image = image;
			Srgb = srgb;
		}

		public RTexture2D CreateTexture() {
			var colors = new Colorb[Height * Width];
			for (var h = 0; h < Height; h++) {
				for (var w = 0; w < Width; w++) {
					var color = Image[w, h];
					colors[w + (h * Width)] = new Colorb(color.R, color.G, color.B, color.A);
				}
			}
			var newtex = RTexture2D.FromColors(colors, Width, Height, Srgb);
			return newtex;
		}

		public RTexture2D CreateTextureAndDisposes() {
			var newtex = CreateTexture();
			Dispose();
			return newtex;
		}
		public void Dispose() {
			Image?.Dispose();
		}
	}
}
