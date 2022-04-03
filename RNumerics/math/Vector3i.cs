using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// 3D integer vector type. This is basically the same as Index3i but
	/// with .x.y.z member names. This makes code far more readable in many places.
	/// Unfortunately I can't see a way to do this w/o so much duplication...we could
	/// have .x/.y/.z accessors but that is much less efficient...
	/// </summary>
	[MessagePackObject]
	public struct Vector3i : IComparable<Vector3i>, IEquatable<Vector3i>
	{
		[Key(0)]
		public int x;
		[Key(1)]
		public int y;
		[Key(2)]
		public int z;

		public Vector3i(int f) { x = y = z = f; }
		public Vector3i(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
		public Vector3i(int[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }
		[IgnoreMember]
		static public readonly Vector3i Zero = new(0, 0, 0);
		[IgnoreMember]
		static public readonly Vector3i One = new(1, 1, 1);
		[IgnoreMember]
		static public readonly Vector3i AxisX = new(1, 0, 0);
		[IgnoreMember]
		static public readonly Vector3i AxisY = new(0, 1, 0);
		[IgnoreMember]
		static public readonly Vector3i AxisZ = new(0, 0, 1);

		[IgnoreMember]
		public int this[int key]
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

		[IgnoreMember]
		public int[] Array => new int[] { x, y, z };



		public void Set(Vector3i o) {
			x = o.x;
			y = o.y;
			z = o.z;
		}
		public void Set(int fX, int fY, int fZ) {
			x = fX;
			y = fY;
			z = fZ;
		}
		public void Add(Vector3i o) {
			x += o.x;
			y += o.y;
			z += o.z;
		}
		public void Subtract(Vector3i o) {
			x -= o.x;
			y -= o.y;
			z -= o.z;
		}
		public void Add(int s) { x += s; y += s; z += s; }


		[IgnoreMember]
		public int LengthSquared => (x * x) + (y * y) + (z * z);


		public static Vector3i operator -(Vector3i v) => new(-v.x, -v.y, -v.z);

		public static Vector3i operator *(int f, Vector3i v) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3i operator *(Vector3i v, int f) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3i operator /(Vector3i v, int f) => new(v.x / f, v.y / f, v.z / f);
		public static Vector3i operator /(int f, Vector3i v) => new(f / v.x, f / v.y, f / v.z);

		public static Vector3i operator *(Vector3i a, Vector3i b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
		public static Vector3i operator /(Vector3i a, Vector3i b) => new(a.x / b.x, a.y / b.y, a.z / b.z);


		public static Vector3i operator +(Vector3i v0, Vector3i v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
		public static Vector3i operator +(Vector3i v0, int f) => new(v0.x + f, v0.y + f, v0.z + f);

		public static Vector3i operator -(Vector3i v0, Vector3i v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
		public static Vector3i operator -(Vector3i v0, int f) => new(v0.x - f, v0.y - f, v0.z - f);




		public static bool operator ==(Vector3i a, Vector3i b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(Vector3i a, Vector3i b) => a.x != b.x || a.y != b.y || a.z != b.z;
		public override bool Equals(object obj) {
			return this == (Vector3i)obj;
		}
		public override int GetHashCode() {
			unchecked {
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				return hash;
			}
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
		public static implicit operator Vector3i(Index3i v) => new(v.a, v.b, v.c);
		public static implicit operator Index3i(Vector3i v) => new(v.x, v.y, v.z);

		// explicit cast to double/float vector types
		public static explicit operator Vector3i(Vector3f v) => new((int)v.x, (int)v.y, (int)v.z);
		public static explicit operator Vector3f(Vector3i v) => new((float)v.x, (float)v.y, (float)v.z);
		public static explicit operator Vector3i(Vector3d v) => new((int)v.x, (int)v.y, (int)v.z);
		public static explicit operator Vector3d(Vector3i v) => new(v.x, v.y, v.z);

	}
}
