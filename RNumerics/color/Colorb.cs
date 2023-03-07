using System;
using System.IO;

namespace RNumerics
{
	public struct Colorb : ISerlize<Colorb>
	{
		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(r);
			binaryWriter.Write(g);
			binaryWriter.Write(b);
			binaryWriter.Write(a);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			r = binaryReader.ReadByte();
			g = binaryReader.ReadByte();
			b = binaryReader.ReadByte();
			a = binaryReader.ReadByte();
		}

		public byte r;
		public byte g;
		public byte b;
		public byte a;

		public Colorb() {
			r = 0;
			g = 0;
			b = 0;
			a = 0;
		}

		public Colorb(in byte greylevel, byte a = 255) { r = g = b = greylevel; this.a = a; }
		public Colorb(in byte r, in byte g, in byte b, in byte a = 255) { this.r = r; this.g = g; this.b = b; this.a = a; }
		public Colorb(in float r, in float g, in float b, in float a = 1.0f) {
			this.r = (byte)MathUtil.Clamp((int)(r * 255.0f), 0, 255);
			this.g = (byte)MathUtil.Clamp((int)(g * 255.0f), 0, 255);
			this.b = (byte)MathUtil.Clamp((int)(b * 255.0f), 0, 255);
			this.a = (byte)MathUtil.Clamp((int)(a * 255.0f), 0, 255);
		}
		public Colorb(in byte[] v2) { r = v2[0]; g = v2[1]; b = v2[2]; a = v2[3]; }
		public Colorb(in Colorb copy) { r = copy.r; g = copy.g; b = copy.b; a = copy.a; }
		public Colorb(in Colorb copy, in byte newAlpha) { r = copy.r; g = copy.g; b = copy.b; a = newAlpha; }

		public byte this[in int key]
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
