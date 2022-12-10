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
	public struct Vector3u : IComparable<Vector3u>, IEquatable<Vector3u>
	{
		[Key(0)]
		public uint x;
		[Key(1)]
		public uint y;
		[Key(2)]
		public uint z;

		[Exposed, IgnoreMember]
		public uint X
		{
			get => x;
			set => x = value;
		}
		[Exposed, IgnoreMember]
		public uint Y
		{
			get => y;
			set => y = value;
		}
		[Exposed, IgnoreMember]
		public uint Z
		{
			get => z;
			set => z = value;
		}
		public Vector3u() {
			x = 0;
			y = 0;
			z = 0;
		}

		public Vector3u(in int f) { x = y = z = (uint)f; }
		public Vector3u(in int x, in int y, in int z) { this.x = (uint)x; this.y = (uint)y; this.z = (uint)z; }

		public Vector3u(in uint f) { x = y = z = f; }
		public Vector3u(in uint x, in uint y, in uint z) { this.x = x; this.y = y; this.z = z; }
		public Vector3u(in uint[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }
		[Exposed,IgnoreMember]
		static public readonly Vector3u Zero = new(0, 0, 0);
		[Exposed,IgnoreMember]
		static public readonly Vector3u One = new(1, 1, 1);
		[Exposed,IgnoreMember]
		static public readonly Vector3u AxisX = new(1, 0, 0);
		[Exposed,IgnoreMember]
		static public readonly Vector3u AxisY = new(0, 1, 0);
		[Exposed,IgnoreMember]
		static public readonly Vector3u AxisZ = new(0, 0, 1);

		[IgnoreMember]
		public uint this[in uint key]
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
		public uint[] Array => new uint[] { x, y, z };



		public void Set(in Vector3u o) {
			x = o.x;
			y = o.y;
			z = o.z;
		}
		public void Set(in uint fX, in uint fY, in uint fZ) {
			x = fX;
			y = fY;
			z = fZ;
		}
		public void Add(in Vector3u o) {
			x += o.x;
			y += o.y;
			z += o.z;
		}
		public void Subtract(in Vector3u o) {
			x -= o.x;
			y -= o.y;
			z -= o.z;
		}
		public void Add(in uint s) { x += s; y += s; z += s; }


		[IgnoreMember]
		public uint LengthSquared => (x * x) + (y * y) + (z * z);


		public static Vector3u operator -(in Vector3u v) => new((uint)-(int)v.x, (uint)-(int)v.y, (uint)-(int)v.z);

		public static Vector3u operator *(in uint f, in Vector3u v) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3u operator *(in Vector3u v, in uint f) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3u operator /(in Vector3u v, in uint f) => new(v.x / f, v.y / f, v.z / f);
		public static Vector3u operator /(in uint f, in Vector3u v) => new(f / v.x, f / v.y, f / v.z);

		public static Vector3u operator *(in Vector3u a, in Vector3u b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
		public static Vector3u operator /(in Vector3u a, in Vector3u b) => new(a.x / b.x, a.y / b.y, a.z / b.z);


		public static Vector3u operator +(in Vector3u v0, in Vector3u v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
		public static Vector3u operator +(in Vector3u v0, in uint f) => new(v0.x + f, v0.y + f, v0.z + f);

		public static Vector3u operator -(in Vector3u v0, in Vector3u v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
		public static Vector3u operator -(in Vector3u v0, in uint f) => new(v0.x - f, v0.y - f, v0.z - f);




		public static bool operator ==(in Vector3u a, in Vector3u b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(in Vector3u a, in Vector3u b) => a.x != b.x || a.y != b.y || a.z != b.z;
		public override bool Equals(object obj) {
			return this == (Vector3u)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z);
		}

		public int CompareTo(Vector3u other) {
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
		public bool Equals(Vector3u other) {
			return x == other.x && y == other.y && z == other.z;
		}



		public override string ToString() {
			return string.Format("{0} {1} {2}", x, y, z);
		}

		// implicit cast between Index3i and Vector3i
		public static implicit operator Vector3u(in Index3i v) => new(v.a, v.b, v.c);
		public static implicit operator Vector3u(in Vector3i v) => new(v.x, v.y, v.z);
	}
}
