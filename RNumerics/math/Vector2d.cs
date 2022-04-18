using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Vector2d : IComparable<Vector2d>, IEquatable<Vector2d>
	{
		[Key(0)]
		public double x;
		[Key(1)]
		public double y;

		public override int GetHashCode() {
			unchecked // Overflow is fine, just wrap
			{
				var hash = (int)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				return hash;
			}
		}
		[IgnoreMember]
		public Vector2d Clean => new Vector2d(double.IsNaN(x)?0:x, double.IsNaN(y) ? 0 : y);

		public Vector2d(double f) { x = y = f; }
		public Vector2d(double x, double y) { this.x = x; this.y = y; }
		public Vector2d(double[] v2) { x = v2[0]; y = v2[1]; }
		public Vector2d(float f) { x = y = f; }
		public Vector2d(float x, float y) { this.x = x; this.y = y; }
		public Vector2d(float[] v2) { x = v2[0]; y = v2[1]; }
		public Vector2d(Vector2d copy) { x = copy.x; y = copy.y; }
		public Vector2d(Vector2f copy) { x = copy.x; y = copy.y; }
		[IgnoreMember]
		static public readonly Vector2d Zero = new(0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector2d One = new(1.0f, 1.0f);
		[IgnoreMember]
		static public readonly Vector2d AxisX = new(1.0f, 0.0f);
		[IgnoreMember]
		static public readonly Vector2d AxisY = new(0.0f, 1.0f);
		[IgnoreMember]
		static public readonly Vector2d MaxValue = new(double.MaxValue, double.MaxValue);
		[IgnoreMember]
		static public readonly Vector2d MinValue = new(double.MinValue, double.MinValue);

		public static Vector2d FromAngleRad(double angle) {
			return new Vector2d(Math.Cos(angle), Math.Sin(angle));
		}
		public static Vector2d FromAngleDeg(double angle) {
			angle *= MathUtil.DEG_2_RAD;
			return new Vector2d(Math.Cos(angle), Math.Sin(angle));
		}


		[IgnoreMember]
		public double this[int key]
		{
			get => (key == 0) ? x : y;
			set {
				if (key == 0) { x = value; }
				else {
					y = value;
				}
			}
		}


		[IgnoreMember]
		public double LengthSquared => (x * x) + (y * y);
		[IgnoreMember]
		public double Length => (double)Math.Sqrt(LengthSquared);

		public double Normalize(double epsilon = MathUtil.EPSILON) {
			var length = Length;
			if (length > epsilon) {
				var invLength = 1.0 / length;
				x *= invLength;
				y *= invLength;
			}
			else {
				length = 0;
				x = y = 0;
			}
			return length;
		}
		[IgnoreMember]
		public Vector2d Normalized
		{
			get {
				var length = Length;
				if (length > MathUtil.EPSILON) {
					var invLength = 1 / length;
					return new Vector2d(x * invLength, y * invLength);
				}
				else {
					return Zero;
				}
			}
		}

		[IgnoreMember]
		public bool IsNormalized => Math.Abs((x * x) + (y * y) - 1) < MathUtil.ZERO_TOLERANCE;

		[IgnoreMember]
		public bool IsFinite
		{
			get { var f = x + y; return double.IsNaN(f) == false && double.IsInfinity(f) == false; }
		}

		public void Round(int nDecimals) {
			x = Math.Round(x, nDecimals);
			y = Math.Round(y, nDecimals);
		}


		public double Dot(Vector2d v2) {
			return (x * v2.x) + (y * v2.y);
		}


		/// <summary>
		/// returns cross-product of this vector with v2 (same as DotPerp)
		/// </summary>
		public double Cross(Vector2d v2) {
			return (x * v2.y) - (y * v2.x);
		}


		/// <summary>
		/// returns right-perp vector, ie rotated 90 degrees to the right
		/// </summary>
		[IgnoreMember]
		public Vector2d Perp => new(y, -x);

		/// <summary>
		/// returns right-perp vector, ie rotated 90 degrees to the right
		/// </summary>
		[IgnoreMember]
		public Vector2d UnitPerp => new Vector2d(y, -x).Normalized;

		/// <summary>
		/// returns dot-product of this vector with v2.Perp
		/// </summary>
		public double DotPerp(Vector2d v2) {
			return (x * v2.y) - (y * v2.x);
		}


		public double AngleD(Vector2d v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return Math.Acos(fDot) * MathUtil.RAD_2_DEG;
		}
		public static double AngleD(Vector2d v1, Vector2d v2) {
			return v1.AngleD(v2);
		}
		public double AngleR(Vector2d v2) {
			var fDot = MathUtil.Clamp(Dot(v2), -1, 1);
			return Math.Acos(fDot);
		}
		public static double AngleR(Vector2d v1, Vector2d v2) {
			return v1.AngleR(v2);
		}



		public double DistanceSquared(Vector2d v2) {
			double dx = v2.x - x, dy = v2.y - y;
			return (dx * dx) + (dy * dy);
		}
		public double Distance(Vector2d v2) {
			double dx = v2.x - x, dy = v2.y - y;
			return Math.Sqrt((dx * dx) + (dy * dy));
		}


		public void Set(Vector2d o) {
			x = o.x;
			y = o.y;
		}
		public void Set(double fX, double fY) {
			x = fX;
			y = fY;
		}
		public void Add(Vector2d o) {
			x += o.x;
			y += o.y;
		}
		public void Subtract(Vector2d o) {
			x -= o.x;
			y -= o.y;
		}



		public static Vector2d operator -(Vector2d v) => new(-v.x, -v.y);

		public static Vector2d operator +(Vector2d a, Vector2d o) => new(a.x + o.x, a.y + o.y);
		public static Vector2d operator +(Vector2d a, double f) => new(a.x + f, a.y + f);

		public static Vector2d operator -(Vector2d a, Vector2d o) => new(a.x - o.x, a.y - o.y);
		public static Vector2d operator -(Vector2d a, double f) => new(a.x - f, a.y - f);

		public static Vector2d operator *(Vector2d a, double f) => new(a.x * f, a.y * f);
		public static Vector2d operator *(double f, Vector2d a) => new(a.x * f, a.y * f);
		public static Vector2d operator /(Vector2d v, double f) => new(v.x / f, v.y / f);
		public static Vector2d operator /(double f, Vector2d v) => new(f / v.x, f / v.y);


		public static Vector2d operator *(Vector2d a, Vector2d b) => new(a.x * b.x, a.y * b.y);
		public static Vector2d operator /(Vector2d a, Vector2d b) => new(a.x / b.x, a.y / b.y);


		public static bool operator ==(Vector2d a, Vector2d b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(Vector2d a, Vector2d b) => a.x != b.x || a.y != b.y;
		public override bool Equals(object obj) {
			return this == (Vector2d)obj;
		}

		public int CompareTo(Vector2d other) {
			if (x != other.x) {
				return x < other.x ? -1 : 1;
			}
			else if (y != other.y) {
				return y < other.y ? -1 : 1;
			}

			return 0;
		}
		public bool Equals(Vector2d other) {
			return x == other.x && y == other.y;
		}


		public bool EpsilonEqual(Vector2d v2, double epsilon) {
			return Math.Abs(x - v2.x) <= epsilon &&
				   Math.Abs(y - v2.y) <= epsilon;
		}


		public static Vector2d Lerp(Vector2d a, Vector2d b, double t) {
			var s = 1 - t;
			return new Vector2d((s * a.x) + (t * b.x), (s * a.y) + (t * b.y));
		}
		public static Vector2d Lerp(ref Vector2d a, ref Vector2d b, double t) {
			var s = 1 - t;
			return new Vector2d((s * a.x) + (t * b.x), (s * a.y) + (t * b.y));
		}


		public override string ToString() {
			return string.Format("{0:F8} {1:F8}", x, y);
		}


		public static implicit operator Vector2d(Vector2f v) => new(v.x, v.y);
		public static explicit operator Vector2f(Vector2d v) => new((float)v.x, (float)v.y);


		// from WildMagic5 Vector2, used in ConvexHull2

		public struct Information
		{
			// The intrinsic dimension of the input set.  The parameter 'epsilon'
			// to the GetInformation function is used to provide a tolerance when
			// determining the dimension.
			public int mDimension;

			// Axis-aligned bounding box of the input set.  The maximum range is
			// the larger of max[0]-min[0] and max[1]-min[1].
			public Vector2d mMin;
			public Vector2d mMax;
			public double mMaxRange;

			// Coordinate system.  The origin is valid for any dimension d.  The
			// unit-length direction vector is valid only for 0 <= i < d.  The
			// extreme index is relative to the array of input points, and is also
			// valid only for 0 <= i < d.  If d = 0, all points are effectively
			// the same, but the use of an epsilon may lead to an extreme index
			// that is not zero.  If d = 1, all points effectively lie on a line
			// segment.  If d = 2, the points are not collinear.
			public Vector2d mOrigin;
			public Vector2d mDirection0;
			public Vector2d mDirection1;

			// The indices that define the maximum dimensional extents.  The
			// values mExtreme[0] and mExtreme[1] are the indices for the points
			// that define the largest extent in one of the coordinate axis
			// directions.  If the dimension is 2, then mExtreme[2] is the index
			// for the point that generates the largest extent in the direction
			// perpendicular to the line through the points corresponding to
			// mExtreme[0] and mExtreme[1].  The triangle formed by the points
			// V[extreme0], V[extreme1], and V[extreme2] is clockwise or
			// counterclockwise, the condition stored in mExtremeCCW.
			public Vector3i mExtreme;
			public bool mExtremeCCW;
		};


		// The value of epsilon is used as a relative error when computing the
		// dimension of the point set.
		public static void GetInformation(IList<Vector2d> points, double epsilon, out Information info) {
			info = new Information();
			var numPoints = points.Count;
			if (numPoints == 0 || points == null || epsilon <= 0) {
				System.Diagnostics.Debug.Assert(false);
				return;
			}

			info.mExtremeCCW = false;

			// Compute the axis-aligned bounding box for the input points.  Keep track
			// of the indices into 'points' for the current min and max.
			int j;
			var indexMin = Vector2i.Zero;
			var indexMax = Vector2i.Zero;
			for (j = 0; j < 2; ++j) {
				info.mMin[j] = points[0][j];
				info.mMax[j] = info.mMin[j];
				indexMin[j] = 0;
				indexMax[j] = 0;
			}

			int i;
			for (i = 1; i < numPoints; ++i) {
				for (j = 0; j < 2; ++j) {
					if (points[i][j] < info.mMin[j]) {
						info.mMin[j] = points[i][j];
						indexMin[j] = i;
					}
					else if (points[i][j] > info.mMax[j]) {
						info.mMax[j] = points[i][j];
						indexMax[j] = i;
					}
				}
			}

			// Determine the maximum range for the bounding box.
			info.mMaxRange = info.mMax[0] - info.mMin[0];
			info.mExtreme[0] = indexMin[0];
			info.mExtreme[1] = indexMax[0];
			var range = info.mMax[1] - info.mMin[1];
			if (range > info.mMaxRange) {
				info.mMaxRange = range;
				info.mExtreme[0] = indexMin[1];
				info.mExtreme[1] = indexMax[1];
			}

			// The origin is either the point of minimum x-value or point of
			// minimum y-value.
			info.mOrigin = points[info.mExtreme[0]];

			// Test whether the point set is (nearly) a point.
			if (info.mMaxRange < epsilon) {
				info.mDimension = 0;
				info.mDirection0 = Vector2d.Zero;
				info.mDirection1 = Vector2d.Zero;
				for (j = 0; j < 2; ++j) {
					info.mExtreme[j + 1] = info.mExtreme[0];
				}

				return;
			}

			// Test whether the point set is (nearly) a line segment.
			info.mDirection0 = points[info.mExtreme[1]] - info.mOrigin;
			info.mDirection0.Normalize();
			info.mDirection1 = -info.mDirection0.Perp;
			var maxDistance = (double)0;
			var maxSign = (double)0;
			info.mExtreme[2] = info.mExtreme[0];
			for (i = 0; i < numPoints; ++i) {
				var diff = points[i] - info.mOrigin;
				var distance = info.mDirection1.Dot(diff);
				double sign = Math.Sign(distance);
				distance = Math.Abs(distance);
				if (distance > maxDistance) {
					maxDistance = distance;
					maxSign = sign;
					info.mExtreme[2] = i;
				}
			}

			if (maxDistance < epsilon * info.mMaxRange) {
				info.mDimension = 1;
				info.mExtreme[2] = info.mExtreme[1];
				return;
			}

			info.mDimension = 2;
			info.mExtremeCCW = maxSign > (double)0;
		}
	}
}