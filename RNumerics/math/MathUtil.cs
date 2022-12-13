using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace RNumerics
{

	public static class MathUtil
	{

		public const double DEG_2_RAD = Math.PI / 180.0;
		public const double RAD_2_DEG = 180.0 / Math.PI;
		public const double TWO_PI = 2.0 * Math.PI;
		public const double FOUR_PI = 4.0 * Math.PI;
		public const double HALF_PI = 0.5 * Math.PI;
		public const double ZERO_TOLERANCE = 1e-08;
		public const double EPSILON = 2.2204460492503131e-016;
		public const double SQRT_TWO = 1.41421356237309504880168872420969807;
		public const double SQRT_TWO_INV = 1.0 / SQRT_TWO;
		public const double SQRT_THREE = 1.73205080756887729352744634150587236;

		public const float DEG_2_RADF = (float)(Math.PI / 180.0);
		public const float RAD_2_DEGF = (float)(180.0 / Math.PI);
		public const float P_IF = (float)Math.PI;
		public const float TWO_P_IF = 2.0f * P_IF;
		public const float HALF_P_IF = 0.5f * P_IF;
		public const float SQRT_TWOF = 1.41421356237f;

		public const float ZERO_TOLERANCEF = 1e-06f;
		public const float EPSILONF = 1.192092896e-07F;


		public static bool IsFinite(in double d) {
			return double.IsInfinity(d) == false && double.IsNaN(d) == false;
		}
		public static bool IsFinite(in float d) {
			return float.IsInfinity(d) == false && float.IsNaN(d) == false;
		}


		public static bool EpsilonEqual(in double a, in double b, in double epsilon = MathUtil.EPSILON) {
			return Math.Abs(a - b) <= epsilon;
		}
		public static bool EpsilonEqual(in float a, in float b, in float epsilon = MathUtil.EPSILONF) {
			return (float)Math.Abs(a - b) <= epsilon;
		}

		public static T Clamp<T>(in T f, in T low, in T high) {
			return ((RDynamic<T>)f < low) ? low : ((RDynamic<T>)f > high) ? high : f;
		}
		public static float Clamp(in float f, in float low, in float high) {
			return (f < low) ? low : (f > high) ? high : f;
		}
		public static double Clamp(in double f, in double low, in double high) {
			return (f < low) ? low : (f > high) ? high : f;
		}
		public static int Clamp(in int f, in int low, in int high) {
			return (f < low) ? low : (f > high) ? high : f;
		}
		public static Vector3f Clamp(in Vector3f f, in float low, in float high) {
			return new Vector3f(Clamp(f.x, low, high), Clamp(f.y, low, high), Clamp(f.z, low, high));
		}

		public static Vector2f Clamp(in Vector2f f, in Vector2f low, in Vector2f high) {
			return new Vector2f(Clamp(f.x, low.x, high.x), Clamp(f.y, low.y, high.y));
		}

		public static Vector2d Abs(in Vector2d vector2d) {
			return new Vector2d(Math.Abs(vector2d.x), Math.Abs(vector2d.y));
		}
		public static Vector2f Abs(in Vector2f vector2f) {
			return new Vector2f(Math.Abs(vector2f.x), Math.Abs(vector2f.y));
		}
		public static Vector2f Abs(in Vector2i vector2f) {
			return new Vector2f(Math.Abs(vector2f.x), Math.Abs(vector2f.y));
		}
		public static Vector3d Abs(in Vector3d vector2d) {
			return new Vector3d(Math.Abs(vector2d.x), Math.Abs(vector2d.y), Math.Abs(vector2d.z));
		}
		public static Vector3f Abs(in Vector3f vector2d) {
			return new Vector3f(Math.Abs(vector2d.x), Math.Abs(vector2d.y), Math.Abs(vector2d.z));
		}

		public static int ModuloClamp(int f, in int N) {
			while (f < 0) {
				f += N;
			}

			return f % N;
		}

		// fMinMaxValue may be signed
		public static float RangeClamp(in float fValue, in float fMinMaxValue) {
			return Clamp(fValue, -Math.Abs(fMinMaxValue), Math.Abs(fMinMaxValue));
		}
		public static double RangeClamp(in double fValue, in double fMinMaxValue) {
			return Clamp(fValue, -Math.Abs(fMinMaxValue), Math.Abs(fMinMaxValue));
		}


		public static float SignedClamp(in float f, in float fMax) {
			return Clamp(Math.Abs(f), 0, fMax) * Math.Sign(f);
		}
		public static double SignedClamp(in double f, in double fMax) {
			return Clamp(Math.Abs(f), 0, fMax) * Math.Sign(f);
		}

		public static float SignedClamp(in float f, in float fMin, in float fMax) {
			return Clamp(Math.Abs(f), fMin, fMax) * Math.Sign(f);
		}
		public static double SignedClamp(in double f, in double fMin, in double fMax) {
			return Clamp(Math.Abs(f), fMin, fMax) * Math.Sign(f);
		}


		public static bool InRange(in float f, in float low, in float high) {
			return f >= low && f <= high;
		}
		public static bool InRange(in double f, in double low, in double high) {
			return f >= low && f <= high;
		}
		public static bool InRange(in int f, in int low, in int high) {
			return f >= low && f <= high;
		}


		// clamps theta to angle interval [min,max]. should work for any theta,
		// regardless of cycles, however min & max values should be in range
		// [-360,360] and min < max
		public static double ClampAngleDeg(double theta, in double min, in double max) {
			// convert interval to center/extent - [c-e,c+e]
			var c = (min + max) * 0.5;
			var e = max - c;

			// get rid of extra rotations
			theta %= 360;

			// shift to origin, then convert theta to +- 180
			theta -= c;
			if (theta < -180) {
				theta += 360;
			}
			else if (theta > 180) {
				theta -= 360;
			}

			// clamp to extent
			if (theta < -e) {
				theta = -e;
			}
			else if (theta > e) {
				theta = e;
			}

			// shift back
			return theta + c;
		}



		// clamps theta to angle interval [min,max]. should work for any theta,
		// regardless of cycles, however min & max values should be in range
		// [-2_PI,2_PI] and min < max
		public static double ClampAngleRad(double theta, in double min, in double max) {
			// convert interval to center/extent - [c-e,c+e]
			var c = (min + max) * 0.5;
			var e = max - c;

			// get rid of extra rotations
			theta %= TWO_PI;

			// shift to origin, then convert theta to +- 180
			theta -= c;
			if (theta < -Math.PI) {
				theta += TWO_PI;
			}
			else if (theta > Math.PI) {
				theta -= TWO_PI;
			}

			// clamp to extent
			if (theta < -e) {
				theta = -e;
			}
			else if (theta > e) {
				theta = e;
			}

			// shift back
			return theta + c;
		}

		public static Vector2i Max(in Vector2i item, in Vector2i max) {
			return new Vector2i(Math.Max(item.x, max.x), Math.Max(item.y, max.y));
		}

		public static Vector2f Max(in Vector2f item, in Vector2f max) {
			return new Vector2f(Math.Max(item.x, max.x), Math.Max(item.y, max.y));
		}
		public static Vector2f Min(in Vector2f item, in Vector2f max) {
			return new Vector2f(Math.Min(item.x, max.x), Math.Min(item.y, max.y));
		}

		public static Vector2d Max(in Vector2d item, in Vector2d max) {
			return new Vector2d(Math.Max(item.x, max.x), Math.Max(item.y, max.y));
		}
		public static Vector2d Min(in Vector2d item, in Vector2d max) {
			return new Vector2d(Math.Min(item.x, max.x), Math.Min(item.y, max.y));
		}
		public static Vector3d Max(in Vector3d item, in Vector3d max) {
			return new Vector3d(Math.Max(item.x, max.x), Math.Max(item.y, max.y), Math.Max(item.z, max.z));
		}
		public static Vector3d Min(in Vector3d item, in Vector3d max) {
			return new Vector3d(Math.Min(item.x, max.x), Math.Min(item.y, max.y), Math.Min(item.z, max.z));
		}
		public static Vector3f Max(in Vector3f item, in Vector3f max) {
			return new Vector3f(Math.Max(item.x, max.x), Math.Max(item.y, max.y), Math.Max(item.z, max.z));
		}
		public static Vector3f Min(in Vector3f item, in Vector3f max) {
			return new Vector3f(Math.Min(item.x, max.x), Math.Min(item.y, max.y), Math.Min(item.z, max.z));
		}


		// for ((i++) % N)-type loops, but where we might be using (i--)
		public static int WrapSignedIndex(int val, in int mod) {
			while (val < 0) {
				val += mod;
			}

			return val % mod;
		}


		// compute min and max of a,b,c with max 3 comparisons (sometimes 2)
		public static void MinMax(in double a, in double b, in double c, out double min, out double max) {
			if (a < b) {
				if (a < c) {
					min = a;
					max = Math.Max(b, c);
				}
				else {
					min = c;
					max = b;
				}
			}
			else {
				if (a > c) {
					max = a;
					min = Math.Min(b, c);
				}
				else {
					min = b;
					max = c;
				}
			}
		}


		public static double Min(in double a, in double b, in double c) {
			return Math.Min(a, Math.Min(b, c));
		}
		public static float Min(in float a, in float b, in float c) {
			return Math.Min(a, Math.Min(b, c));
		}
		public static int Min(in int a, in int b, in int c) {
			return Math.Min(a, Math.Min(b, c));
		}
		public static double Max(in double a, in double b, in double c) {
			return Math.Max(a, Math.Max(b, c));
		}
		public static float Max(in float a, in float b, in float c) {
			return Math.Max(a, Math.Max(b, c));
		}
		public static int Max(in int a, in int b, in int c) {
			return Math.Max(a, Math.Max(b, c));
		}



		// there are fast approximations to this...
		public static double InvSqrt(in double f) {
			return f / Math.Sqrt(f);
		}


		// normal Atan2 returns in range [-pi,pi], this shifts to [0,2pi]
		public static double Atan2Positive(in double y, in double x) {
			var theta = Math.Atan2(y, x);
			if (theta < 0) {
				theta = (2 * Math.PI) + theta;
			}

			return theta;
		}


		public static float PlaneAngleD(Vector3f a, Vector3f b, in int nPlaneNormalIdx = 1) {
			a[nPlaneNormalIdx] = b[nPlaneNormalIdx] = 0.0f;
			a.Normalize();
			b.Normalize();
			return Vector3f.AngleD(a, b);
		}
		public static double PlaneAngleD(Vector3d a, Vector3d b, in int nPlaneNormalIdx = 1) {
			a[nPlaneNormalIdx] = b[nPlaneNormalIdx] = 0.0;
			a.Normalize();
			b.Normalize();
			return Vector3d.AngleD(a, b);
		}


		public static float PlaneAngleSignedD(Vector3f vFrom, Vector3f vTo, in int nPlaneNormalIdx = 1) {
			vFrom[nPlaneNormalIdx] = vTo[nPlaneNormalIdx] = 0.0f;
			vFrom.Normalize();
			vTo.Normalize();
			var c = vFrom.Cross(vTo);
			if (c.LengthSquared < MathUtil.ZERO_TOLERANCEF) {        // vectors are parallel
				return vFrom.Dot(vTo) < 0 ? 180.0f : 0;
			}
			float fSign = Math.Sign(c[nPlaneNormalIdx]);
			var fAngle = fSign * Vector3f.AngleD(vFrom, vTo);
			return fAngle;
		}
		public static double PlaneAngleSignedD(Vector3d vFrom, Vector3d vTo, in int nPlaneNormalIdx = 1) {
			vFrom[nPlaneNormalIdx] = vTo[nPlaneNormalIdx] = 0.0;
			vFrom.Normalize();
			vTo.Normalize();
			var c = vFrom.Cross(vTo);
			if (c.LengthSquared < MathUtil.ZERO_TOLERANCE) {        // vectors are parallel
				return vFrom.Dot(vTo) < 0 ? 180.0 : 0;
			}
			double fSign = Math.Sign(c[nPlaneNormalIdx]);
			var fAngle = fSign * Vector3d.AngleD(vFrom, vTo);
			return fAngle;
		}

		public static float PlaneAngleSignedD(Vector3f vFrom, Vector3f vTo, in Vector3f planeN) {
			vFrom -= Vector3f.Dot(vFrom, planeN) * planeN;
			vTo -= Vector3f.Dot(vTo, planeN) * planeN;
			vFrom.Normalize();
			vTo.Normalize();
			var c = Vector3f.Cross(vFrom, vTo);
			if (c.LengthSquared < MathUtil.ZERO_TOLERANCEF) {        // vectors are parallel
				return vFrom.Dot(vTo) < 0 ? 180.0f : 0;
			}
			float fSign = Math.Sign(Vector3f.Dot(c, planeN));
			var fAngle = fSign * Vector3f.AngleD(vFrom, vTo);
			return fAngle;
		}
		public static double PlaneAngleSignedD(Vector3d vFrom, Vector3d vTo, in Vector3d planeN) {
			vFrom -= Vector3d.Dot(vFrom, planeN) * planeN;
			vTo -= Vector3d.Dot(vTo, planeN) * planeN;
			vFrom.Normalize();
			vTo.Normalize();
			var c = Vector3d.Cross(vFrom, vTo);
			if (c.LengthSquared < MathUtil.ZERO_TOLERANCE) {        // vectors are parallel
				return vFrom.Dot(vTo) < 0 ? 180.0 : 0;
			}
			double fSign = Math.Sign(Vector3d.Dot(c, planeN));
			var fAngle = fSign * Vector3d.AngleD(vFrom, vTo);
			return fAngle;
		}


		public static float PlaneAngleSignedD(Vector2f vFrom, Vector2f vTo) {
			vFrom.Normalize();
			vTo.Normalize();
			float fSign = Math.Sign(vFrom.Cross(vTo));
			var fAngle = fSign * Vector2f.AngleD(vFrom, vTo);
			return fAngle;
		}
		public static double PlaneAngleSignedD(Vector2d vFrom, Vector2d vTo) {
			vFrom.Normalize();
			vTo.Normalize();
			double fSign = Math.Sign(vFrom.Cross(vTo));
			var fAngle = fSign * Vector2d.AngleD(vFrom, vTo);
			return fAngle;
		}



		public static int MostParallelAxis(in Frame3f f, in Vector3f vDir) {
			double dot0 = Math.Abs(f.X.Dot(vDir));
			double dot1 = Math.Abs(f.Y.Dot(vDir));
			double dot2 = Math.Abs(f.Z.Dot(vDir));
			var m = Math.Max(dot0, Math.Max(dot1, dot2));
			return (m == dot0) ? 0 : (m == dot1) ? 1 : 2;
		}

		public static T DynamicLerp<T>(T a, T b, double t) {
			try {
				try {
					return ((dynamic)a * (1.0 - t)) + ((dynamic)t * b);
				}
				catch {
					return ((dynamic)a * (float)(1.0 - t)) + ((float)t * (dynamic)b);
				}
			}
			catch {
				try {
					return (RDynamic<T>)(T)((dynamic)a * (1.0 - t)) + (RDynamic<T>)(T)(t * (dynamic)b);
				}
				catch {
					return (RDynamic<T>)(T)((dynamic)a * (float)(1.0 - t)) + (RDynamic<T>)(T)((float)t * (dynamic)b);
				}
			}
		}


		public static float Lerp(in float a, in float b, in float t) {
			return ((1.0f - t) * a) + (t * b);
		}
		public static double Lerp(in double a, in double b, in double t) {
			return ((1.0 - t) * a) + (t * b);
		}

		public static float SmoothStep(in float a, in float b, float t) {
			t = t * t * (3.0f - (2.0f * t));
			return ((1.0f - t) * a) + (t * b);
		}
		public static double SmoothStep(in double a, in double b, double t) {
			t = t * t * (3.0 - (2.0 * t));
			return ((1.0 - t) * a) + (t * b);
		}


		public static float SmoothInterp(in float a, in float b, in float t) {
			var tt = WyvillRise01(t);
			return ((1.0f - tt) * a) + (tt * b);
		}
		public static double SmoothInterp(in double a, in double b, in double t) {
			var tt = WyvillRise01(t);
			return ((1.0 - tt) * a) + (tt * b);
		}

		//! if yshift is 0, function approaches y=1 at xZero from y=0. 
		//! speed (> 0) controls how fast it gets there
		//! yshift pushes the whole graph upwards (so that it actually crosses y=1 at some point)
		public static float SmoothRise0To1(in float fX, in float yshift, in float xZero, in float speed) {
			var denom = Math.Pow(fX - (xZero - 1), speed);
			var fY = (float)(1 + yshift + (1 / -denom));
			return Clamp(fY, 0, 1);
		}

		public static float WyvillRise01(in float fX) {
			var d = Clamp(1.0f - (fX * fX), 0.0f, 1.0f);
			return 1 - (d * d * d);
		}
		public static double WyvillRise01(in double fX) {
			var d = Clamp(1.0 - (fX * fX), 0.0, 1.0);
			return 1 - (d * d * d);
		}

		public static float WyvillFalloff01(in float fX) {
			var d = 1 - (fX * fX);
			return d >= 0 ? (d * d * d) : 0;
		}
		public static double WyvillFalloff01(in double fX) {
			var d = 1 - (fX * fX);
			return d >= 0 ? (d * d * d) : 0;
		}


		public static float WyvillFalloff(float fD, in float fInnerRad, in float fOuterRad) {
			if (fD > fOuterRad) {
				return 0;
			}
			else if (fD > fInnerRad) {
				fD -= fInnerRad;
				fD /= fOuterRad - fInnerRad;
				fD = Math.Max(0, Math.Min(1, fD));
				var fVal = 1.0f - (fD * fD);
				return fVal * fVal * fVal;
			}
			else {
				return 1.0f;
			}
		}
		public static double WyvillFalloff(double fD, in double fInnerRad, in double fOuterRad) {
			if (fD > fOuterRad) {
				return 0;
			}
			else if (fD > fInnerRad) {
				fD -= fInnerRad;
				fD /= fOuterRad - fInnerRad;
				fD = Math.Max(0, Math.Min(1, fD));
				var fVal = 1.0f - (fD * fD);
				return fVal * fVal * fVal;
			}
			else {
				return 1.0;
			}
		}



		// lerps from [0,1] for x in range [deadzone,R]
		public static float LinearRampT(in float R, in float deadzoneR, float x) {
			float sign = Math.Sign(x);
			x = Math.Abs(x);
			if (x < deadzoneR) {
				return 0.0f;
			}
			else if (x > R) {
				return sign * 1.0f;
			}
			else {
				x = Math.Min(x, R);
				var d = (x - deadzoneR) / (R - deadzoneR);
				return sign * d;
			}
		}



		public static double Area(in Vector3d v1, in Vector3d v2, in Vector3d v3) {
			return 0.5 * (v2 - v1).Cross(v3 - v1).Length;
		}


		public static Vector3d Normal(in Vector3d v1, in Vector3d v2, in Vector3d v3) {
			var edge1 = v2 - v1;
			var edge2 = v3 - v2;
			edge1.Normalize();
			edge2.Normalize();
			var vCross = edge1.Cross(edge2);
			vCross.Normalize();
			return vCross;
		}


		/// <summary>
		/// compute vector in direction of triangle normal (cross-product). No normalization.
		/// </summary>
		/// <returns>The normal direction.</returns>
		public static Vector3d FastNormalDirection(in Vector3d v1, in Vector3d v2, in Vector3d v3) {
			var edge1 = v2 - v1;
			var edge2 = v3 - v1;
			return edge1.Cross(edge2);
		}


		/// <summary>
		/// simultaneously compute triangle normal and area, and only normalize after
		/// cross-product, not before (so, fewer normalizes then Normal())
		/// </summary>
		public static Vector3d FastNormalArea(in Vector3d v1, in Vector3d v2, in Vector3d v3, out double area) {
			var edge1 = v2 - v1;
			var edge2 = v3 - v1;
			var vCross = edge1.Cross(edge2);
			area = 0.5 * vCross.Normalize();
			return vCross;
		}


		/// <summary>
		/// aspect ratio of triangle 
		/// </summary>
		public static double AspectRatio(in Vector3d v1, in Vector3d v2, in Vector3d v3) {
			double a = v1.Distance(v2), b = v2.Distance(v3), c = v3.Distance(v1);
			var s = (a + b + c) / 2.0;
			return a * b * c / (8.0 * (s - a) * (s - b) * (s - c));
		}


		//! fast cotangent between two normalized vectors 
		//! cot = cos/sin, both of which can be computed from vector identities
		//! returns zero if result would be unstable (eg infinity)
		// formula from http://www.geometry.caltech.edu/pubs/DMSB_III.pdf
		public static double VectorCot(in Vector3d v1, in Vector3d v2) {
			var fDot = v1.Dot(v2);
			var lensqr1 = v1.LengthSquared;
			var lensqr2 = v2.LengthSquared;
			var d = Clamp((lensqr1 * lensqr2) - (fDot * fDot), 0.0f, double.MaxValue);
			return d < MathUtil.ZERO_TOLERANCE ? 0 : fDot / Math.Sqrt(d);
		}

		public static double VectorTan(in Vector3d v1, in Vector3d v2) {
			var fDot = v1.Dot(v2);
			var lensqr1 = v1.LengthSquared;
			var lensqr2 = v2.LengthSquared;
			var d = MathUtil.Clamp((lensqr1 * lensqr2) - (fDot * fDot), 0.0f, double.MaxValue);
			return d == 0 ? 0 : Math.Sqrt(d) / fDot;
		}


		public static bool IsObtuse(in Vector3d v1, in Vector3d v2, in Vector3d v3) {
			var a2 = v1.DistanceSquared(v2);
			var b2 = v1.DistanceSquared(v3);
			var c2 = v2.DistanceSquared(v3);
			return (a2 + b2 < c2) || (b2 + c2 < a2) || (c2 + a2 < b2);
		}


		// code adapted from http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
		//    Return: >0 for P2 left of the line through P0 and P1
		//            =0 for P2 on the line
		//            <0 for P2 right of the line
		public static double IsLeft(in Vector2d P0, in Vector2d P1, in Vector2d P2) {
			return Math.Sign(((P1.x - P0.x) * (P2.y - P0.y)) - ((P2.x - P0.x) * (P1.y - P0.y)));
		}



		/// <summary>
		/// Compute barycentric coordinates/weights of vPoint inside triangle (V0,V1,V2). 
		/// If point is in triangle plane and inside triangle, coords will be positive and sum to 1.
		/// ie if result is a, then vPoint = a.x*V0 + a.y*V1 + a.z*V2.
		/// </summary>
		public static Vector3d BarycentricCoords(in Vector3d vPoint, in Vector3d V0, in Vector3d V1, in Vector3d V2) {
			var kV02 = V0 - V2;
			var kV12 = V1 - V2;
			var kPV2 = vPoint - V2;
			var fM00 = kV02.Dot(kV02);
			var fM01 = kV02.Dot(kV12);
			var fM11 = kV12.Dot(kV12);
			var fR0 = kV02.Dot(kPV2);
			var fR1 = kV12.Dot(kPV2);
			var fDet = (fM00 * fM11) - (fM01 * fM01);
			var fInvDet = 1.0 / fDet;
			var fBary1 = ((fM11 * fR0) - (fM01 * fR1)) * fInvDet;
			var fBary2 = ((fM00 * fR1) - (fM01 * fR0)) * fInvDet;
			var fBary3 = 1.0 - fBary1 - fBary2;
			return new Vector3d(fBary1, fBary2, fBary3);
		}

		/// <summary>
		/// Compute barycentric coordinates/weights of vPoint inside triangle (V0,V1,V2). 
		/// If point is inside triangle, coords will pe positive and sum to 1.
		/// ie if result is a, then vPoint = a.x*V0 + a.y*V1 + a.z*V2.
		/// </summary>
		public static Vector3d BarycentricCoords(in Vector2d vPoint, in Vector2d V0, in Vector2d V1, in Vector2d V2) {
			var kV02 = V0 - V2;
			var kV12 = V1 - V2;
			var kPV2 = vPoint - V2;
			var fM00 = kV02.Dot(kV02);
			var fM01 = kV02.Dot(kV12);
			var fM11 = kV12.Dot(kV12);
			var fR0 = kV02.Dot(kPV2);
			var fR1 = kV12.Dot(kPV2);
			var fDet = (fM00 * fM11) - (fM01 * fM01);
			var fInvDet = 1.0 / fDet;
			var fBary1 = ((fM11 * fR0) - (fM01 * fR1)) * fInvDet;
			var fBary2 = ((fM00 * fR1) - (fM01 * fR0)) * fInvDet;
			var fBary3 = 1.0 - fBary1 - fBary2;
			return new Vector3d(fBary1, fBary2, fBary3);
		}


		/// <summary>
		/// signed winding angle of oriented triangle [a,b,c] wrt point p
		/// formula from Jacobson et al 13 http://igl.ethz.ch/projects/winding-number/
		/// </summary>
		public static double TriSolidAngle(Vector3d a, Vector3d b, Vector3d c, ref Vector3d p) {
			a -= p;
			b -= p;
			c -= p;
			double la = a.Length, lb = b.Length, lc = c.Length;
			var bottom = (la * lb * lc) + (a.Dot(b) * lc) + (b.Dot(c) * la) + (c.Dot(a) * lb);
			var top = (a.x * ((b.y * c.z) - (c.y * b.z))) - (a.y * ((b.x * c.z) - (c.x * b.z))) + (a.z * ((b.x * c.y) - (c.x * b.y)));
			return 2.0 * Math.Atan2(top, bottom);
		}



		public static bool SolveQuadratic(double a, in double b, in double c, out double minT, out double maxT) {
			minT = maxT = 0;
			if (a == 0 && b == 0)   // function is constant...
{
				return true;
			}

			var discrim = (b * b) - (4.0 * a * c);
			if (discrim < 0) {
				return false;    // no solution
			}

			// a bit odd but numerically better (says NRIC)
			var t = -0.5 * (b + (Math.Sign(b) * Math.Sqrt(discrim)));
			minT = t / a;
			maxT = c / t;
			if (minT > maxT) {
				a = minT;
				minT = maxT;
				maxT = a;   // swap
			}

			return true;
		}




		static readonly int[] _powers_of_10 = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };
		public static int PowerOf10(in int n) {
			return _powers_of_10[n];
		}


		/// <summary>
		/// Iterate from 0 to (nMax-1) using prime-modulo, so we see every index once, but not in-order
		/// </summary>
		public static IEnumerable<int> ModuloIteration(int nMaxExclusive, int nPrime = 31337) {
			var i = 0;
			var done = false;
			while (done == false) {
				yield return i;
				i = (i + nPrime) % nMaxExclusive;
				done = i == 0;
			}
		}
		public static double FastSin(double x) {
			double sinn;
			if (x < -3.14159265) {
				x += 6.28318531;
			}
			else if (x > 3.14159265) {
				x -= 6.28318531;
			}

			if (x < 0) {
				sinn = (1.27323954 * x) + (0.405284735 * x * x);
				sinn = sinn < 0 ? (0.225 * ((sinn * -sinn) - sinn)) + sinn : (0.225 * ((sinn * sinn) - sinn)) + sinn;
				return sinn;
			}
			else {
				sinn = (1.27323954 * x) - (0.405284735 * x * x);
				sinn = sinn < 0 ? (0.225 * ((sinn * -sinn) - sinn)) + sinn : (0.225 * ((sinn * sinn) - sinn)) + sinn;
				return sinn;

			}
		}

		public static double FastCos(in double v) {
			return FastSin(v + 1.5707963);
		}
		public static float Sign(in float value) {
			return MathF.Sign(value);
		}
		public static Vector2f Sign(in Vector2f value) {
			return new Vector2f(Sign(value.x), Sign(value.y));
		}
		public static Vector3f Sign(in Vector3f value) {
			return new Vector3f(Sign(value.x), Sign(value.y), Sign(value.z));
		}
	}
}
