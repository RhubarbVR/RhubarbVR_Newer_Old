using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Linker;

using static Godot.TextServer;

namespace RhubarbVR.Bindings.FontBindings
{
	public class GodotFont : IRFont
	{
		public FontFile FontFile;

		public RFont RFont;
		public void Dispose() {
			FontFile.Free();
		}

		public void Init(RFont rFont) {
			RFont = rFont;
			FontFile ??= new FontFile();
		}
		public byte[] Data { get => FontFile.Data; set => throw new NotImplementedException(); }
		public bool GenerateMipmaps { get => FontFile.GenerateMipmaps; set => FontFile.GenerateMipmaps = value; }
		public RFontAntialiasing Antialiasing { get => (RFontAntialiasing)FontFile.Antialiasing ; set => FontFile.Antialiasing  = (FontAntialiasing)value; }
		public string FontName { get => FontFile.FontName ; set => FontFile.FontName  = value; }
		public string StyleName { get => FontFile.StyleName ; set => FontFile.StyleName  = value; }
		public RFontStyle FontStyle { get => (RFontStyle)FontFile.FontStyle ; set => FontFile.FontStyle  = (FontStyle)value; }
		public RSubpixelPositioning SubpixelPositioning { get => (RSubpixelPositioning)FontFile.SubpixelPositioning ; set => FontFile.SubpixelPositioning  = (SubpixelPositioning)value; }
		public bool MultichannelSignedDistanceField { get => FontFile.MultichannelSignedDistanceField ; set => FontFile.MultichannelSignedDistanceField  = value; }
		public int MsdfPixelRange { get => FontFile.MsdfPixelRange ; set => FontFile.MsdfPixelRange  = value; }
		public int MsdfSize { get => FontFile.MsdfSize ; set => FontFile.MsdfSize  = value; }
		public bool ForceAutohinter { get => FontFile.ForceAutohinter ; set => FontFile.ForceAutohinter  = value; }
		public RHinting Hinting { get => (RHinting)FontFile.Hinting ; set => FontFile.Hinting  = (Hinting)value; }
		public float Oversampling { get => FontFile.Oversampling ; set => FontFile.Oversampling  = value; }
		public int FixedSize { get => FontFile.FixedSize ; set => FontFile.FixedSize  = value; }

		public void AddFallBack(IRFont rFont) {
			if(rFont is GodotFont godot) {
				FontFile.Fallbacks.Add(godot.FontFile);
			}
		}

		public bool LoadBitmapFont(string path) {
			return FontFile.LoadBitmapFont(path) == Error.Ok;
		}

		public bool LoadDynamicFont(string path) {
			FontFile.MsdfSize = 96;
			return FontFile.LoadDynamicFont(path) == Error.Ok;
		}

		public void RemoveFallBack(IRFont rFont) {
			if (rFont is GodotFont godot) {
				FontFile.Fallbacks.Remove(godot.FontFile);
			}
		}
	}
}
