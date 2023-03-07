using Assimp;


using System;
using System.IO;

namespace RNumerics
{

	public struct Index3i : IComparable<Index3i>, IEquatable<Index3i>, ISerlize<Index3i>
	{
		public int a;
		public int b;
		public int c;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(a);
			binaryWriter.Write(b);
			binaryWriter.Write(c);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			a = binaryReader.ReadInt32();
			b = binaryReader.ReadInt32();
			c = binaryReader.ReadInt32();
		}

		[Exposed]
		public int A
		{
			get => a;
			set => a = value;
		}
		[Exposed]
		public int B
		{
			get => b;
			set => b = value;
		}
		[Exposed]
		public int C
		{
			get => c;
			set => c = value;
		}

		public Index3i() {
			a = 0;
			b = 0;
			c = 0;
		}

		public Index3i(in int z) { a = b = c = z; }
		public Index3i(in int ii, in int jj, in int kk) { a = ii; b = jj; c = kk; }
		public Index3i(in int[] i2) { a = i2[0]; b = i2[1]; c = i2[2]; }
		public Index3i(in Index3i copy) { a = copy.a; b = copy.b; c = copy.b; }

		// reverse last two indices if cycle is true (useful for cw/ccw codes)
		public Index3i(in int ii, in int jj, in int kk, in bool cycle) {
			a = ii;
			if (cycle) { b = kk; c = jj; }
			else { b = jj; c = kk; }
		}

		[Exposed]
		static public readonly Index3i Zero = new(0, 0, 0);
		[Exposed]
		static public readonly Index3i One = new(1, 1, 1);
		[Exposed]
		static public readonly Index3i Max = new(int.MaxValue, int.MaxValue, int.MaxValue);
		[Exposed]
		static public readonly Index3i Min = new(int.MinValue, int.MinValue, int.MinValue);



		public int this[in int key]
		{
			get => (key == 0) ? a : (key == 1) ? b : c;
			set {
				if (key == 0) { a = value; }
				else if (key == 1) { b = value; }
				else {
					c = value;
				}
			}
		}


		public int[] Array => new int[] { a, b, c };



		public int LengthSquared => (a * a) + (b * b) + (c * c);

		public int Length => (int)Math.Sqrt(LengthSquared);


		public void Set(in Index3i o) {
			a = o[0];
			b = o[1];
			c = o[2];
		}
		public void Set(in int ii, in int jj, in int kk) {
			a = ii;
			b = jj;
			c = kk;
		}


		public static Index3i operator -(in Index3i v) => new(-v.a, -v.b, -v.c);

		public static Index3i operator *(in int f, in Index3i v) => new(f * v.a, f * v.b, f * v.c);
		public static Index3i operator *(in Index3i v, in int f) => new(f * v.a, f * v.b, f * v.c);
		public static Index3i operator /(in Index3i v, in int f) => new(v.a / f, v.b / f, v.c / f);


		public static Index3i operator *(in Index3i a, in Index3i b) => new(a.a * b.a, a.b * b.b, a.c * b.c);
		public static Index3i operator /(in Index3i a, in Index3i b) => new(a.a / b.a, a.b / b.b, a.c / b.c);


		public static Index3i operator +(in Index3i v0, in Index3i v1) => new(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c);
		public static Index3i operator +(in Index3i v0, in int f) => new(v0.a + f, v0.b + f, v0.c + f);

		public static Index3i operator -(in Index3i v0, in Index3i v1) => new(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c);
		public static Index3i operator -(in Index3i v0, in int f) => new(v0.a - f, v0.b - f, v0.c - f);


		public static bool operator ==(in Index3i a, in Index3i b) => a.a == b.a && a.b == b.b && a.c == b.c;
		public static bool operator !=(in Index3i a, in Index3i b) => a.a != b.a || a.b != b.b || a.c != b.c;
		public override bool Equals(object obj) {
			return this == (Index3i)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(a, b, c);
		}

		public int CompareTo(Index3i other) {
			if (a != other.a) {
				return a < other.a ? -1 : 1;
			}
			else if (b != other.b) {
				return b < other.b ? -1 : 1;
			}
			else if (c != other.c) {
				return c < other.c ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Index3i other) {
			return a == other.a && b == other.b && c == other.c;
		}


		public override string ToString() {
			return string.Format("[{0},{1},{2}]", a, b, c);
		}

	}











	public struct Index2i : IComparable<Index2i>, IEquatable<Index2i>, ISerlize<Index2i>
	{
		public int a;
		public int b;


		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(a);
			binaryWriter.Write(b);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			a = binaryReader.ReadInt32();
			b = binaryReader.ReadInt32();
		}


		public Index2i() {
			a = 0;
			b = 0;
		}
		public Index2i(in int z) { a = b = z; }
		public Index2i(in int ii, in int jj) { a = ii; b = jj; }
		public Index2i(in int[] i2) { a = i2[0]; b = i2[1]; }
		public Index2i(in Index2i copy) { a = copy.a; b = copy.b; }

		static public readonly Index2i Zero = new(0, 0);

		static public readonly Index2i One = new(1, 1);

		static public readonly Index2i Max = new(int.MaxValue, int.MaxValue);

		static public readonly Index2i Min = new(int.MinValue, int.MinValue);


		public int this[in int key]
		{
			get => (key == 0) ? a : b;
			set {
				if (key == 0) { a = value; }
				else {
					b = value;
				}
			}
		}

		public int[] Array => new int[] { a, b };


		public int LengthSquared => (a * a) + (b * b);

		public int Length => (int)Math.Sqrt(LengthSquared);


		public void Set(in Index2i o) {
			a = o[0];
			b = o[1];
		}
		public void Set(in int ii, in int jj) {
			a = ii;
			b = jj;
		}


		public static Index2i operator -(in Index2i v) => new(-v.a, -v.b);

		public static Index2i operator *(in int f, in Index2i v) => new(f * v.a, f * v.b);
		public static Index2i operator *(in Index2i v, in int f) => new(f * v.a, f * v.b);
		public static Index2i operator /(in Index2i v, in int f) => new(v.a / f, v.b / f);


		public static Index2i operator *(in Index2i a, in Index2i b) => new(a.a * b.a, a.b * b.b);
		public static Index2i operator /(in Index2i a, in Index2i b) => new(a.a / b.a, a.b / b.b);


		public static Index2i operator +(in Index2i v0, in Index2i v1) => new(v0.a + v1.a, v0.b + v1.b);
		public static Index2i operator +(in Index2i v0, in int f) => new(v0.a + f, v0.b + f);

		public static Index2i operator -(in Index2i v0, in Index2i v1) => new(v0.a - v1.a, v0.b - v1.b);

		public static Index2i operator -(in Index2i v0, in int f) => new(v0.a - f, v0.b - f);


		public static bool operator ==(in Index2i a, in Index2i b) => a.a == b.a && a.b == b.b;
		public static bool operator !=(in Index2i a, in Index2i b) => a.a != b.a || a.b != b.b;
		public override bool Equals(object obj) {
			return this == (Index2i)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(a, b);
		}

		public int CompareTo(Index2i other) {
			if (a != other.a) {
				return a < other.a ? -1 : 1;
			}
			else if (b != other.b) {
				return b < other.b ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Index2i other) {
			return a == other.a && b == other.b;
		}


		public override string ToString() {
			return string.Format("[{0},{1}]", a, b);
		}

		public static TypeCode GetTypeCode() {
			return TypeCode.Object;
		}
	}








	public struct Index4i : ISerlize<Index4i>
	{
		public int a;
		public int b;
		public int c;
		public int d;


		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(a);
			binaryWriter.Write(b);
			binaryWriter.Write(c);
			binaryWriter.Write(d);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			a = binaryReader.ReadInt32();
			b = binaryReader.ReadInt32();
			c = binaryReader.ReadInt32();
			d = binaryReader.ReadInt32();
		}

		public Index4i() {
			a = 0;
			b = 0;
			c = 0;
			d = 0;
		}
		public Index4i(in int z) { a = b = c = d = z; }
		public Index4i(in int aa, in int bb, in int cc, in int dd) { a = aa; b = bb; c = cc; d = dd; }
		public Index4i(in int[] i2) { a = i2[0]; b = i2[1]; c = i2[2]; d = i2[3]; }
		public Index4i(in Index4i copy) { a = copy.a; b = copy.b; c = copy.b; d = copy.d; }


		static public readonly Index4i Zero = new(0, 0, 0, 0);

		static public readonly Index4i One = new(1, 1, 1, 1);

		static public readonly Index4i Max = new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);



		public int this[in int key]
		{
			get => (key == 0) ? a : (key == 1) ? b : (key == 2) ? c : d;
			set {
				if (key == 0) { a = value; }
				else if (key == 1) { b = value; }
				else if (key == 2) { c = value; }
				else {
					d = value;
				}
			}
		}


		public int[] Array => new int[4] { a, b, c, d };



		public int LengthSquared => (a * a) + (b * b) + (c * c) + (d * d);

		public int Length => (int)Math.Sqrt(LengthSquared);


		public void Set(in Index4i o) {
			a = o[0];
			b = o[1];
			c = o[2];
			d = o[3];
		}
		public void Set(in int aa, in int bb, in int cc, in int dd) {
			a = aa;
			b = bb;
			c = cc;
			d = dd;
		}


		public bool Contains(in int val) {
			return a == val || b == val || c == val || d == val;
		}

		public void Sort() {
			int tmp;   // if we use 2 temp ints, we can swap in a different order where some test pairs
					   // could be done simultaneously, but no idea if compiler would optimize that anyway...
			if (d < c) { tmp = d; d = c; c = tmp; }
			if (c < b) { tmp = c; c = b; b = tmp; }
			if (b < a) { tmp = b; b = a; a = tmp; }   // now a is smallest value
			if (b > c) { tmp = c; c = b; b = tmp; }
			if (c > d) { tmp = d; d = c; c = tmp; }   // now d is largest value
			if (b > c) { tmp = c; c = b; b = tmp; }   // bow b,c are sorted
		}


		public static Index4i operator -(in Index4i v) => new(-v.a, -v.b, -v.c, -v.d);

		public static Index4i operator *(in int f, in Index4i v) => new(f * v.a, f * v.b, f * v.c, f * v.d);
		public static Index4i operator *(in Index4i v, in int f) => new(f * v.a, f * v.b, f * v.c, f * v.d);
		public static Index4i operator /(in Index4i v, in int f) => new(v.a / f, v.b / f, v.c / f, v.d / f);


		public static Index4i operator *(in Index4i a, in Index4i b) => new(a.a * b.a, a.b * b.b, a.c * b.c, a.d * b.d);
		public static Index4i operator /(in Index4i a, in Index4i b) => new(a.a / b.a, a.b / b.b, a.c / b.c, a.d / b.d);


		public static Index4i operator +(in Index4i v0, in Index4i v1) => new(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c, v0.d + v1.d);
		public static Index4i operator +(in Index4i v0, in int f) => new(v0.a + f, v0.b + f, v0.c + f, v0.d + f);

		public static Index4i operator -(in Index4i v0, in Index4i v1) => new(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c, v0.d - v1.d);
		public static Index4i operator -(in Index4i v0, in int f) => new(v0.a - f, v0.b - f, v0.c - f, v0.d - f);


		public static bool operator ==(in Index4i a, in Index4i b) => a.a == b.a && a.b == b.b && a.c == b.c && a.d == b.d;
		public static bool operator !=(in Index4i a, in Index4i b) => a.a != b.a || a.b != b.b || a.c != b.c || a.d != b.d;
		public override bool Equals(object obj) {
			return this == (Index4i)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(a, b, c, d);
		}

		public int CompareTo(in Index4i other) {
			if (a != other.a) {
				return a < other.a ? -1 : 1;
			}
			else if (b != other.b) {
				return b < other.b ? -1 : 1;
			}
			else if (c != other.c) {
				return c < other.c ? -1 : 1;
			}
			else if (d != other.d) {
				return d < other.d ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(in Index4i other) {
			return a == other.a && b == other.b && c == other.c && d == other.d;
		}



		public override string ToString() {
			return string.Format("[{0},{1},{2},{3}]", a, b, c, d);
		}

	}



}
