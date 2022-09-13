using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine.Linker;
using RNumerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RStereoKit
{
	public sealed class SKTexture2d : IRTexture2D
	{
		public RTexture2D White { get; set; } = new RTexture2D(Tex.White);

		public void Dispose(object tex) {
		}

		public RhuEngine.Linker.TexAddress GetAddressMode(object target) {
			return (RhuEngine.Linker.TexAddress)((Tex)target).AddressMode;
		}

		public int GetAnisoptropy(object target) {
			return ((Tex)target).Anisoptropy;
		}

		public int GetHeight(object target) {
			return ((Tex)target).Height;
		}

		public RhuEngine.Linker.TexSample GetSampleMode(object target) {
			return (RhuEngine.Linker.TexSample)((Tex)target).SampleMode;
		}

		public int GetWidth(object target) {
			return ((Tex)target).Width;
		}

		public object Make(RhuEngine.Linker.TexType dynamic, RhuEngine.Linker.TexFormat rgba32Linear) {
			return new Tex((StereoKit.TexType)dynamic, (StereoKit.TexFormat)rgba32Linear);
		}

		public unsafe object MakeFromColors(Colorb[] colors, int width, int height, bool srgb) {
			var color32s = new Color32[colors.Length];
			var hanndel = GCHandle.Alloc(color32s,GCHandleType.Pinned);
			var pin = hanndel.AddrOfPinnedObject();
			Parallel.For(0, colors.Length, (i) => ((Colorb*)pin)[i] = colors[i]);
			hanndel.Free();
			return Tex.FromColors(color32s, width, height, srgb);
		}

		public void SetAddressMode(object target, RhuEngine.Linker.TexAddress value) {
			((Tex)target).AddressMode = (StereoKit.TexAddress)value;
		}

		public void SetAnisoptropy(object target, int value) {
			((Tex)target).Anisoptropy = value;
		}

		public void SetColors(object tex, int width, int height, byte[] rgbaData) {
			((Tex)tex).SetColors(width, height, rgbaData);
		}

		public unsafe void SetColors(object tex, int width, int height, Colorb[] colors) {
			var color32s = new Color32[colors.Length];
			var hanndel = GCHandle.Alloc(color32s, GCHandleType.Pinned);
			var pin = hanndel.AddrOfPinnedObject();
			Parallel.For(0, colors.Length, (i) => ((Colorb*)pin)[i] = colors[i]);
			hanndel.Free();
			((Tex)tex).SetColors(width, height, color32s);
		}

		public void SetSampleMode(object target, RhuEngine.Linker.TexSample value) {
			((Tex)target).SampleMode = (StereoKit.TexSample)value;
		}

		public void SetSize(object tex, int width, int height) {
			((Tex)tex).SetSize(width, height);
		}
	}
}
