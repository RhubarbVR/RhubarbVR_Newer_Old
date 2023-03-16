using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.IO;

namespace RNumerics
{
	public struct Vector2f : IComparable<Vector2f>, IEquatable<Vector2f>, ISerlize<Vector2f>
	{
		public float x;
		public float y;

		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadSingle();
			y = binaryReader.ReadSingle();
		}

		[Exposed]
		public float X
		{
			get => x;
			set => x = value;
		}

		[Exposed]
		public float Y
		{
			get => y;
			set => y = value;
		}

		public Vector2f() {
			x = 0f;
			y = 0f;
		}
		public Vector2f(in float f) { x = y = f; }
		public Vector2f(in float x, in float y) { this.x = x; this.y = y; }
		public Vector2f(in float[] v2) { x = v2[0]; y = v2[1]; }
		public Vector2f(in double f) { x = y = (float)f; }
		public Vector2f(in double x, in double y) { this.x = (float)x; this.y = (float)y; }
		public Vector2f(in double[] v2) { x = (float)v2[0]; y = (float)v2[1]; }
		public Vector2f(in Vector2f copy) { x = copy[0]; y = copy[1]; }
		public Vector2f(in Vector2d copy) { x = (float)copy[0]; y = (float)copy[1]; }

		public static explicit operator Vector2f(in Vector2i v) => new(v.x, v.y);

		[Exposed]
		static public readonly Vector2f Zero = new(0.0f, 0.0f);
		[Exposed]
		static public readonly Vector2f Inf = new(float.PositiveInfinity, float.PositiveInfinity);
		[Exposed]
		static public readonly Vector2f NInf = new(float.NegativeInfinity, float.NegativeInfinity);
		[Exposed]
		static public readonly Vector2f One = new(1.0f, 1.0f);
		[Exposed]
		static public readonly Vector2f AxisX = new(1.0f, 0.0f);
		[Exposed]
		static public readonly Vector2f AxisY = new(0.0f, 1.0f);
		[Exposed]
		static public readonly Vector2f MaxValue = new(float.MaxValue, float.MaxValue);
		[Exposed]
		static public readonly Vector2f MinValue = new(float.MinValue, float.MinValue);
		public bool IsInBox(in Vector2f min, in Vector2f check) {
			return check.y >= min.y && check.y <= y && check.x >= min.x && check.x <= x;
		}
		public bool IsInBox(in Vector2f min, in Vector2d check) {
			return check.y >= min.y && check.y <= y && check.x >= min.x && check.x <= x;
		}
		
		public float this[in int key]
		{
			get => (key == 0) ? x : y;
			set {
				if (key == 0) { x = value; }
				else {
					y = value;
				}
			}
		}


		
#pragma warning disable IDE1006 // Naming Styles
		public Vector3f _Y_ => new(0, y,0);
#pragma warning restore IDE1006 // Naming Styles

		
		public Vector3f XY_ => new(x,y);

		
		public Vector3f X__ => new(x,0,0);

		
		public Vector2f YX => new(y, x);

		
		public float LengthSquared => (x * x) + (y * y);
		
		public float Length => (float)Math.Sqrt(LengthSquared);
		
		public float YAngleD => AngleD(AxisY);
		
		public float YAngleR => AngleR(AxisY);
		
		public float XAngleD => AngleD(AxisX);
		
		public float XAngleR => AngleR(AxisX);
		public float Normalize(in float epsilon = MathUtil.EPSILONF) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0f / length;
				x *= invLength;
				y *= invLength;
			}
			else {
				length = 0;
				x = y = 0;
			}
			return length;
		}
		
		public Vector2f Normalized
		{
			get {
				var length = Length;
				if (length > MathUtil.EPSILONF) {
					var invLength = 1 / length;
					return new Vector2f(x * invLength, y * invLength);
				}
				else {
					return Vector2f.Zero;
				}
			}
		}

		
		public Vector2f Clean => new (float.IsNaN(x)?0f:x, float.IsNaN(y) ? 0f : y);

		
		public bool IsNormalized => Math.Abs((x * x) + (y * y) - 1) < MathUtil.ZERO_TOLERANCEF;
		
		public bool IsFinite
		{
			get { var f = x + y; return !float.IsNaN(f) && !float.IsInfinity(f); }
		}

		public void Round(in int nDecimals) {
			x = (float)Math.Round(x, nDecimals);
			y = (float)Math.Round(y, nDecimals);
		}
		public float Dot(in Vector2f v2) {
			return (x * v2.x) + (y * v2.y);
		}


		public float Cross(in Vector2f v2) {
			return (x * v2.y) - (y * v2.x);
		}

		
		public Vector2f Perp => new(y, -x);

		public bool IsWithIn(in Vector2f min, in Vector2f max) {
			return MathUtil.Clamp(this, min, max) == this;
		}

		
		public Vector2f UnitPerp => new Vector2f(y, -x).Normalized;

		public float DotPerp(in Vector2f v2) {
			return (x * v2.y) - (y * v2.x);
		}


		public float AngleD(in Vector2f v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)(Math.Acos(fDot) * MathUtil.RAD_2_DEG);
		}
		public static float AngleD(in Vector2f v1, in Vector2f v2) {
			return v1.AngleD(v2);
		}
		public float AngleR(in Vector2f v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return (float)Math.Acos(fDot);
		}
		public static float AngleR(in Vector2f v1, in Vector2f v2) {
			return v1.AngleR(v2);
		}


		[Exposed]
		public float DistanceSquared(in Vector2f v2) {
			float dx = v2.x - x, dy = v2.y - y;
			return (dx * dx) + (dy * dy);
		}
		[Exposed]
		public float Distance(in Vector2f v2) {
			float dx = v2.x - x, dy = v2.y - y;
			return (float)Math.Sqrt((dx * dx) + (dy * dy));
		}
		[Exposed]
		public float Distance(in Vector2d v2) {
			return Distance((Vector2f)v2);
		}
		[Exposed]
		public void Set(in Vector2f o) {
			x = o.x;
			y = o.y;
		}
		[Exposed]
		public void Set(in float fX, in float fY) {
			x = fX;
			y = fY;
		}
		[Exposed]
		public void Add(in Vector2f o) {
			x += o.x;
			y += o.y;
		}
		[Exposed]
		public void Subtract(in Vector2f o) {
			x -= o.x;
			y -= o.y;
		}
		[Exposed]
		public static Vector2f MinMaxIntersect(in Vector2d point, in Vector2f min, in Vector2f max) {
			return MinMaxIntersect((Vector2f)point, min, max);
		}
		[Exposed]
		public static Vector2f MinMaxIntersect(in Vector2f point, in Vector2f min, in Vector2f max) {
			return new Vector2f(Math.Max(Math.Min(point.x,max.x),min.x), Math.Max(Math.Min(point.y, max.y), min.y));
		}
		[Exposed]
		public static Vector2f? Intersect(in Vector2d line1V1, in Vector2d line1V2, in Vector2f line2V1, in Vector2f line2V2) {
			var A1 = line1V2.y - line1V1.y;
			var B1 = line1V1.x - line1V2.x;
			var C1 = (A1 * line1V1.x) + (B1 * line1V1.y);
			var A2 = line2V2.y - line2V1.y;
			var B2 = line2V1.x - line2V2.x;
			var C2 = (A2 * line2V1.x) + (B2 * line2V1.y);
			var det = (A1 * B2) - (A2 * B1);
			if (det == 0) {
				return null;
			}
			else {
				var x = ((B2 * C1) - (B1 * C2)) / det;
				var y = ((A1 * C2) - (A2 * C1)) / det;
				return new Vector2f(x, y);
			}
		}
		[Exposed]
		public static Vector2f? Intersect(in Vector2f line1V1, in Vector2f line1V2, in Vector2f line2V1, in Vector2f line2V2) {
			var A1 = line1V2.y - line1V1.y;
			var B1 = line1V1.x - line1V2.x;
			var C1 = (A1 * line1V1.x) + (B1 * line1V1.y);
			var A2 = line2V2.y - line2V1.y;
			var B2 = line2V1.x - line2V2.x;
			var C2 = (A2 * line2V1.x) + (B2 * line2V1.y);
			var det = (A1 * B2) - (A2 * B1);
			if (det == 0) {
				return Zero;
			}
			else {
				var x = ((B2 * C1) - (B1 * C2)) / det;
				var y = ((A1 * C2) - (A2 * C1)) / det;
				return new Vector2f(x, y);
			}
		}
		public static Vector2f operator -(in Vector2f v) => new(-v.x, -v.y);

		public static Vector2f operator +(in Vector2f a, in Vector2f o) => new(a.x + o.x, a.y + o.y);
		public static Vector2f operator +(in Vector2f a, in float f) => new(a.x + f, a.y + f);

		public static Vector2f operator -(in Vector2f a, in Vector2f o) => new(a.x - o.x, a.y - o.y);
		public static Vector2f operator -(in Vector2f a, in float f) => new(a.x - f, a.y - f);

		public static Vector2f operator *(in Vector2f a, in float f) => new(a.x * f, a.y * f);
		public static Vector2f operator *(in float f, in Vector2f a) => new(a.x * f, a.y * f);
		public static Vector2f operator /(in Vector2f v, in float f) => new(v.x / f, v.y / f);
		public static Vector2f operator /(in float f, in Vector2f v) => new(f / v.x, f / v.y);

		public static Vector2f operator *(in Vector2f a, in Vector2f b) => new(a.x * b.x, a.y * b.y);
		public static Vector2f operator /(in Vector2f a, in Vector2f b) => new(a.x / b.x, a.y / b.y);

		public static bool operator >(in Vector2f a, in Vector2f b) => a.x > b.x || a.y > b.y;
		public static bool operator <(in Vector2f a, in Vector2f b) => a.x < b.x || a.y < b.y;
		public static bool operator >=(in Vector2f a, in Vector2f b) => a.x >= b.x && a.y >= b.y;
		public static bool operator <=(in Vector2f a, in Vector2f b) => a.x <= b.x && a.y <= b.y;
		public static bool operator ==(in Vector2f a, in Vector2f b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(in Vector2f a, in Vector2f b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj) {
			return this == (Vector2f)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y);
		}
		[Exposed]
		public Vector2f ClosestPointOnLine(in Vector2f first, in Vector2f next) {
			var thisfirst = this - first;
			var nextfitst = next - first;
			var magnextfitst = nextfitst.LengthSquared;
			var nextfirstpro = thisfirst.Dot(nextfitst);
			var dist = nextfirstpro / magnextfitst;
			return first + (nextfitst * dist);
		}

		public int CompareTo(Vector2f other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector2f other) {
			return x == other.x && y == other.y;
		}


		public bool EpsilonEqual(in Vector2f v2, in float epsilon) {
			return (float)Math.Abs(x - v2.x) <= epsilon &&
				   (float)Math.Abs(y - v2.y) <= epsilon;
		}


		public static Vector2f Lerp(in Vector2f a, in Vector2f b, in float t) {
			var s = 1 - t;
			return new Vector2f((s * a.x) + (t * b.x), (s * a.y) + (t * b.y));
		}


		public override string ToString() {
			return string.Format("{0:F8} {1:F8}", x, y);
		}

		public unsafe Vector2 ToSystemNumric() {
			fixed (Vector2f* vector3f = &this) {
				return *(Vector2*)vector3f;
			}
		}

		public static unsafe Vector2f ToRhuNumrics(ref Vector2 value) {
			fixed (Vector2* vector3f = &value) {
				return *(Vector2f*)vector3f;
			}
		}
		[Exposed]
		public static Vector2f GetUVPosOnTry(in Vector3d p1, in Vector2f p1uv, in Vector3d p2, in Vector2f p2uv, in Vector3d p3, in Vector2f p3uv, in Vector3d point) {
			// calculate vectors from point f to vertices p1, p2 and p3:
			var f1 = p1 - point;
			var f2 = p2 - point;
			var f3 = p3 - point;
			// calculate the areas (parameters order is essential in this case):
			var va = Vector3d.Cross(p1 - p2, p1 - p3); // main triangle cross product
			var va1 = Vector3d.Cross(f2, f3); // p1's triangle cross product
			var va2 = Vector3d.Cross(f3, f1); // p2's triangle cross product
			var va3 = Vector3d.Cross(f1, f2); // p3's triangle cross product
			var a = va.Magnitude; // main triangle area
								  // calculate barycentric coordinates with sign:
			var a1 = va1.Magnitude / a * Math.Sign(Vector3d.Dot(va, va1));
			var a2 = va2.Magnitude / a * Math.Sign(Vector3d.Dot(va, va2));
			var a3 = va3.Magnitude / a * Math.Sign(Vector3d.Dot(va, va3));
			// find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
			var uv = (p1uv * (float)a1) + (p2uv * (float)a2) + (p3uv * (float)a3);
			return uv;
		}


		public static explicit operator Vector2(in Vector2f b) => b.ToSystemNumric();

		public static explicit operator Vector2f(Vector2 b) => ToRhuNumrics(ref b);

	}
}
