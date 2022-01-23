using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using StereoKit;

namespace RhuEngine
{
	public class ImageSharpTexture
	{
		public Image<Rgba32> Image { get; }
		public bool Srgb { get; }

		public int Width => Image.Width;

		public int Height => Image.Height;

		public ImageSharpTexture(string path) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(path)) { }
		public ImageSharpTexture(string path, bool srgb) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(path), srgb) { }
		public ImageSharpTexture(Stream stream) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(stream)) { }
		public ImageSharpTexture(Stream stream, bool srgb) : this(SixLabors.ImageSharp.Image.Load<Rgba32>(stream), srgb) { }
		public ImageSharpTexture(Image<Rgba32> image) : this(image, false) { }
		public ImageSharpTexture(Image<Rgba32> image, bool srgb) {
			Image = image;
			Srgb = srgb;
		}

		public Tex CreateTexture() {
			var colors = new Color32[Height * Width];
			for (var h = 0; h < Height; h++) {
				for (var w = 0; w < Width; w++) {
					var color = Image[w, h];
					colors[w + (h * Width)] = new Color32(color.R, color.G, color.B, color.A);
				}
			}
			return Tex.FromColors(colors, Width, Height, Srgb);
		}
	}
}
