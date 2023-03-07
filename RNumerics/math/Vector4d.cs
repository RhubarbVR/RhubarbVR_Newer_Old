using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RNumerics
{
	public struct Vector4d : IComparable<Vector4d>, IEquatable<Vector4d>, ISerlize<Vector4d>
	{
		public double x;
		public double y;
		public double z;
		public double w;


		public void Serlize(BinaryWriter binaryWriter) {
			binaryWriter.Write(x);
			binaryWriter.Write(y);
			binaryWriter.Write(z);
			binaryWriter.Write(w);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			x = binaryReader.ReadDouble();
			y = binaryReader.ReadDouble();
			z = binaryReader.ReadDouble();
			w = binaryReader.ReadDouble();
		}

		[Exposed]
		public double X
		{
			get => x;
			set => x = value;
		}
		[Exposed]
		public double Y
		{
			get => y;
			set => y = value;
		}
		[Exposed]
		public double Z
		{
			get => z;
			set => z = value;
		}
		[Exposed]
		public double W
		{
			get => w;
			set => w = value;
		}
		public Vector4d() {
			x = 0;
			y = 0;
			z = 0;
			w = 0;
		}

		public Vector4d(in double f) { x = y = z = w = f; }
		public Vector4d(in double x, in double y, in double z, in double w) { this.x = x; this.y = y; this.z = z; this.w = w; }
		public Vector4d(in double[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
		public Vector4d(in Vector4d copy) { x = copy.x; y = copy.y; z = copy.z; w = copy.w; }
		[Exposed]
		static public readonly Vector4d Zero = new(0.0f, 0.0f, 0.0f, 0.0f);
		[Exposed]
		static public readonly Vector4d One = new(1.0f, 1.0f, 1.0f, 1.0f);

		
		public double this[in int key]
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

		
		public double LengthSquared => (x * x) + (y * y) + (z * z) + (w * w);
		
		public double Length => Math.Sqrt(LengthSquared);

		
		public double LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z) + Math.Abs(w);


		public double Normalize(in double epsilon = MathUtil.EPSILON) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0 / length;
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
		public Vector4d Normalized
		{
			get {
				var length = Length;
				if (length > MathUtil.EPSILON) {
					var invLength = 1.0 / length;
					return new Vector4d(x * invLength, y * invLength, z * invLength, w * invLength);
				}
				else {
					return Vector4d.Zero;
				}
			}
		}

		public bool IsNormalized => Math.Abs((x * x) + (y * y) + (z * z) + (w * w) - 1) < MathUtil.ZERO_TOLERANCE;


		public bool IsFinite
		{
			get { var f = x + y + z + w; return double.IsNaN(f) == false && double.IsInfinity(f) == false; }
		}

		public void Round(in int nDecimals) {
			x = Math.Round(x, nDecimals);
			y = Math.Round(y, nDecimals);
			z = Math.Round(z, nDecimals);
			w = Math.Round(w, nDecimals);
		}


		public double Dot(in Vector4d v2) {
			return (x * v2.x) + (y * v2.y) + (z * v2.z) + (w * v2.w);
		}


		public static double Dot(in Vector4d v1, in Vector4d v2) {
			return v1.Dot(v2);
		}


		public double AngleD(in Vector4d v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return Math.Acos(fDot) * MathUtil.RAD_2_DEG;
		}
		public static double AngleD(in Vector4d v1, in Vector4d v2) {
			return v1.AngleD(v2);
		}
		public double AngleR(in Vector4d v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return Math.Acos(fDot);
		}
		public static double AngleR(in Vector4d v1, in Vector4d v2) {
			return v1.AngleR(v2);
		}

		public double DistanceSquared(in Vector4d v2) {
			double dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
			return (dx * dx) + (dy * dy) + (dz * dz) + (dw * dw);
		}

		public double Distance(in Vector4d v2) {
			double dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
			return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz) + (dw * dw));
		}



		public static Vector4d operator -(in Vector4d v) => new(-v.x, -v.y, -v.z, -v.w);

		public static Vector4d operator *(in double f, in Vector4d v) => new(f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4d operator *(in Vector4d v, in double f) => new(f * v.x, f * v.y, f * v.z, f * v.w);
		public static Vector4d operator /(in Vector4d v, in double f) => new(v.x / f, v.y / f, v.z / f, v.w / f);
		public static Vector4d operator /(in double f, in Vector4d v) => new(f / v.x, f / v.y, f / v.z, f / v.w);

		public static Vector4d operator *(in Vector4d a,in Vector4d b) => new(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		public static Vector4d operator /(in Vector4d a, in Vector4d b) => new(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);


		public static Vector4d operator +(in Vector4d v0, in Vector4d v1) => new(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
		public static Vector4d operator +(in Vector4d v0, in double f) => new(v0.x + f, v0.y + f, v0.z + f, v0.w + f);

		public static Vector4d operator -(in Vector4d v0, in Vector4d v1) => new(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
		public static Vector4d operator -(in Vector4d v0, in double f) => new(v0.x - f, v0.y - f, v0.z - f, v0.w - f);



		public static bool operator ==(in Vector4d a, in Vector4d b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(in Vector4d a, in Vector4d b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
		public override bool Equals(object obj) {
			return this == (Vector4d)obj;
		}
		public override int GetHashCode() {
			return HashCode.Combine(x, y, z, w);
		}

		public int CompareTo(Vector4d other) {
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
		public bool Equals(Vector4d other) {
			return x == other.x && y == other.y && z == other.z && w == other.w;
		}


		public bool EpsilonEqual(in Vector4d v2, in double epsilon) {
			return Math.Abs(x - v2.x) <= epsilon &&
				   Math.Abs(y - v2.y) <= epsilon &&
				   Math.Abs(z - v2.z) <= epsilon &&
				   Math.Abs(w - v2.w) <= epsilon;
		}



		public override string ToString() {
			return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", x, y, z, w);
		}
		public string ToString(string fmt) {
			return string.Format("{0} {1} {2} {3}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt), w.ToString(fmt));
		}

	}
}
