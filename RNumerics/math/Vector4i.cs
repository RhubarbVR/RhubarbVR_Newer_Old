using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// 3D integer vector type. This is basically the same as Index3i but
	/// with .x.y.z member names. This makes code far more readable in many places.
	/// Unfortunately I can't see a way to do this w/o so much duplication...we could
	/// have .x/.y/.z accessors but that is much less efficient...
	/// </summary>
	public struct Vector4i : IComparable<Vector4i>, IEquatable<Vector4i>, ISerlize<Vector4i>
	{
		public int x;
		public int y;
		public int z;
		public int w;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
			binaryWriter.Write(z);
			binaryWriter.Write(w);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadInt32();
			y = binaryReader.ReadInt32();
			z = binaryReader.ReadInt32();
			w = binaryReader.ReadInt32();
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
		[Exposed]
		public int Z
		{
			get => z;
			set => z = value;
		}
		[Exposed]
		public int W
		{
			get => w;
			set => w = value;
		}
		public Vector4i() {
			x = 0;
			y = 0;
			z = 0;
			w = 0;
		}

		public Vector4i(in int f) { x = y = z = w = f; }
		public Vector4i(in int x, in int y, in int z, in int w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Vector4i(in int[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }

		[Exposed]
		static public readonly Vector4i Zero = new(0, 0, 0, 0);
		[Exposed]
		static public readonly Vector4i One = new(1, 1, 1, 1);
		[Exposed]
		static public readonly Vector4i AxisX = new(1, 0, 0, 0);
		[Exposed]
		static public readonly Vector4i AxisY = new(0, 1, 0, 0);
		[Exposed]
		static public readonly Vector4i AxisZ = new(0, 0, 1, 0);
		[Exposed]
		static public readonly Vector4i AxisW = new(0, 0, 0, 1);

		
		public int this[in int key]
		{
			get => (key == 0) ? x : (key == 1) ? y : (key == 2) ? w : z;
			set { if (key == 0) { x = value; } else if (key == 1) { y = value; } else if (key == 3) { w = value; } else { z = value; }; }
		}

		
		public int[] Array => new int[] { x, y, z, w };



		public void Set(in Vector4i o) {
			x = o.x;
			y = o.y;
			z = o.z;
			w = o.w;
		}
		public void Set(in int fX, in int fY, in int fZ, in int fW) {
			x = fX;
			y = fY;
			z = fZ;
			w = fW;
		}
		public void Add(in Vector4i o) {
			x += o.x;
			y += o.y;
			z += o.z;
			w += o.w;
		}
		public void Subtract(in Vector4i o) {
			x -= o.x;
			y -= o.y;
			z -= o.z;
			w -= o.w;
		}
		public void Add(in int s) { x += s; y += s; z += s; w += s; }


		
		public int LengthSquared => (x * x) + (y * y) + (z * z);


		public static Vector4i operator -(in Vector4i v) => new(-v.x, -v.y, -v.z, -v.w);

		public static Vector4i operator *(in int f, in Vector4i v) => new(f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4i operator *(in Vector4i v, in int f) => new(f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4i operator /(in Vector4i v, in int f) => new(v.x / f, v.y / f, v.z / f, v.w / f);
		public static Vector4i operator /(in int f, in Vector4i v) => new(f / v.x, f / v.y, f / v.z, v.w / f);

		public static Vector4i operator *(in Vector4i a, in Vector4i b) => new(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		public static Vector4i operator /(in Vector4i a, in Vector4i b) => new(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);


		public static Vector4i operator +(in Vector4i v0, in Vector4i v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
		public static Vector4i operator +(in Vector4i v0, in int f) => new(v0.x + f, v0.y + f, v0.z + f, v0.w + f);

		public static Vector4i operator -(in Vector4i v0, in Vector4i v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
		public static Vector4i operator -(in Vector4i v0, in int f) => new(v0.x - f, v0.y - f, v0.z - f, v0.w - f);




		public static bool operator ==(in Vector4i a, in Vector4i b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(in Vector4i a, in Vector4i b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
		public override bool Equals(object obj) {
			return this == (Vector4i)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z, w);
		}
		public int CompareTo(Vector4i other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}
			else if (z != other.z) {
				return z < other.z ? -1 : 1;
			}
			else if (w != other.w) {
				return w < other.w ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector4i other) {
			return x == other.x && y == other.y && z == other.z && w == other.w;
		}



		public override string ToString() {
			return string.Format("{0} {1} {2}", x, y, z);
		}



		// implicit cast between Index4i and Vector4i
		public static implicit operator Vector4i(in Index4i v) => new(v.a, v.b, v.c, v.d);
		public static implicit operator Index4i(in Vector4i v) => new(v.x, v.y, v.z, v.w);
	}
}
