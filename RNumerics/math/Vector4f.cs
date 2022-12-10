using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector4f : IComparable<Vector4f>, IEquatable<Vector4f>
	{
		[Key(0)]
		public float x;
		[Key(1)]
		public float y;
		[Key(2)]
		public float z;
		[Key(3)]
		public float w;

		[Exposed, IgnoreMember]
		public float X
		{
			get => x;
			set => x = value;
		}
		[Exposed, IgnoreMember]
		public float Y
		{
			get => y;
			set => y = value;
		}
		[Exposed, IgnoreMember]
		public float Z
		{
			get => z;
			set => z = value;
		}
		[Exposed, IgnoreMember]
		public float W
		{
			get => w;
			set => w = value;
		}

		public Vector4f() {
			x = 0f;
			y = 0f;
			z = 0f;
			w = 0f;
		}
		public Vector4 ToSystem() {
			return new Vector4(x, y, z, w);
		}
		public Vector4f(in Vector3f a, in float w) { x = a.x; y = a.y; z = a.z; this.w = w; }

		public Vector4f(in float f) { x = y = z = w = f; }
		public Vector4f(in float x, in float y, in float z, in float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Vector4f(in float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
		public Vector4f(in Vector4f copy) { x = copy.x; y = copy.y; z = copy.z; w = copy.w; }
		[Exposed,IgnoreMember]
		public static readonly Vector4f Zero = new(0.0f, 0.0f, 0.0f, 0.0f);
		[Exposed,IgnoreMember]
		public static readonly Vector4f One = new(1.0f, 1.0f, 1.0f, 1.0f);

		[IgnoreMember]
		public float this[in int key]
		{
			get => (key < 2) ? ((key == 0) ? x : y) : ((key == 2) ? z : w);
			set {
				if (key < 2) {
					if (key == 0) { x = value; }
					else {
						y = value;
					}
				}
				else {
					if (key == 2) { z = value; }
					else {
						w = value;
					}
				}
			}
		}

		[IgnoreMember]
		public float LengthSquared => (x * x) + (y * y) + (z * z) + (w * w);
		[IgnoreMember]
		public float Length => (float)Math.Sqrt(LengthSquared);

		[IgnoreMember]
		public float LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z) + Math.Abs(w);


		public float Normalize(in float epsilon = MathUtil.EPSILONF) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0f / length;
				x *= invLength;
				y *= invLength;
				z *= invLength;
				w *= invLength;
			}
			else {
				length = 0;
				x = y = z = w = 0;
			}
			return length;
		}
		[IgnoreMember]
		public Vector4f Normalized
		{
			get {
				var length = Length;
				if (length > MathUtil.EPSILON) {
					var invLength = 1.0f / length;
					return new Vector4f(x * invLength, y * invLength, z * invLength, w * invLength);
				}
				else {
					return Vector4f.Zero;
				}
			}
		}

		[IgnoreMember]
		public bool IsNormalized => Math.Abs((x * x) + (y * y) + (z * z) + (w * w) - 1) < MathUtil.ZERO_TOLERANCE;


		[IgnoreMember]
		public bool IsFinite
		{
			get { var f = x + y + z + w; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
		}

		public void Round(in int nDecimals) {
			x = (float)Math.Round(x, nDecimals);
			y = (float)Math.Round(y, nDecimals);
			z = (float)Math.Round(z, nDecimals);
			w = (float)Math.Round(w, nDecimals);
		}


		public float Dot(in Vector4f v2) {
			return (x * v2.x) + (y * v2.y) + (z * v2.z) + (w * v2.w);
		}


		public static float Dot(in Vector4f v1, in Vector4f v2) {
			return v1.Dot(v2);
		}


		public float AngleD(in Vector4f v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)Math.Acos(fDot) * MathUtil.RAD_2_DEGF;
		}
		public static float AngleD(in Vector4f v1, in Vector4f v2) {
			return v1.AngleD(v2);
		}
		public float AngleR(in Vector4f v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)Math.Acos(fDot);
		}
		public static float AngleR(in Vector4f v1, in Vector4f v2) {
			return v1.AngleR(v2);
		}

		public float DistanceSquared(in Vector4f v2) {
			float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
			return (dx * dx) + (dy * dy) + (dz * dz) + (dw * dw);
		}

		public float Distance(in Vector4f v2) {
			float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
			return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz) + (dw * dw));
		}



		public static Vector4f operator -(in Vector4f v) => new(-v.x, -v.y, -v.z, -v.w);

		public static Vector4f operator *(in float f, in Vector4f v) => new(f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4f operator *(in Vector4f v, in float f) => new(f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4f operator /(in Vector4f v, in float f) => new(v.x / f, v.y / f, v.z / f, v.w / f);
		public static Vector4f operator /(in float f, in Vector4f v) => new(f / v.x, f / v.y, f / v.z, f / v.w);

		public static Vector4f operator *(in Vector4f a,in  Vector4f b) => new(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		public static Vector4f operator /(in Vector4f a, in Vector4f b) => new(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);


		public static Vector4f operator +(in Vector4f v0,in Vector4f v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
		public static Vector4f operator +(in Vector4f v0, in float f) => new(v0.x + f, v0.y + f, v0.z + f, v0.w + f);

		public static Vector4f operator -(in Vector4f v0, in Vector4f v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
		public static Vector4f operator -(in Vector4f v0, in float f) => new(v0.x - f, v0.y - f, v0.z - f, v0.w - f);



		public static bool operator ==(in Vector4f a, in Vector4f b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(in Vector4f a, in Vector4f b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
		public override bool Equals(object obj) {
			return this == (Vector4f)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z, w);
		}

		public int CompareTo(Vector4f other) {
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
		public bool Equals(Vector4f other) {
			return x == other.x && y == other.y && z == other.z && w == other.w;
		}


		public bool EpsilonEqual(in Vector4f v2, in float epsilon) {
			return Math.Abs(x - v2.x) <= epsilon &&
				   Math.Abs(y - v2.y) <= epsilon &&
				   Math.Abs(z - v2.z) <= epsilon &&
				   Math.Abs(w - v2.w) <= epsilon;
		}



		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", x, y, z, w);
		}
		public string ToString(in string fmt) {
			return string.Format("{0} {1} {2} {3}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt), w.ToString(fmt));
		}

		public static unsafe Vector4f ToRhuNumrics(ref Vector4 value) {
			fixed (Vector4* vector3f = &value) {
				return *(Vector4f*)vector3f;
			}
		}

		public static unsafe explicit operator Vector4f(Vector4 v) => ToRhuNumrics(ref v);

		public static unsafe explicit operator Vector4(in Vector4f v) => v.ToSystemNumrics();

		public unsafe Vector4 ToSystemNumrics() {
			fixed (Vector4f* vector3f = &this) {
				return *(Vector4*)vector3f;
			}
		}

	}
}
