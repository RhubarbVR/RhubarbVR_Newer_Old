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
	public struct Vector3i : IComparable<Vector3i>, IEquatable<Vector3i>, ISerlize<Vector3i>
	{
		public int x;
		public int y;
		public int z;


		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
			binaryWriter.Write(z);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadInt32();
			y = binaryReader.ReadInt32();
			z = binaryReader.ReadInt32();
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
		public Vector3i() {
			x = 0;
			y = 0;
			z = 0;
		}

		public Vector3i(in int f) { x = y = z = f; }
		public Vector3i(in int x, in int y, in int z) { this.x = x; this.y = y; this.z = z; }
		public Vector3i(in int[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }
		[Exposed]
		static public readonly Vector3i Zero = new(0, 0, 0);
		[Exposed]
		static public readonly Vector3i One = new(1, 1, 1);
		[Exposed]
		static public readonly Vector3i AxisX = new(1, 0, 0);
		[Exposed]
		static public readonly Vector3i AxisY = new(0, 1, 0);
		[Exposed]
		static public readonly Vector3i AxisZ = new(0, 0, 1);

		
		public int this[in int key]
		{
			get => (key == 0) ? x : (key == 1) ? y : z;
			set {
				if (key == 0) { x = value; }
				else if (key == 1) { y = value; }
				else {
					z = value;
				}
			}
		}

		
		public int[] Array => new int[] { x, y, z };



		public void Set(in Vector3i o) {
			x = o.x;
			y = o.y;
			z = o.z;
		}
		public void Set(in int fX, in int fY, in int fZ) {
			x = fX;
			y = fY;
			z = fZ;
		}
		public void Add(in Vector3i o) {
			x += o.x;
			y += o.y;
			z += o.z;
		}
		public void Subtract(in Vector3i o) {
			x -= o.x;
			y -= o.y;
			z -= o.z;
		}
		public void Add(in int s) { x += s; y += s; z += s; }


		
		public int LengthSquared => (x * x) + (y * y) + (z * z);


		public static Vector3i operator -(in Vector3i v) => new(-v.x, -v.y, -v.z);
										   
		public static Vector3i operator *(in int f, in Vector3i v) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3i operator *(in Vector3i v, in int f) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3i operator /(in Vector3i v, in int f) => new(v.x / f, v.y / f, v.z / f);
		public static Vector3i operator /(in int f, in Vector3i v) => new(f / v.x, f / v.y, f / v.z);
										   
		public static Vector3i operator *(in Vector3i a, in Vector3i b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
		public static Vector3i operator /(in Vector3i a, in Vector3i b) => new(a.x / b.x, a.y / b.y, a.z / b.z);
										   
										   
		public static Vector3i operator +(in Vector3i v0, in Vector3i v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
		public static Vector3i operator +(in Vector3i v0, in int f) => new(v0.x + f, v0.y + f, v0.z + f);
										   
		public static Vector3i operator -(in Vector3i v0, in Vector3i v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
		public static Vector3i operator -(in Vector3i v0, in int f) => new(v0.x - f, v0.y - f, v0.z - f);




		public static bool operator ==(in Vector3i a, in Vector3i b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(in Vector3i a, in Vector3i b) => a.x != b.x || a.y != b.y || a.z != b.z;
		public override bool Equals(object obj) {
			return this == (Vector3i)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z);
		}

		public int CompareTo(Vector3i other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}
			else if (z != other.z) {
				return z < other.z ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector3i other) {
			return x == other.x && y == other.y && z == other.z;
		}



		public override string ToString() {
			return string.Format("{0} {1} {2}", x, y, z);
		}


		// implicit cast between Index3i and Vector3i
		public static implicit operator Vector3i(in Index3i v) => new(v.a, v.b, v.c);
		public static implicit operator Index3i(in Vector3i v) => new(v.x, v.y, v.z);

		// explicit cast to double/float vector types
		public static explicit operator Vector3i(in Vector3f v) => new((int)v.x, (int)v.y, (int)v.z);
		public static explicit operator Vector3f(in Vector3i v) => new((float)v.x, (float)v.y, (float)v.z);
		public static explicit operator Vector3i(in Vector3d v) => new((int)v.x, (int)v.y, (int)v.z);
		public static explicit operator Vector3d(in Vector3i v) => new(v.x, v.y, v.z);

	}
}
