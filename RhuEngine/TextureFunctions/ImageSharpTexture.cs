using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RhuEngine.Linker;
using RNumerics;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RhuEngine
{
	public sealed class ImageSharpTexture : IDisposable
	{
		public Image<Rgba32> Image { get; private set; }
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
		public RImageTexture2D Texture2D { get; private set; }
		public RImage RImage { get; private set; }

		public unsafe RTexture2D CreateTexture() {
			var colors = new Colorb[Height * Width];
			var hanndel = GCHandle.Alloc(colors, GCHandleType.Pinned);
			var pin = hanndel.AddrOfPinnedObject();
			Parallel.For(0, colors.Length, (i) => {
				var w = i % Width;
				var h = i / Width;
				var color = Image[w, h];
				((Rgba32*)pin)[i] = color;
			});
			hanndel.Free();
			RImage = new RImage(null);
			RImage.Create(Width, Height, true, RFormat.Rgba8);
			RImage.SetColors(Width, Height, colors);
			Texture2D = new RImageTexture2D(RImage);
			return Texture2D;
		}
		public unsafe RTexture2D UpdateTexture() {
			if (Texture2D is null) {
				throw new Exception("Not started");
			}
			var colors = new Colorb[Height * Width];
			var hanndel = GCHandle.Alloc(colors, GCHandleType.Pinned);
			var pin = hanndel.AddrOfPinnedObject();
			Parallel.For(0, colors.Length, (i) => {
				var w = i % Width;
				var h = i / Height;
				var color = Image[w, h];
				((Rgba32*)pin)[i] = color;
			});
			hanndel.Free();
			RImage.SetColors(Width, Height, colors);
			Texture2D.UpdateImage(RImage);
			return Texture2D;
		}

		public void UpdateImage(Image<Rgba32> image) {
			Image?.Dispose();
			Image = image;
		}

		public RTexture2D CreateTextureAndDisposes() {
			var newtex = CreateTexture();
			Dispose();
			RImage.Dispose();
			return newtex;
		}
		public void Dispose() {
			Image?.Dispose();
		}
	}
}
