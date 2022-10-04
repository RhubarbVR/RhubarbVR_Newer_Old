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

	public enum RHorizontalAlignment : long
	{
		//
		// Summary:
		//     Horizontal left alignment, usually for text-derived classes.
		Left,
		//
		// Summary:
		//     Horizontal center alignment, usually for text-derived classes.
		Center,
		//
		// Summary:
		//     Horizontal right alignment, usually for text-derived classes.
		Right,
		//
		// Summary:
		//     Expand row to fit width, usually for text-derived classes.
		Fill
	}
	public enum RVerticalAlignment : long
	{
		//
		// Summary:
		//     Vertical top alignment, usually for text-derived classes.
		Top,
		//
		// Summary:
		//     Vertical center alignment, usually for text-derived classes.
		Center,
		//
		// Summary:
		//     Vertical bottom alignment, usually for text-derived classes.
		Bottom,
		//
		// Summary:
		//     Expand rows to fit height, usually for text-derived classes.
		Fill
	}
	public enum RAutowrapMode : long
	{
		//
		// Summary:
		//     Autowrap is disabled.
		Off,
		//
		// Summary:
		//     Wraps the text inside the node's bounding rectangle by allowing to break lines
		//     at arbitrary positions, which is useful when very limited space is available.
		Arbitrary,
		//
		// Summary:
		//     Wraps the text inside the node's bounding rectangle by soft-breaking between
		//     words.
		Word,
		//
		// Summary:
		//     Behaves similarly to Godot.TextServer.AutowrapMode.Word, but force-breaks a word
		//     if that single word does not fit in one line.
		WordSmart
	}

	public enum ROverrunBehavior : long
	{
		//
		// Summary:
		//     No text trimming is performed.
		NoTrimming,
		//
		// Summary:
		//     Trims the text per character.
		TrimChar,
		//
		// Summary:
		//     Trims the text per word.
		TrimWord,
		//
		// Summary:
		//     Trims the text per character and adds an ellipsis to indicate that parts are
		//     hidden.
		TrimEllipsis,
		//
		// Summary:
		//     Trims the text per word and adds an ellipsis to indicate that parts are hidden.
		TrimWordEllipsis
	}
	public enum RVisibleCharactersBehavior : long
	{
		//
		// Summary:
		//     Trims text before the shaping. e.g, increasing Godot.Label.VisibleCharacters
		//     or Godot.RichTextLabel.VisibleCharacters value is visually identical to typing
		//     the text.
		CharsBeforeShaping,
		//
		// Summary:
		//     Displays glyphs that are mapped to the first Godot.Label.VisibleCharacters or
		//     Godot.RichTextLabel.VisibleCharacters characters from the beginning of the text.
		CharsAfterShaping,
		//
		// Summary:
		//     Displays Godot.Label.VisibleRatio or Godot.RichTextLabel.VisibleRatio glyphs,
		//     starting from the left or from the right, depending on Godot.Control.LayoutDirection
		//     value.
		GlyphsAuto,
		//
		// Summary:
		//     Displays Godot.Label.VisibleRatio or Godot.RichTextLabel.VisibleRatio glyphs,
		//     starting from the left.
		GlyphsLtr,
		//
		// Summary:
		//     Displays Godot.Label.VisibleRatio or Godot.RichTextLabel.VisibleRatio glyphs,
		//     starting from the right.
		GlyphsRtl
	}

	public enum RTextDirection : long
	{
		//
		// Summary:
		//     Text writing direction is the same as layout direction.
		Inherited = 3L,
		//
		// Summary:
		//     Automatic text writing direction, determined from the current locale and text
		//     content.
		Auto = 0L,
		//
		// Summary:
		//     Left-to-right text writing direction.
		Ltr = 1L,
		//
		// Summary:
		//     Right-to-left text writing direction.
		Rtl = 2L
	}

	public enum RStructuredTextParser : long
	{
		//
		// Summary:
		//     Use default behavior. Same as STRUCTURED_TEXT_NONE unless specified otherwise
		//     in the control description.
		Default,
		//
		// Summary:
		//     BiDi override for URI.
		Uri,
		//
		// Summary:
		//     BiDi override for file path.
		File,
		//
		// Summary:
		//     BiDi override for email.
		Email,
		//
		// Summary:
		//     BiDi override for lists.
		//     Structured text options: list separator String.
		List,
		//
		// Summary:
		//     Use default Unicode BiDi algorithm.
		None,
		//
		// Summary:
		//     User defined structured text BiDi override function.
		Custom
	}

	public interface IRText : IDisposable
	{
		IRTexture2D Init(RText text);
		public bool AutoScale { get; set; }
		public RFont Font { get; set; }
		public string Text { get; set; }
		public float LineSpacing { get; set; }
		public int FontSize { get; set; }
		public Colorf FontColor { get; set; }
		public int OutlineSize { get; set; }
		public Colorf OutlineColor { get; set; }
		public int ShadowSize { get; set; }
		public Colorf ShadowColor { get; set; }
		public Vector2f ShadowOffset { get; set; }
		public Vector2i Size { get; set; }

		public RHorizontalAlignment HorizontalAlignment { get; set; }
		public RVerticalAlignment VerticalAlignment { get; set; }
		public RAutowrapMode AutowrapMode { get; set; }
		public bool ClipText { get; set; }
		public ROverrunBehavior TextOverrunBehavior { get; set; }
		public bool Uppercase { get; set; }
		public int LinesSkipped { get; set; }
		public int MaxLinesVisible { get; set; }
		public int VisibleCharacters { get; set; }
		public RVisibleCharactersBehavior VisibleCharactersBehavior { get; set; }
		public float VisibleRatio { get; set; }
		public RTextDirection TextDirection { get; set; }
		public string Language { get; set; }
		public RStructuredTextParser StructuredTextBidiOverride { get; set; }

	}

	public sealed class RText : IDisposable
	{
		public RTexture2D texture2D;

		public static Type Instance { get; set; }

		public IRText Inst { get; set; }

		public RText(RFont font, bool autoScale = true) {
			Inst = (IRText)Activator.CreateInstance(Instance);
			texture2D = new RTexture2D(Inst.Init(this));
			Font = font;
			FontSize = 96;
			AutoScale = autoScale;
		}

		public bool AutoScale { get => Inst.AutoScale; set => Inst.AutoScale = value; }
		public RFont Font { get => Inst.Font; set => Inst.Font = value; }
		public string Text { get => Inst.Text; set => Inst.Text = value; }
		public float LineSpacing { get => Inst.LineSpacing; set => Inst.LineSpacing = value; }
		public int FontSize { get => Inst.FontSize; set => Inst.FontSize = value; }
		public Colorf FontColor { get => Inst.FontColor; set => Inst.FontColor = value; }
		public int OutlineSize { get => Inst.OutlineSize; set => Inst.OutlineSize = value; }
		public Colorf OutlineColor { get => Inst.OutlineColor; set => Inst.OutlineColor = value; }
		public int ShadowSize { get => Inst.ShadowSize; set => Inst.ShadowSize = value; }
		public Colorf ShadowColor { get => Inst.ShadowColor; set => Inst.ShadowColor = value; }
		public Vector2f ShadowOffset { get => Inst.ShadowOffset; set => Inst.ShadowOffset = value; }
		public Vector2i Size { get => Inst.Size; set => Inst.Size = value; }
		public RHorizontalAlignment HorizontalAlignment { get => Inst.HorizontalAlignment; set => Inst.HorizontalAlignment = value; }
		public RVerticalAlignment VerticalAlignment { get => Inst.VerticalAlignment; set => Inst.VerticalAlignment = value; }
		public RAutowrapMode AutowrapMode { get => Inst.AutowrapMode; set => Inst.AutowrapMode = value; }
		public bool ClipText { get => Inst.ClipText; set => Inst.ClipText = value; }
		public ROverrunBehavior TextOverrunBehavior { get => Inst.TextOverrunBehavior; set => Inst.TextOverrunBehavior = value; }
		public bool Uppercase { get => Inst.Uppercase; set => Inst.Uppercase = value; }
		public int LinesSkipped { get => Inst.LinesSkipped; set => Inst.LinesSkipped = value; }
		public int MaxLinesVisible { get => Inst.MaxLinesVisible; set => Inst.MaxLinesVisible = value; }
		public int VisibleCharacters { get => Inst.VisibleCharacters; set => Inst.VisibleCharacters = value; }
		public RVisibleCharactersBehavior VisibleCharactersBehavior { get => Inst.VisibleCharactersBehavior; set => Inst.VisibleCharactersBehavior = value; }
		public float VisibleRatio { get => Inst.VisibleRatio; set => Inst.VisibleRatio = value; }
		public RTextDirection TextDirection { get => Inst.TextDirection; set => Inst.TextDirection = value; }
		public string Language { get => Inst.Language; set => Inst.Language = value; }
		public RStructuredTextParser StructuredTextBidiOverride { get => Inst.StructuredTextBidiOverride; set => Inst.StructuredTextBidiOverride = value; }

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
			TempFiles.AddTempFile(temp);
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
