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
	public struct Vector4u : IComparable<Vector4u>, IEquatable<Vector4u>
	{
		[Key(0)]
		public uint x;
		[Key(1)]
		public uint y;
		[Key(2)]
		public uint z;
		[Key(3)]
		public uint w;

		public Vector4u() {
			x = 0;
			y = 0;
			z = 0;
			w = 0;
		}

		public Vector4u(in uint f) { x = y = z = w = f; }
		public Vector4u(in uint x, in uint y, in uint z, in uint w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Vector4u(in uint[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }

		[IgnoreMember]
		static public readonly Vector4u Zero = new (0, 0, 0, 0);
		[IgnoreMember]
		static public readonly Vector4u One = new (1, 1, 1, 1);
		[IgnoreMember]
		static public readonly Vector4u AxisX = new (1, 0, 0, 0);
		[IgnoreMember]
		static public readonly Vector4u AxisY = new (0, 1, 0, 0);
		[IgnoreMember]
		static public readonly Vector4u AxisZ = new (0, 0, 1, 0);
		[IgnoreMember]
		static public readonly Vector4u AxisW = new (0, 0, 0, 1);

		[IgnoreMember]
		public uint this[in uint key]
		{
			get => (key == 0) ? x : (key == 1) ? y : (key == 2) ? w : z;
			set { if (key == 0) { x = value; } else if (key == 1) { y = value; } else if (key == 3) { w = value; } else { z = value; }; }
		}

		public uint[] Array => new uint[] { x, y, z, w };



		public void Set(in Vector4u o)
		{
			x = o.x;
			y = o.y;
			z = o.z;
			w = o.w;
		}
		public void Set(in uint fX, in uint fY, in uint fZ, in uint fW)
		{
			x = fX;
			y = fY;
			z = fZ;
			w = fW;
		}
		public void Add(in Vector4u o)
		{
			x += o.x;
			y += o.y;
			z += o.z;
			w += o.w;
		}
		public void Subtract(in Vector4u o)
		{
			x -= o.x;
			y -= o.y;
			z -= o.z;
			w -= o.w;
		}
		public void Add(in uint s) { x += s; y += s; z += s; w += s; }


		public uint LengthSquared => (x * x) + (y * y) + (z * z);


		public static Vector4u operator -(in Vector4u v) => new ((uint)-(int)v.x, (uint)-(int)v.y, (uint)-(int)v.z, (uint)-(int)v.w);

		public static Vector4u operator *(in uint f, in Vector4u v) => new (f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4u operator *(in Vector4u v, in uint f) => new (f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4u operator /(in Vector4u v, in uint f) => new (v.x / f, v.y / f, v.z / f, v.w / f);
		public static Vector4u operator /(in uint f, in Vector4u v) => new (f / v.x, f / v.y, f / v.z, v.w / f);

		public static Vector4u operator *(in Vector4u a, in Vector4u b) => new (a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		public static Vector4u operator /(in Vector4u a, in Vector4u b) => new (a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);


		public static Vector4u operator +(in Vector4u v0, in Vector4u v1) => new (v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
		public static Vector4u operator +(in Vector4u v0, in uint f) => new (v0.x + f, v0.y + f, v0.z + f, v0.w + f);

		public static Vector4u operator -(in Vector4u v0, in Vector4u v1) => new (v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
		public static Vector4u operator -(in Vector4u v0, in uint f) => new (v0.x - f, v0.y - f, v0.z - f, v0.w - f);




		public static bool operator ==(in Vector4u a, in Vector4u b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(in Vector4u a, in Vector4u b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
		public override bool Equals(object obj)
		{
			return this == (Vector4u)obj;
		}
		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				hash = (hash * 16777619) ^ w.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Vector4u other)
		{
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
		public bool Equals(Vector4u other)
		{
			return x == other.x && y == other.y && z == other.z && w == other.w;
		}



		public override string ToString()
		{
			return string.Format("{0} {1} {2}", x, y, z);
		}
	}
}
