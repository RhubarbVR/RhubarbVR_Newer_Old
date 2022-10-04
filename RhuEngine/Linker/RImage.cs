using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public enum RFormat : long
	{
		//
		// Summary:
		//     Texture format with a single 8-bit depth representing luminance.
		L8,
		//
		// Summary:
		//     OpenGL texture format with two values, luminance and alpha each stored with 8
		//     bits.
		La8,
		//
		// Summary:
		//     OpenGL texture format RED with a single component and a bitdepth of 8.
		R8,
		//
		// Summary:
		//     OpenGL texture format RG with two components and a bitdepth of 8 for each.
		Rg8,
		//
		// Summary:
		//     OpenGL texture format RGB with three components, each with a bitdepth of 8.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Rgb8,
		//
		// Summary:
		//     OpenGL texture format RGBA with four components, each with a bitdepth of 8.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Rgba8,
		//
		// Summary:
		//     OpenGL texture format RGBA with four components, each with a bitdepth of 4.
		Rgba4444,
		Rgb565,
		//
		// Summary:
		//     OpenGL texture format GL_R32F where there's one component, a 32-bit floating-point
		//     value.
		Rf,
		//
		// Summary:
		//     OpenGL texture format GL_RG32F where there are two components, each a 32-bit
		//     floating-point values.
		Rgf,
		//
		// Summary:
		//     OpenGL texture format GL_RGB32F where there are three components, each a 32-bit
		//     floating-point values.
		Rgbf,
		//
		// Summary:
		//     OpenGL texture format GL_RGBA32F where there are four components, each a 32-bit
		//     floating-point values.
		Rgbaf,
		//
		// Summary:
		//     OpenGL texture format GL_R32F where there's one component, a 16-bit "half-precision"
		//     floating-point value.
		Rh,
		//
		// Summary:
		//     OpenGL texture format GL_RG32F where there are two components, each a 16-bit
		//     "half-precision" floating-point value.
		Rgh,
		//
		// Summary:
		//     OpenGL texture format GL_RGB32F where there are three components, each a 16-bit
		//     "half-precision" floating-point value.
		Rgbh,
		//
		// Summary:
		//     OpenGL texture format GL_RGBA32F where there are four components, each a 16-bit
		//     "half-precision" floating-point value.
		Rgbah,
		//
		// Summary:
		//     A special OpenGL texture format where the three color components have 9 bits
		//     of precision and all three share a single 5-bit exponent.
		Rgbe9995,
		//
		// Summary:
		//     The S3TC texture format that uses Block Compression 1, and is the smallest variation
		//     of S3TC, only providing 1 bit of alpha and color data being premultiplied with
		//     alpha.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Dxt1,
		//
		// Summary:
		//     The S3TC texture format that uses Block Compression 2, and color data is interpreted
		//     as not having been premultiplied by alpha. Well suited for images with sharp
		//     alpha transitions between translucent and opaque areas.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Dxt3,
		//
		// Summary:
		//     The S3TC texture format also known as Block Compression 3 or BC3 that contains
		//     64 bits of alpha channel data followed by 64 bits of DXT1-encoded color data.
		//     Color data is not premultiplied by alpha, same as DXT3. DXT5 generally produces
		//     superior results for transparent gradients compared to DXT3.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Dxt5,
		//
		// Summary:
		//     Texture format that uses Red Green Texture Compression, normalizing the red channel
		//     data using the same compression algorithm that DXT5 uses for the alpha channel.
		RgtcR,
		//
		// Summary:
		//     Texture format that uses Red Green Texture Compression, normalizing the red and
		//     green channel data using the same compression algorithm that DXT5 uses for the
		//     alpha channel.
		RgtcRg,
		//
		// Summary:
		//     Texture format that uses BPTC compression with unsigned normalized RGBA components.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		BptcRgba,
		//
		// Summary:
		//     Texture format that uses BPTC compression with signed floating-point RGB components.
		BptcRgbf,
		//
		// Summary:
		//     Texture format that uses BPTC compression with unsigned floating-point RGB components.
		BptcRgbfu,
		//
		// Summary:
		//     Ericsson Texture Compression format 1, also referred to as "ETC1", and is part
		//     of the OpenGL ES graphics standard. This format cannot store an alpha channel.
		Etc,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (R11_EAC variant), which provides one channel
		//     of unsigned data.
		Etc2R11,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (SIGNED_R11_EAC variant), which provides
		//     one channel of signed data.
		Etc2R11s,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (RG11_EAC variant), which provides two
		//     channels of unsigned data.
		Etc2Rg11,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (SIGNED_RG11_EAC variant), which provides
		//     two channels of signed data.
		Etc2Rg11s,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (RGB8 variant), which is a follow-up of
		//     ETC1 and compresses RGB888 data.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Etc2Rgb8,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (RGBA8variant), which compresses RGBA8888
		//     data with full alpha support.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Etc2Rgba8,
		//
		// Summary:
		//     Ericsson Texture Compression format 2 (RGB8_PUNCHTHROUGH_ALPHA1 variant), which
		//     compresses RGBA data to make alpha either fully transparent or fully opaque.
		//     Note: When creating an Godot.ImageTexture, an sRGB to linear color space conversion
		//     is performed.
		Etc2Rgb8a1,
		Etc2RaAsRg,
		Dxt5RaAsRg,
		//
		// Summary:
		//     Represents the size of the Godot.Image.Format enum.
		Max
	}

	public enum RInterpolation : long
	{
		//
		// Summary:
		//     Performs nearest-neighbor interpolation. If the image is resized, it will be
		//     pixelated.
		Nearest,
		//
		// Summary:
		//     Performs bilinear interpolation. If the image is resized, it will be blurry.
		//     This mode is faster than Godot.Image.Interpolation.Cubic, but it results in lower
		//     quality.
		Bilinear,
		//
		// Summary:
		//     Performs cubic interpolation. If the image is resized, it will be blurry. This
		//     mode often gives better results compared to Godot.Image.Interpolation.Bilinear,
		//     at the cost of being slower.
		Cubic,
		//
		// Summary:
		//     Performs bilinear separately on the two most-suited mipmap levels, then linearly
		//     interpolates between them.
		//     It's slower than Godot.Image.Interpolation.Bilinear, but produces higher-quality
		//     results with far fewer aliasing artifacts.
		//     If the image does not have mipmaps, they will be generated and used internally,
		//     but no mipmaps will be generated on the resulting image.
		//     Note: If you intend to scale multiple copies of the original image, it's better
		//     to call Godot.Image.GenerateMipmaps(System.Boolean)] on it in advance, to avoid
		//     wasting processing power in generating them again and again.
		//     On the other hand, if the image already has mipmaps, they will be used, and a
		//     new set will be generated for the resulting image.
		Trilinear,
		//
		// Summary:
		//     Performs Lanczos interpolation. This is the slowest image resizing mode, but
		//     it typically gives the best results, especially when downscalng images.
		Lanczos
	}

	public enum RAlphaMode : long
	{
		//
		// Summary:
		//     Image does not have alpha.
		None,
		//
		// Summary:
		//     Image stores alpha in a single bit.
		Bit,
		//
		// Summary:
		//     Image uses alpha.
		Blend
	}

	public enum RCompressMode : long
	{
		//
		// Summary:
		//     Use S3TC compression.
		S3tc,
		//
		// Summary:
		//     Use ETC compression.
		Etc,
		//
		// Summary:
		//     Use ETC2 compression.
		Etc2,
		//
		// Summary:
		//     Use BPTC compression.
		Bptc
	}

	public enum RUsedChannels : long
	{
		L,
		La,
		R,
		Rg,
		Rgb,
		Rgba
	}

	public enum RCompressSource : long
	{
		//
		// Summary:
		//     Source texture (before compression) is a regular texture. Default for all textures.
		Generic,
		//
		// Summary:
		//     Source texture (before compression) is in sRGB space.
		Srgb,
		//
		// Summary:
		//     Source texture (before compression) is a normal texture (e.g. it can be compressed
		//     into two channels).
		Normal
	}


	public interface IRImage : IDisposable
	{
		int Width { get; }

		int Height { get; }

		bool HasMipmaps { get; }

		RFormat Format { get; }

		byte[] RawData();

		void Convert(RFormat rFormat);

		int GetMipmapOffset(int mipmap);

		void ResizePowerOfTwo(bool square, RInterpolation rInterpolation);

		void Resize(int width, int height, RInterpolation rInterpolation);

		void ShrinkFactorTwo();

		void Crop(int width, int height);

		void FlipX();
		void FlipY();

		bool GenerateMipmaps(bool renormalize);

		void ClearMipmaps();

		void Create(int width, int height, bool mipmaps, RFormat format);
		void CreateWithData(int width, int height, bool mipmaps, RFormat format, byte[] data);

		bool IsEmpty { get; }

		byte[] SaveJpg(float quality);

		byte[] SavePng();
		byte[] SaveExr(bool grayScale);
		byte[] SaveWebp(bool lossy, float quality);

		RAlphaMode DetectAlpha();

		bool IsInvisible { get; }

		RUsedChannels FetectUsedChannels(RCompressSource source);

		void Compress(RCompressMode rCompressMode, RCompressSource source, float quality);
		void CompressFromChannels(RCompressMode rCompressMode, RUsedChannels rUsedChannels, float quality);

		void Decompress();

		bool IsCompressed { get; }

		void Rotate90(RClockDirection rClockDirection);
		void Rotate180();
		void FixAlphaEdges();
		void PremultiplyAlpha();
		void SrgbToLinear();
		void NormalMapToXy();
		IRImage RgbeToSrgb();

		void BumpMapToNormalMap(float bumpScale);

		void Fill(Colorf color);
		void FillRect(Vector2i pos, Vector2i size, Colorf color);

		(Vector2i pos, Vector2i size) GetUsedRect();
		IRImage GetRect(Vector2i pos, Vector2i size);
		IRImage Copy();

		Colorf GetPixel(int x, int y);
		void SetPixel(int x, int y, Colorf color);
		void AdjustBcs(float brightness, float contrast, float saturation);

		bool LoadPng(byte[] data);

		bool LoadJpg(byte[] data);
		bool LoadWebp(byte[] data);

		bool LoadTga(byte[] data);
		bool LoadBmp(byte[] data);

		void Init(RImage rTexture2D);

	}

	public class RImage : IDisposable
	{
		public static Type Instance { get; set; }

		public IRImage Inst;


		public int Width => Inst.Width;

		public int Height => Inst.Height;

		public bool HasMipmaps => Inst.HasMipmaps;

		public RFormat Format => Inst.Format;

		public byte[] RawData() {
			return Inst.RawData();
		}

		public void Convert(RFormat rFormat) {
			Inst.Convert(rFormat);
		}

		public int GetMipmapOffset(int mipmap) {
			return Inst.GetMipmapOffset(mipmap);
		}

		public void ResizePowerOfTwo(bool square = false, RInterpolation rInterpolation = RInterpolation.Bilinear) {
			Inst.ResizePowerOfTwo(square, rInterpolation);
		}

		public void Resize(int width, int height, RInterpolation rInterpolation = RInterpolation.Bilinear) {
			Inst.Resize(width, height, rInterpolation);
		}

		public void ShrinkFactorTwo() {
			Inst.ShrinkFactorTwo();
		}

		public void Crop(Vector2i size) {
			Inst.Crop(size.x, size.y);
		}

		public void Crop(int width, int height) {
			Inst.Crop(width, height);
		}

		public void FlipX() {
			Inst.FlipX();
		}
		public void FlipY() {
			Inst.FlipY();
		}

		public bool GenerateMipmaps(bool renormalize = false) {
			return Inst.GenerateMipmaps(renormalize);
		}

		public void ClearMipmaps() {
			Inst.ClearMipmaps();
		}

		public void Create(int width, int height, bool mipmaps, RFormat format) {
			Inst.Create(Math.Max(2,width), Math.Max(2, height), mipmaps, format);
		}
		public void CreateWithData(int width, int height, bool mipmaps, RFormat format, byte[] data) {
			Inst.CreateWithData(width, height, mipmaps, format, data);
		}

		public bool IsEmpty => Inst.IsEmpty;

		public byte[] SaveJpg(float quality = 0.75f) {
			return Inst.SaveJpg(quality);
		}

		public byte[] SavePng() {
			return Inst.SavePng();
		}
		public byte[] SaveExr(bool grayScale = false) {
			return Inst.SaveExr(grayScale);
		}
		public byte[] SaveWebp(bool lossy = false, float quality = 0.75f) {
			return Inst.SaveWebp(lossy, quality);
		}

		public RAlphaMode DetectAlpha() {
			return Inst.DetectAlpha();
		}

		public bool IsInvisible => Inst.IsInvisible;

		public RUsedChannels FetectUsedChannels(RCompressSource source = RCompressSource.Generic) {
			return Inst.FetectUsedChannels(source);
		}

		public void Compress(RCompressMode rCompressMode, RCompressSource source = RCompressSource.Generic, float quality = 0.7f) {
			Inst.Compress(rCompressMode, source, quality);
		}
		public void CompressFromChannels(RCompressMode rCompressMode, RUsedChannels rUsedChannels, float quality = 0.7f) {
			Inst.CompressFromChannels(rCompressMode, rUsedChannels, quality);
		}

		public void Decompress() {
			Inst.Decompress();
		}

		public bool IsCompressed => Inst.IsCompressed;

		public void Rotate90(RClockDirection rClockDirection) {
			Inst.Rotate90(rClockDirection);
		}
		public void Rotate180() {
			Inst.Rotate180();
		}
		public void FixAlphaEdges() {
			Inst.FixAlphaEdges();
		}
		public void PremultiplyAlpha() {
			Inst.PremultiplyAlpha();
		}
		public void SrgbToLinear() {
			Inst.SrgbToLinear();
		}
		public void NormalMapToXy() {
			Inst.NormalMapToXy();
		}
		public RImage RgbeToSrgb() {
			return new RImage(Inst.RgbeToSrgb());
		}

		public void BumpMapToNormalMap(float bumpScale = 1f) {
			Inst.BumpMapToNormalMap(bumpScale);
		}

		public void Fill(Colorf color) {
			Inst.Fill(color);
		}


		public void FillRect(Vector2i pos, Vector2i size, Colorf color) {
			Inst.FillRect(pos, size, color);
		}

		public (Vector2i pos, Vector2i size) GetUsedRect() {
			return Inst.GetUsedRect();
		}

		public RImage GetRect(Vector2i pos, Vector2i size) {
			return new RImage(Inst.GetRect(pos, size));
		}
		public RImage Copy() {
			return new RImage(Inst.Copy());
		}
		public Colorf GetPixel(Vector2i pos) {
			return Inst.GetPixel(pos.x, pos.y);
		}
		public Colorf GetPixel(int x, int y) {
			return Inst.GetPixel(x, y);
		}

		public void SetPixel(Vector2i pos, Colorf color) {
			Inst.SetPixel(pos.x, pos.y, color);
		}

		public void SetPixel(int x, int y, Colorf color) {
			Inst.SetPixel(x, y, color);
		}
		public void AdjustBcs(float brightness, float contrast, float saturation) {
			Inst.AdjustBcs(brightness, contrast, saturation);
		}

		public bool LoadPng(byte[] data) {
			return Inst.LoadPng(data);
		}

		public bool LoadJpg(byte[] data) {
			return Inst.LoadJpg(data);
		}
		public bool LoadWebp(byte[] data) {
			return Inst.LoadWebp(data);
		}

		public bool LoadTga(byte[] data) {
			return Inst.LoadTga(data);
		}
		public bool LoadBmp(byte[] data) {
			return Inst.LoadBmp(data);
		}


		public RImage(IRImage tex) {
			Inst = tex ?? (IRImage)Activator.CreateInstance(Instance);
			Inst.Init(this);
		}

		public void Dispose() {
			Inst.Dispose();
			Inst = null;
		}
		public void SetColors(int width, int height, Colorf[] colors) {
			if (width * height != colors.Length) {
				throw new InvalidOperationException("Colors have to many or to little");
			}

			Resize(width, height);
			for (var x = 0; x < width; x++) {
				for (var y = 0; y < height; y++) {
					var colorVal = x + (y * width);
					SetPixel(x, y, colors[colorVal]);
				}
			}
		}
		public void SetColors(int width, int height, Colorb[] colors,bool RebuildMipmaps = true) {
			if (width * height != colors.Length) {
				throw new InvalidOperationException("Colors have to many or to little");
			}
			if (width != Width || height != Height) {
				Resize(width, height);
			}
			for (var y = 0; y < Height; y++) {
				for (var x = 0; x < Width; x++) {
					var colorVal = x + (y * width);
					SetPixel(x, y, new Colorf(colors[colorVal].r, colors[colorVal].g, colors[colorVal].b, colors[colorVal].a));
				}
			}
			if (HasMipmaps && RebuildMipmaps) {
				GenerateMipmaps();
			}
		}
	}
}
