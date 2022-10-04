using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RNumerics;


namespace RhuEngine.Linker
{
	public enum RFontAntialiasing : long
	{
		//
		// Summary:
		//     Font glyphs are rasterized as 1-bit bitmaps.
		None,
		//
		// Summary:
		//     Font glyphs are rasterized as 8-bit grayscale anti-aliased bitmaps.
		Gray,
		//
		// Summary:
		//     Font glyphs are rasterized for LCD screens.
		//     LCD sub-pixel layout is determined by the value of gui/theme/lcd_subpixel_layout
		//     project settings.
		//     LCD sub-pixel anti-aliasing mode is suitable only for rendering horizontal, unscaled
		//     text in 2D.
		Lcd
	}

	[Flags]
	public enum RFontStyle : long
	{
		None = 0,
		Regular = None,

		//
		// Summary:
		//     Font is bold.
		Bold = 0x1L,
		//
		// Summary:
		//     Font is italic or oblique.
		Italic = 0x2L,
		//
		// Summary:
		//     Font have fixed-width characters.
		FixedWidth = 0x4L
	}

	public enum RSubpixelPositioning : long
	{
		//
		// Summary:
		//     Glyph horizontal position is rounded to the whole pixel size, each glyph is rasterized
		//     once.
		Disabled = 0L,
		//
		// Summary:
		//     Glyph horizontal position is rounded based on font size.
		//     - To one quarter of the pixel size if font size is smaller or equal to Godot.TextServer.SubpixelPositioning.OneQuarterMaxSize.
		//     - To one half of the pixel size if font size is smaller or equal to Godot.TextServer.SubpixelPositioning.OneHalfMaxSize.
		//     - To the whole pixel size for larger fonts.
		Auto = 1L,
		//
		// Summary:
		//     Glyph horizontal position is rounded to one half of the pixel size, each glyph
		//     is rasterized up to two times.
		OneHalf = 2L,
		//
		// Summary:
		//     Glyph horizontal position is rounded to one quarter of the pixel size, each glyph
		//     is rasterized up to four times.
		OneQuarter = 3L,
		//
		// Summary:
		//     Maximum font size which will use one half of the pixel subpixel positioning in
		//     Godot.TextServer.SubpixelPositioning.Auto mode.
		OneHalfMaxSize = 20L,
		//
		// Summary:
		//     Maximum font size which will use one quarter of the pixel subpixel positioning
		//     in Godot.TextServer.SubpixelPositioning.Auto mode.
		OneQuarterMaxSize = 0x10L
	}


	public enum RHinting : long
	{
		//
		// Summary:
		//     Disables font hinting (smoother but less crisp).
		None,
		//
		// Summary:
		//     Use the light font hinting mode.
		Light,
		//
		// Summary:
		//     Use the default font hinting mode (crisper but less smooth).
		//     Note: This hinting mode changes both horizontal and vertical glyph metrics. If
		//     applied to monospace font, some glyphs might have different width.
		Normal
	}

	public interface IRFont : IDisposable
	{
		void Init(RFont rFont);

		void AddFallBack(IRFont rFont);

		void RemoveFallBack(IRFont rFont);

		bool LoadBitmapFont(string path);

		bool LoadDynamicFont(string path);

		byte[] Data { get; set; }

		bool GenerateMipmaps { get; set; }

		RFontAntialiasing Antialiasing { get; set; }

		string FontName { get; set; }
		string StyleName { get; set; }
		RFontStyle FontStyle { get; set; }
		RSubpixelPositioning SubpixelPositioning { get; set; }
		bool MultichannelSignedDistanceField { get; set; }
		int MsdfPixelRange { get; set; }
		int MsdfSize { get; set; }
		bool ForceAutohinter { get; set; }
		RHinting Hinting { get; set; }

		float Oversampling { get; set; }

		int FixedSize { get; set; }

	}

	public interface IRText:IDisposable {
		IRTexture2D Init(RText text, RFont font);

		void SetText(string text);
	}

	public sealed class RText : IDisposable
	{
		public RTexture2D texture2D;

		public static Type Instance { get; set; }

		public IRText Inst { get; set; }

		public RText(RFont font) {
			Inst = (IRText)Activator.CreateInstance(Instance);
			texture2D = new RTexture2D(Inst.Init(this, font));
		}

		public string Text
		{
			set => Inst.SetText(value);
		}
		public float AspectRatio => texture2D.Width / texture2D.Height;

		public void Dispose() {
			Inst.Dispose();
		}
	}

	public sealed class RFont : IDisposable
	{

		public static Type Instance { get; set; }

		public IRFont Inst { get; set; }

		private readonly List<RFont> _fallbacks = new();

		public int FallbackCount => _fallbacks.Count;
		public void AddFallBack(RFont rFont) {
			_fallbacks.Add(rFont);
			Inst.AddFallBack(rFont.Inst);
		}

		public void RemoveFallBack(RFont rFont) {
			_fallbacks.Remove(rFont);
			Inst.RemoveFallBack(rFont.Inst);
		}

		public RFont(IRFont font) {
			Inst = font ?? (IRFont)Activator.CreateInstance(Instance);
			Inst.Init(this);
		}
		public bool LoadBitmapFont(string path) {
			return Inst.LoadBitmapFont(path);
		}
		public bool LoadDynamicFont(Stream data) {
			var temp = Path.GetTempFileName();
			var fileStream = File.OpenWrite(temp);
			data.CopyTo(fileStream);
			fileStream.Close();
			fileStream.Dispose();
			RLog.Info($"Temp Font file Created Path: {temp}");
			return LoadDynamicFont(temp);
		}

		public bool LoadDynamicFont(string path) {
			var fontstate = Inst.LoadDynamicFont(path);
			if (!fontstate) {
				RLog.Warn("Failed to load font");
			}
			return fontstate;
		}

		public void Dispose() {
			Inst.Dispose();
		}

		public byte[] Data { get => Inst.Data; set => Inst.Data = value; }
		public bool GenerateMipmaps { get => Inst.GenerateMipmaps; set => Inst.GenerateMipmaps = value; }
		public RFontAntialiasing Antialiasing { get => Inst.Antialiasing; set => Inst.Antialiasing = value; }
		public string FontName { get => Inst.FontName; set => Inst.FontName = value; }
		public string StyleName { get => Inst.StyleName; set => Inst.StyleName = value; }
		public RFontStyle FontStyle { get => Inst.FontStyle; set => Inst.FontStyle = value; }
		public RSubpixelPositioning SubpixelPositioning { get => Inst.SubpixelPositioning; set => Inst.SubpixelPositioning = value; }
		public bool MultichannelSignedDistanceField { get => Inst.MultichannelSignedDistanceField; set => Inst.MultichannelSignedDistanceField = value; }
		public int MsdfPixelRange { get => Inst.MsdfPixelRange; set => Inst.MsdfPixelRange = value; }
		public int MsdfSize { get => Inst.MsdfSize; set => Inst.MsdfSize = value; }
		public bool ForceAutohinter { get => Inst.ForceAutohinter; set => Inst.ForceAutohinter = value; }
		public RHinting Hinting { get => Inst.Hinting; set => Inst.Hinting = value; }
		public float Oversampling { get => Inst.Oversampling; set => Inst.Oversampling = value; }
		public int FixedSize { get => Inst.FixedSize; set => Inst.FixedSize = value; }

	}
}
