using System;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Colorb
	{
		[Key(0)]
		public byte r;
		[Key(1)]
		public byte g;
		[Key(2)]
		public byte b;
		[Key(3)]
		public byte a;

		public Colorb(byte greylevel, byte a = 1) { r = g = b = greylevel; this.a = a; }
		public Colorb(byte r, byte g, byte b, byte a = 1) { this.r = r; this.g = g; this.b = b; this.a = a; }
		public Colorb(float r, float g, float b, float a = 1.0f)
		{
			this.r = (byte)MathUtil.Clamp((int)(r * 255.0f), 0, 255);
			this.g = (byte)MathUtil.Clamp((int)(g * 255.0f), 0, 255);
			this.b = (byte)MathUtil.Clamp((int)(b * 255.0f), 0, 255);
			this.a = (byte)MathUtil.Clamp((int)(a * 255.0f), 0, 255);
		}
		public Colorb(byte[] v2) { r = v2[0]; g = v2[1]; b = v2[2]; a = v2[3]; }
		public Colorb(Colorb copy) { r = copy.r; g = copy.g; b = copy.b; a = copy.a; }
		public Colorb(Colorb copy, byte newAlpha) { r = copy.r; g = copy.g; b = copy.b; a = newAlpha; }

		[IgnoreMember]
		public byte this[int key]
		{
			get => key == 0 ? r : key == 1 ? g : key == 2 ? b : a;
			set {
				switch (key) {
					case 0:
						r = value;
						break;
					case 1:
						g = value;
						break;
					case 2:
						b = value;
						break;
					default:
						a = value;
						break;
				}
			}
		}

	}
}
