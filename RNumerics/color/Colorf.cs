using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;

namespace RNumerics
{
	[MessagePackObject]
	public struct Colorf : IComparable<Colorf>, IEquatable<Colorf>, IConvertible
	{
		[Key(0)]
		public float r;
		[Key(1)]
		public float g;
		[Key(2)]
		public float b;
		[Key(3)]
		public float a;

		public Colorf(float greylevel, float a = 1) { r = g = b = greylevel; this.a = a; }
		public Colorf(float r, float g, float b, float a = 1) { this.r = r; this.g = g; this.b = b; this.a = a; }
		public Colorf(int r, int g, int b, int a = 255) {
			this.r = MathUtil.Clamp((float)r, 0.0f, 255.0f) / 255.0f;
			this.g = MathUtil.Clamp((float)g, 0.0f, 255.0f) / 255.0f;
			this.b = MathUtil.Clamp((float)b, 0.0f, 255.0f) / 255.0f;
			this.a = MathUtil.Clamp((float)a, 0.0f, 255.0f) / 255.0f;
		}
		public Colorf(float[] v2) { r = v2[0]; g = v2[1]; b = v2[2]; a = v2[3]; }
		public Colorf(Colorf copy) { r = copy.r; g = copy.g; b = copy.b; a = copy.a; }
		public Colorf(Colorf copy, float newAlpha) { r = copy.r; g = copy.g; b = copy.b; a = newAlpha; }

		public Colorf Clone(float fAlphaMultiply = 1.0f) {
			return new Colorf(r, g, b, a * fAlphaMultiply);
		}

		[IgnoreMember]
		public float this[int key]
		{
			get => key == 0 ? r : key == 1 ? g : key == 2 ? b : a;
			set {
				if (key == 0) { r = value; }
				else if (key == 1) { g = value; }
				else if (key == 2) { b = value; }
				else {
					a = value;
				}
			}
		}

		public float SqrDistance(Colorf v2) {
			float a = r - v2.r, b = g - v2.g, c = b - v2.b, d = a - v2.a;
			return (a * a) + (b * b) + (c * c) + (d * d);
		}

		public Vector3f ToRGB() {
			return new Vector3f(r, g, b);
		}
		public Colorb ToBytes() {
			return new Colorb(r, g, b, a);
		}

		public void Set(Colorf o) {
			r = o.r;
			g = o.g;
			b = o.b;
			a = o.a;
		}
		public void Set(float fR, float fG, float fB, float fA) {
			r = fR;
			g = fG;
			b = fB;
			a = fA;
		}


		public Colorf SetAlpha(float a) {
			this.a = a;
			return this;
		}
		public void Add(Colorf o) {
			r += o.r;
			g += o.g;
			b += o.b;
			a += o.a;
		}
		public void Subtract(Colorf o) {
			r -= o.r;
			g -= o.g;
			b -= o.b;
			a -= o.a;
		}
		public Colorf WithAlpha(float newAlpha) {
			return new Colorf(r, g, b, newAlpha);
		}


		public static Colorf operator -(Colorf v) => new(-v.r, -v.g, -v.b, -v.a);

		public static Colorf operator *(float f, Colorf v) => new(f * v.r, f * v.g, f * v.b, f * v.a);
		public static Colorf operator *(Colorf v, float f) => new(f * v.r, f * v.g, f * v.b, f * v.a);

		public static Colorf operator +(Colorf v0, Colorf v1) => new(v0.r + v1.r, v0.g + v1.g, v0.b + v1.b, v0.a + v1.a);
		public static Colorf operator +(Colorf v0, float f) => new(v0.r + f, v0.g + f, v0.b + f, v0.a + f);

		public static Colorf operator -(Colorf v0, Colorf v1) => new(v0.r - v1.r, v0.g - v1.g, v0.b - v1.b, v0.a - v1.a);
		public static Colorf operator -(Colorf v0, float f) => new(v0.r - f, v0.g - f, v0.b - f, v0.a = f);


		public static bool operator ==(Colorf a, Colorf b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
		public static bool operator !=(Colorf a, Colorf b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
		public override bool Equals(object obj) {
			return this == (Colorf)obj;
		}
		public override int GetHashCode() {
			return (r + g + b + a).GetHashCode();
		}
		public int CompareTo(Colorf other) {
			if (r != other.r) {
				return r < other.r ? -1 : 1;
			}
			else if (g != other.g) {
				return g < other.g ? -1 : 1;
			}
			else if (b != other.b) {
				return b < other.b ? -1 : 1;
			}
			else if (a != other.a) {
				return a < other.a ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Colorf other) {
			return r == other.r && g == other.g && b == other.b && a == other.a;
		}


		public static Colorf Lerp(Colorf a, Colorf b, float t) {
			var s = 1 - t;
			return new Colorf((s * a.r) + (t * b.r), (s * a.g) + (t * b.g), (s * a.b) + (t * b.b), (s * a.a) + (t * b.a));
		}



		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", r, g, b, a);
		}
		public string ToString(string fmt) {
			return string.Format("{0} {1} {2} {3}", r.ToString(fmt), g.ToString(fmt), b.ToString(fmt), a.ToString(fmt));
		}

		public TypeCode GetTypeCode() {
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider) {
			return ToString();
		}

		public object ToType(Type conversionType, IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider = null) {
			var bytes = ToBytes();
			return bytes.r
				+ ((uint)bytes.g << 8)
				+ ((uint)bytes.b << 16)
				+ ((uint)bytes.a << 24);
		}

		public ulong ToUInt64(IFormatProvider provider) {
			throw new NotImplementedException();
		}

		[IgnoreMember]
		static public readonly Colorf TransparentWhite = new(255, 255, 255, 0);
		[IgnoreMember]
		static public readonly Colorf TransparentBlack = new(0, 0, 0, 0);

		[IgnoreMember]
		static public readonly Colorf White = new(1f, 1f, 1f, 1f);
		[IgnoreMember]
		static public readonly Colorf Black = new(0f, 0f, 0f, 1f);
		[IgnoreMember]
		static public readonly Colorf Blue = new(0, 0, 255, 255);
		[IgnoreMember]
		static public readonly Colorf Green = new(0, 255, 0, 255);
		[IgnoreMember]
		static public readonly Colorf Red = new(255, 0, 0, 255);
		[IgnoreMember]
		static public readonly Colorf Yellow = new(255, 255, 0, 255);
		[IgnoreMember]
		static public readonly Colorf Cyan = new(0, 255, 255, 255);
		[IgnoreMember]
		static public readonly Colorf Magenta = new(255, 0, 255, 255);
		[IgnoreMember]
		static public readonly Colorf VideoWhite = new(235, 235, 235, 255);
		[IgnoreMember]
		static public readonly Colorf VideoBlack = new(16, 16, 16, 255);
		[IgnoreMember]
		static public readonly Colorf VideoBlue = new(16, 16, 235, 255);
		[IgnoreMember]
		static public readonly Colorf VideoGreen = new(16, 235, 16, 255);
		[IgnoreMember]
		static public readonly Colorf VideoRed = new(235, 16, 16, 255);
		[IgnoreMember]
		static public readonly Colorf VideoYellow = new(235, 235, 16, 255);
		[IgnoreMember]
		static public readonly Colorf VideoCyan = new(16, 235, 235, 255);
		[IgnoreMember]
		static public readonly Colorf VideoMagenta = new(235, 16, 235, 255);
		[IgnoreMember]
		static public readonly Colorf RhubarbGreen = new(17, 255, 0);
		[IgnoreMember]
		static public readonly Colorf RhubarbRed = new(237, 25, 67);
		[IgnoreMember]
		static public readonly Colorf Purple = new(161, 16, 193, 255);
		[IgnoreMember]
		static public readonly Colorf DarkRed = new(128, 16, 16, 255);
		[IgnoreMember]
		static public readonly Colorf FireBrick = new(178, 34, 34, 255);
		[IgnoreMember]
		static public readonly Colorf HotPink = new(255, 105, 180, 255);
		[IgnoreMember]
		static public readonly Colorf LightPink = new(255, 182, 193, 255);
		[IgnoreMember]
		static public readonly Colorf DarkBlue = new(16, 16, 139, 255);
		[IgnoreMember]
		static public readonly Colorf BlueMetal = new(176, 197, 235, 255);
		[IgnoreMember]
		static public readonly Colorf Navy = new(16, 16, 128, 255);
		[IgnoreMember]
		static public readonly Colorf CornflowerBlue = new(100, 149, 237, 255);
		[IgnoreMember]
		static public readonly Colorf LightSteelBlue = new(176, 196, 222, 255);
		[IgnoreMember]
		static public readonly Colorf DarkSlateBlue = new(72, 61, 139, 255);
		[IgnoreMember]
		static public readonly Colorf Teal = new(16, 128, 128, 255);
		[IgnoreMember]
		static public readonly Colorf ForestGreen = new(16, 139, 16, 255);
		[IgnoreMember]
		static public readonly Colorf LightGreen = new(144, 238, 144, 255);
		[IgnoreMember]
		static public readonly Colorf Orange = new(230, 73, 16, 255);
		[IgnoreMember]
		static public readonly Colorf Gold = new(235, 115, 63, 255);
		[IgnoreMember]
		static public readonly Colorf DarkYellow = new(235, 200, 95, 255);
		[IgnoreMember]
		static public readonly Colorf SiennaBrown = new(160, 82, 45, 255);
		[IgnoreMember]
		static public readonly Colorf SaddleBrown = new(139, 69, 19, 255);
		[IgnoreMember]
		static public readonly Colorf Goldenrod = new(218, 165, 32, 255);
		[IgnoreMember]
		static public readonly Colorf Wheat = new(245, 222, 179, 255);


		[IgnoreMember]
		static public readonly Colorf LightGrey = new(211, 211, 211, 255);
		[IgnoreMember]
		static public readonly Colorf Silver = new(192, 192, 192, 255);
		[IgnoreMember]
		static public readonly Colorf LightSlateGrey = new(119, 136, 153, 255);
		[IgnoreMember]
		static public readonly Colorf Grey = new(128, 128, 128, 255);
		[IgnoreMember]
		static public readonly Colorf DarkGrey = new(169, 169, 169, 255);
		[IgnoreMember]
		static public readonly Colorf SlateGrey = new(112, 128, 144, 255);
		[IgnoreMember]
		static public readonly Colorf DimGrey = new(105, 105, 105, 255);
		[IgnoreMember]
		static public readonly Colorf DarkSlateGrey = new(47, 79, 79, 255);



		// default colors
		[IgnoreMember]
		static readonly public Colorf StandardBeige = new(0.75f, 0.75f, 0.5f);
		[IgnoreMember]
		static readonly public Colorf SelectionGold = new(1.0f, 0.6f, 0.05f);
		[IgnoreMember]
		static readonly public Colorf PivotYellow = new(1.0f, 1.0f, 0.05f);



		// allow conversion to/from Vector3f
		public static implicit operator Vector3f(Colorf c) => new(c.r, c.g, c.b);
		public static implicit operator Colorf(Vector3f c) => new(c.x, c.y, c.z, 1);


		public static implicit operator Colorf(ColorHSV c) => c.ConvertToRGB();


		public static implicit operator ColorHSV(Colorf color) {
			var outval = new ColorHSV(0, 0, 0, color.a);
			outval.ConvertFromRGB(color);
			return outval;
		}

	}
}
