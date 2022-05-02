using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface IRTexture2D
	{
		RTexture2D White { get; }
		int GetWidth(object target);
		int GetHeight(object target);
		int GetAnisoptropy(object target);
		void SetAnisoptropy(object target,int value);
		TexAddress GetAddressMode(object target);
		void SetAddressMode(object target, TexAddress value);
		TexSample GetSampleMode(object target);
		void SetSampleMode(object target, TexSample value);

		object Make(TexType dynamic, TexFormat rgba32Linear);

		object MakeFromMemory(byte[] data);
		public object MakeFromColors(Colorb[] colors, int width, int height, bool srgb);

		public void SetSize(object tex,int width, int height);

		public void SetColors(object tex,int width, int height, byte[] rgbaData);
	}

	public class RTexture2D
	{
		public static IRTexture2D Instance { get; set; }

		public object Tex;
		public RTexture2D(object tex) {
			Tex = tex;
		}

		public RTexture2D(TexType dynamic, TexFormat rgba32Linear) {
			Tex = Instance?.Make(dynamic, rgba32Linear);
		}

		public static RTexture2D White => Instance?.White;
		public int Width => Instance?.GetWidth(Tex)??0;
		public int Height => Instance?.GetHeight(Tex) ?? 0;
		public int Anisoptropy
		{
			get => Instance?.GetAnisoptropy(Tex)??0;
			set => Instance?.SetAnisoptropy(Tex, value);
		}
		public TexAddress AddressMode
		{
			get => Instance?.GetAddressMode(Tex) ?? 0;
			set => Instance?.SetAddressMode(Tex, value);
		}
		public TexSample SampleMode
		{
			get => Instance?.GetSampleMode(Tex) ?? 0;
			set => Instance?.SetSampleMode(Tex, value);
		}

		public static RTexture2D FromMemory(byte[] vs) {
			return new RTexture2D(Instance.MakeFromMemory(vs));
		}

		public static RTexture2D FromColors(Colorb[] colors, int width, int height, bool srgb) {
			return new RTexture2D(Instance.MakeFromColors(colors,width,height,srgb));
		}

		public void SetSize(int width, int height) {
			Instance.SetSize(Tex, width, height);
		}

		public void SetColors(int width, int height, byte[] rgbaData) {
			Instance.SetColors(Tex, width, height, rgbaData);
		}
	}
}
