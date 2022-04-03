using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector3f : IComparable<Vector3f>, IEquatable<Vector3f>
	{
		[Key(0)]
		public float x;
		[Key(1)]
		public float y;
		[Key(2)]
		public float z;
		[IgnoreMember]
		public float Magnitude => (float)Math.Sqrt((x * x) + (y * y) + (z * z));
		public Vector3f(float f) { x = y = z = f; }
		public Vector3f(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
		public Vector3f(float x, float y) { this.x = x; this.y = y; z = 0f; }
		public Vector3f(float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }
		public Vector3f(Vector3f copy) { x = copy.x; y = copy.y; z = copy.z; }

		public Vector3f(double f) { x = y = z = (float)f; }
		public Vector3f(double x, double y, double z) { this.x = (float)x; this.y = (float)y; this.z = (float)z; }
		public Vector3f(double[] v2) { x = (float)v2[0]; y = (float)v2[1]; z = (float)v2[2]; }

		public float Distance(Vector3 v2) {
			float dx = v2.X - x, dy = v2.Y - y, dz = v2.Z - z;
			return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
		}

		public Vector3f(Vector3d copy) { x = (float)copy.x; y = (float)copy.y; z = (float)copy.z; }
		[IgnoreMember]
		public static readonly Vector3f Zero = new(0.0f, 0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector3f One = new(1.0f, 1.0f, 1.0f);
		[IgnoreMember]
		static public readonly Vector3f OneNormalized = new Vector3f(1.0f, 1.0f, 1.0f).Normalized;
		[IgnoreMember]
		static public readonly Vector3f Invalid = new(float.MaxValue, float.MaxValue, float.MaxValue);
		[IgnoreMember]
		static public readonly Vector3f AxisX = new(1.0f, 0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector3f AxisY = new(0.0f, 1.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector3f AxisZ = new(0.0f, 0.0f, 1.0f);
		[IgnoreMember]
		static public readonly Vector3f MaxValue = new(float.MaxValue, float.MaxValue, float.MaxValue);
		[IgnoreMember]
		static public readonly Vector3f MinValue = new(float.MinValue, float.MinValue, float.MinValue);
		[IgnoreMember]
		/// <summary>A vector representing the up axis. In StereoKit, this is
		/// the same as `new Vec3(0,1,0)`.</summary>
		public static readonly Vector3f Up = new (0, 1, 0);

		[IgnoreMember]
		/// <summary>StereoKit uses a right-handed coordinate system, which
		/// means that forward is looking down the -Z axis! This value is the
		/// same as `new Vec3(0,0,-1)`. This is NOT the same as UnitZ!
		/// </summary>
		public static readonly Vector3f Forward = new (0, 0, -1);
		[IgnoreMember]
		/// <summary>When looking forward, this is the direction to the 
		/// right! In StereoKit, this is the same as `new Vec3(1,0,0)`
		/// </summary>
		public static readonly Vector3f Right = new (1, 0, 0);
		[IgnoreMember]
		public float this[int key]
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
		public Vector2f Xy
		{
			get => new(x, y);
			set { x = value.x; y = value.y; }
		}
		[IgnoreMember]
		public Vector2f Xz
		{
			get => new(x, z);
			set { x = value.x; z = value.y; }
		}
		[IgnoreMember]
		public Vector2f Yz
		{
			get => new(y, z);
			set { y = value.x; z = value.y; }
		}
		[IgnoreMember]

		public float LengthSquared => (x * x) + (y * y) + (z * z);
		[IgnoreMember]
		public float Length => (float)Math.Sqrt(LengthSquared);
		[IgnoreMember]
		public float LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
		[IgnoreMember]
		public float Max => Math.Max(x, Math.Max(y, z));
		[IgnoreMember]
		public float Min => Math.Min(x, Math.Min(y, z));
		[IgnoreMember]
		public float MaxAbs => Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
		[IgnoreMember]
		public float MinAbs => Math.Min(Math.Abs(x), Math.Min(Math.Abs(y), Math.Abs(z)));


		public float Normalize(float epsilon = MathUtil.EPSILONF) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0f / length;
				x *= invLength;
				y *= invLength;
				z *= invLength;
			}
			else {
				length = 0;
				x = y = z = 0;
			}
			return length;
		}
		[IgnoreMember]
		public Vector3f Normalized
		{
			get {
				var length = Length;
				if (length > MathUtil.EPSILONF) {
					var invLength = 1 / length;
					return new Vector3f(x * invLength, y * invLength, z * invLength);
				}
				else {
					return Vector3f.Zero;
				}
			}
		}
		[IgnoreMember]
		public bool IsNormalized => Math.Abs((x * x) + (y * y) + (z * z) - 1) < MathUtil.ZERO_TOLERANCEF;
		[IgnoreMember]

		public bool IsFinite
		{
			get { var f = x + y + z; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
		}
		[IgnoreMember]
		public float SqrMagnitude => (x * x) + (y * y) + (z * z);
		public Vector3f RmoveSmallest() {
			var smallist = Math.Min(x, Math.Min(y, z));
			return y == smallist ? new Vector3f(x, 0, z) : x == smallist ? new Vector3f(0, y, z) : new Vector3f(x, y, 0);
		}
		public Vector3f SetComponent(float value, int index) {
			return index switch {
				0 => new Vector3f(value, y, z),
				1 => new Vector3f(x, value, z),
				2 => new Vector3f(x, y, value),
				_ => throw new ArgumentException("Invalid vector index"),
			};
		}

		public void Round(int nDecimals) {
			x = (float)Math.Round(x, nDecimals);
			y = (float)Math.Round(y, nDecimals);
			z = (float)Math.Round(z, nDecimals);
		}


		public float Dot(Vector3f v2) {
			return (x * v2[0]) + (y * v2[1]) + (z * v2[2]);
		}
		public static float Dot(Vector3f v1, Vector3f v2) {
			return v1.Dot(v2);
		}


		public Vector3f Cross(Vector3f v2) {
			return new Vector3f(
				(y * v2.z) - (z * v2.y),
				(z * v2.x) - (x * v2.z),
				(x * v2.y) - (y * v2.x));
		}
		public static Vector3f Cross(Vector3f v1, Vector3f v2) {
			return v1.Cross(v2);
		}

		public Vector3f UnitCross(Vector3f v2) {
			var n = new Vector3f(
				(y * v2.z) - (z * v2.y),
				(z * v2.x) - (x * v2.z),
				(x * v2.y) - (y * v2.x));
			n.Normalize();
			return n;
		}

		public float AngleD(Vector3f v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)(Math.Acos(fDot) * MathUtil.RAD_2_DEG);
		}
		public static float AngleD(Vector3f v1, Vector3f v2) {
			return v1.AngleD(v2);
		}
		public float AngleR(Vector3f v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)Math.Acos(fDot);
		}
		public static float AngleR(Vector3f v1, Vector3f v2) {
			return v1.AngleR(v2);
		}


		public float DistanceSquared(Vector3f v2) {
			float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z;
			return (dx * dx) + (dy * dy) + (dz * dz);
		}
		public float Distance(Vector3f v2) {
			float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z;
			return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
		}



		public void Set(Vector3f o) {
			x = o[0];
			y = o[1];
			z = o[2];
		}
		public void Set(float fX, float fY, float fZ) {
			x = fX;
			y = fY;
			z = fZ;
		}
		public void Add(Vector3f o) {
			x += o[0];
			y += o[1];
			z += o[2];
		}
		public void Subtract(Vector3f o) {
			x -= o[0];
			y -= o[1];
			z -= o[2];
		}



		public static Vector3f operator -(Vector3f v) => new(-v.x, -v.y, -v.z);

		public static Vector3f operator *(float f, Vector3f v) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3f operator *(Vector3f v, float f) => new(f * v.x, f * v.y, f * v.z);
		public static Vector3f operator /(Vector3f v, float f) => new(v.x / f, v.y / f, v.z / f);
		public static Vector3f operator /(float f, Vector3f v) => new(f / v.x, f / v.y, f / v.z);

		public static Vector3f operator *(Vector3f a, Vector3f b) => new(a.x * b.x, a.y * b.y, a.z * b.z);
		public static Vector3f operator /(Vector3f a, Vector3f b) => new(a.x / b.x, a.y / b.y, a.z / b.z);


		public static Vector3f operator +(Vector3f v0, Vector3f v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
		public static Vector3f operator +(Vector3f v0, float f) => new(v0.x + f, v0.y + f, v0.z + f);

		public static Vector3f operator -(Vector3f v0, Vector3f v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
		public static Vector3f operator -(Vector3f v0, float f) => new(v0.x - f, v0.y - f, v0.z - f);


		public static bool operator ==(Vector3f a, Vector3f b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(Vector3f a, Vector3f b) => a.x != b.x || a.y != b.y || a.z != b.z;

		public static bool operator >(Vector3f a, Vector3f b) => a.x > b.x || a.y > b.y || a.z > b.z;
		public static bool operator <(Vector3f a, Vector3f b) => a.x < b.x || a.y < b.y || a.z < b.z;

		public override bool Equals(object obj) {
			return this == (Vector3f)obj;
		}
		public override int GetHashCode() {
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				return hash;
			}
		}
		public int CompareTo(Vector3f other) {
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
		public bool Equals(Vector3f other) {
			return x == other.x && y == other.y && z == other.z;
		}


		public bool EpsilonEqual(Vector3f v2, float epsilon) {
			return (float)Math.Abs(x - v2.x) <= epsilon &&
				   (float)Math.Abs(y - v2.y) <= epsilon &&
				   (float)Math.Abs(z - v2.z) <= epsilon;
		}


		public static Vector3f Lerp(Vector3f a, Vector3f b, float t) {
			var s = 1 - t;
			return new Vector3f((s * a.x) + (t * b.x), (s * a.y) + (t * b.y), (s * a.z) + (t * b.z));
		}



		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8}", x, y, z);
		}
		public string ToString(string fmt) {
			return string.Format("{0} {1} {2}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt));
		}


		public unsafe Vector3 ToSystemNumrics() {
			fixed (Vector3f* vector3f = &this) {
				return *(Vector3*)vector3f;
			}
		}
		public static unsafe Vector3f ToRhuNumrics(ref Vector3 value) {
			fixed (Vector3* vector3f = &value) {
				return *(Vector3f*)vector3f;
			}
		}

		public static implicit operator Vector3(Vector3f b) => b.ToSystemNumrics();

		public static implicit operator Vector3f(Vector3 b) => ToRhuNumrics(ref b);

	}
}
