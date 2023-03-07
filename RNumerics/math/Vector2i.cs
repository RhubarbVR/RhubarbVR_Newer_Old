using System;
using System.IO;

namespace RNumerics
{
	public struct Vector2i : IComparable<Vector2i>, IEquatable<Vector2i>, ISerlize<Vector2i>
	{
		public int x;
		public int y;


		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadInt32();
			y = binaryReader.ReadInt32();
		}

		[Exposed]
		public int X
		{
			get => x;
			set => x = value;
		}
		[Exposed]
		public int Y
		{
			get => y;
			set => y = value;
		}
		public Vector2i() {
			x = 0;
			y = 0;
		}

		public Vector2i(in int f) { x = y = f; }
		public Vector2i(in int x, in int y) { this.x = x; this.y = y; }
		public Vector2i(in int[] v2) { x = v2[0]; y = v2[1]; }

		public Vector2i(float x, float y) : this((int)x, (int)y) {
		}

		[Exposed]
		public static readonly Vector2i Zero = new(0, 0);
		[Exposed]
		static public readonly Vector2i One = new(1, 1);
		[Exposed]
		static public readonly Vector2i AxisX = new(1, 0);
		[Exposed]
		static public readonly Vector2i AxisY = new(0, 1);

		
		public int this[in int key]
		{
			get => (key == 0) ? x : y;
			set {
				if (key == 0) { x = value; }
				else {
					y = value;
				}
			}
		}

		
		public int[] Array => new int[] { x, y };

		public void Add(in int s) { x += s; y += s; }


		
		public int LengthSquared => (x * x) + (y * y);


		public static Vector2i operator -(in Vector2i v) => new(-v.x, -v.y);

		public static Vector2i operator *(in int f, in Vector2i v) => new(f * v.x, f * v.y);
		public static Vector2i operator *(in Vector2i v, in int f) => new(f * v.x, f * v.y);
		public static Vector2i operator /(in Vector2i v, in int f) => new(v.x / f, v.y / f);
		public static Vector2i operator /(in int f, in Vector2i v) => new(f / v.x, f / v.y);

		public static Vector2i operator *(in Vector2i a, in Vector2i b) => new(a.x * b.x, a.y * b.y);
		public static Vector2i operator /(in Vector2i a, in Vector2i b) => new(a.x / b.x, a.y / b.y);


		public static Vector2i operator +(in Vector2i v0, in Vector2i v1) => new(v0.x + v1.x, v0.y + v1.y);
		public static Vector2i operator +(in Vector2i v0, in int f) => new(v0.x + f, v0.y + f);

		public static Vector2i operator -(in Vector2i v0, in Vector2i v1) => new(v0.x - v1.x, v0.y - v1.y);
		public static Vector2i operator -(in Vector2i v0, in int f) => new(v0.x - f, v0.y - f);



		public static bool operator ==(in Vector2i a, in Vector2i b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(in Vector2i a, in Vector2i b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj) {
			return this == (Vector2i)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y);
		}

		public int CompareTo(Vector2i other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector2i other) {
			return x == other.x && y == other.y;
		}



		public override string ToString() {
			return string.Format("{0} {1}", x, y);
		}

		public static TypeCode GetTypeCode() {
			return TypeCode.Object;
		}
	}











	public struct Vector2l : IComparable<Vector2l>, IEquatable<Vector2l>, ISerlize<Vector2l>
	{
		public long x;
		public long y;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadInt64();
			y = binaryReader.ReadInt64();
		}

		public Vector2l() {
			x = 0;
			y = 0;
		}
		public Vector2l(long f) { x = y = f; }
		public Vector2l(long x, long y) { this.x = x; this.y = y; }
		public Vector2l(long[] v2) { x = v2[0]; y = v2[1]; }
		[Exposed]
		static public readonly Vector2l Zero = new(0, 0);
		[Exposed]
		static public readonly Vector2l One = new(1, 1);
		[Exposed]
		static public readonly Vector2l AxisX = new(1, 0);
		[Exposed]
		static public readonly Vector2l AxisY = new(0, 1);

		
		public long this[long key]
		{
			get => (key == 0) ? x : y;
			set {
				if (key == 0) { x = value; }
				else {
					y = value;
				}
			}
		}

		
		public long[] Array => new long[] { x, y };

		public void Add(long s) { x += s; y += s; }




		public static Vector2l operator -(Vector2l v) => new(-v.x, -v.y);

		public static Vector2l operator *(long f, Vector2l v) => new(f * v.x, f * v.y);
		public static Vector2l operator *(Vector2l v, long f) => new(f * v.x, f * v.y);
		public static Vector2l operator /(Vector2l v, long f) => new(v.x / f, v.y / f);
		public static Vector2l operator /(long f, Vector2l v) => new(f / v.x, f / v.y);

		public static Vector2l operator *(Vector2l a, Vector2l b) => new(a.x * b.x, a.y * b.y);
		public static Vector2l operator /(Vector2l a, Vector2l b) => new(a.x / b.x, a.y / b.y);


		public static Vector2l operator +(Vector2l v0, Vector2l v1) => new(v0.x + v1.x, v0.y + v1.y);
		public static Vector2l operator +(Vector2l v0, long f) => new(v0.x + f, v0.y + f);

		public static Vector2l operator -(Vector2l v0, Vector2l v1) => new(v0.x - v1.x, v0.y - v1.y);
		public static Vector2l operator -(Vector2l v0, long f) => new(v0.x - f, v0.y - f);



		public static bool operator ==(Vector2l a, Vector2l b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(Vector2l a, Vector2l b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj) {
			return this == (Vector2l)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y);
		}

		public int CompareTo(Vector2l other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector2l other) {
			return x == other.x && y == other.y;
		}



		public override string ToString() {
			return string.Format("{0} {1}", x, y);
		}
	}



}
