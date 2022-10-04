using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Linker;

using RNumerics;

using static Godot.Image;

using Vector2i = RNumerics.Vector2i;

namespace RhubarbVR.Bindings.TextureBindings
{
	public class GodotImage : IRImage
	{
		public GodotImage(Image image) {
			Image = image;
		}
		public GodotImage() {

		}

		public Image Image { get; set; }

		public RImage RImage { get; set; }
		public void Dispose() {
			Image.Free();
		}
		public void Init(RImage rTexture2D) {
			RImage = rTexture2D;
			Image ??= new Image();
		}

		public int Width => Image.GetWidth();

		public int Height => Image.GetHeight();

		public bool HasMipmaps => Image.HasMipmaps();

		public RFormat Format => (RFormat)Image.GetFormat();

		public bool IsEmpty => Image.IsEmpty();

		public bool IsInvisible => Image.IsInvisible();

		public bool IsCompressed => Image.IsCompressed();

		public void AdjustBcs(float brightness, float contrast, float saturation) {
			Image.AdjustBcs(brightness, contrast, saturation);
		}

		public void BumpMapToNormalMap(float bumpScale) {
			Image.BumpMapToNormalMap(bumpScale);
		}

		public void ClearMipmaps() {
			Image.ClearMipmaps();
		}

		public void Compress(RCompressMode rCompressMode, RCompressSource source, float quality) {
			Image.Compress((Image.CompressMode)rCompressMode, (Image.CompressSource)source, quality);
		}

		public void CompressFromChannels(RCompressMode rCompressMode, RUsedChannels rUsedChannels, float quality) {
			Image.CompressFromChannels((Image.CompressMode)rCompressMode, (Image.UsedChannels)rUsedChannels, quality);
		}

		public void Convert(RFormat rFormat) {
			Image.Convert((Image.Format)rFormat);
		}

		public IRImage Copy() {
			var image = new Image();
			image.CopyFrom(Image);
			return new GodotImage(image);
		}

		public void Create(int width, int height, bool mipmaps, RFormat format) {
			Image.Create(width, height, mipmaps, (Image.Format)format);
		}

		public void CreateWithData(int width, int height, bool mipmaps, RFormat format, byte[] data) {
			Image.CreateFromData(width, height, mipmaps, (Image.Format)format, data);
		}

		public void Crop(int width, int height) {
			Image.Crop(width, height);
		}

		public void Decompress() {
			Image.Decompress();
		}

		public RAlphaMode DetectAlpha() {
			return (RAlphaMode)Image.DetectAlpha();
		}

		public RUsedChannels FetectUsedChannels(RCompressSource source) {
			return (RUsedChannels)Image.DetectUsedChannels((Image.CompressSource)source);
		}

		public void Fill(Colorf color) {
			Image.Fill(new Color(color.r, color.g, color.b, color.a));
		}

		public void FillRect(Vector2i rectPos, Vector2i rectSiz, Colorf color) {
			Image.FillRect(new Rect2i(rectPos.x, rectPos.y, rectSiz.x, rectSiz.y), new Color(color.r, color.g, color.b, color.a));
		}

		public void FixAlphaEdges() {
			Image.FixAlphaEdges();
		}

		public void FlipX() {
			Image.FlipX();
		}

		public void FlipY() {
			Image.FlipY();
		}

		public bool GenerateMipmaps(bool renormalize) {
			return Image.GenerateMipmaps(renormalize) == Error.Ok;
		}

		public int GetMipmapOffset(int mipmap) {
			return Image.GetMipmapOffset(mipmap);
		}

		public Colorf GetPixel(int x, int y) {
			var pixel = Image.GetPixel(x, y);
			return new Colorf(pixel.r, pixel.g, pixel.b, pixel.a);
		}

		public IRImage GetRect(Vector2i rectPos, Vector2i rectSiz) {
			return new GodotImage(Image.GetRect(new Rect2i(rectPos.x, rectPos.y, rectSiz.x, rectSiz.y)));
		}

		public (Vector2i pos, Vector2i size) GetUsedRect() {
			var rect = Image.GetUsedRect();
			return (new Vector2i(rect.Position.x, rect.Position.y), new Vector2i(rect.Size.x, rect.Size.y));
		}

		public bool LoadBmp(byte[] data) {
			return Image.LoadBmpFromBuffer(data) == Error.Ok;
		}

		public bool LoadJpg(byte[] data) {
			return Image.LoadJpgFromBuffer(data) == Error.Ok;
		}

		public bool LoadPng(byte[] data) {
			return Image.LoadPngFromBuffer(data) == Error.Ok;
		}

		public bool LoadTga(byte[] data) {
			return Image.LoadTgaFromBuffer(data) == Error.Ok;
		}

		public bool LoadWebp(byte[] data) {
			return Image.LoadWebpFromBuffer(data) == Error.Ok;
		}

		public void NormalMapToXy() {
			Image.NormalMapToXy();
		}

		public void PremultiplyAlpha() {
			Image.PremultiplyAlpha();
		}

		public byte[] RawData() {
			return Image.GetData();
		}

		public void Resize(int width, int height, RInterpolation rInterpolation) {
			Image.Resize(width, height, (Interpolation)rInterpolation);
		}

		public void ResizePowerOfTwo(bool square, RInterpolation rInterpolation) {
			Image.ResizeToPo2(square, (Interpolation)rInterpolation);
		}

		public IRImage RgbeToSrgb() {
			return new GodotImage(Image.RgbeToSrgb());
		}

		public void Rotate180() {
			Image.Rotate180();
		}

		public void Rotate90(RClockDirection rClockDirection) {
			if (rClockDirection != RClockDirection.None) {
				Image.Rotate90((ClockDirection)rClockDirection);
			}
		}

		public byte[] SaveExr(bool grayScale) {
			return Image.SaveExrToBuffer(grayScale);
		}

		public byte[] SaveJpg(float quality) {
			return Image.SaveJpgToBuffer(quality);
		}

		public byte[] SavePng() {
			return Image.SavePngToBuffer();
		}

		public byte[] SaveWebp(bool lossy, float quality) {
			return Image.SaveWebpToBuffer(lossy, quality);
		}

		public void SetPixel(int x, int y, Colorf color) {
			Image.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a));
		}

		public void ShrinkFactorTwo() {
			Image.ShrinkX2();
		}

		public void SrgbToLinear() {
			Image.SrgbToLinear();
		}
	}
}
