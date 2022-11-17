using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public sealed class DistPoint3Triangle3
	{
		Vector3d _point;
		public Vector3d Point
		{
			get => _point;
			set { _point = value; DistanceSquared = -1.0; }
		}

		Triangle3d _triangle;
		public Triangle3d Triangle
		{
			get => _triangle;
			set { _triangle = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d TriangleClosest;
		public Vector3d TriangleBaryCoords;


		public DistPoint3Triangle3(in Vector3d PointIn, in Triangle3d TriangleIn) {
			_point = PointIn;
			_triangle = TriangleIn;
		}

		public DistPoint3Triangle3 Compute() {
			GetSquared();
			return this;
		}

		public double Get() {
			return Math.Sqrt(GetSquared());
		}


		public double GetSquared() {
			if (DistanceSquared >= 0) {
				return DistanceSquared;
			}

			DistanceSquared = DistanceSqr(ref _point, ref _triangle, out TriangleClosest, out TriangleBaryCoords);
			return DistanceSquared;
		}


		public static double DistanceSqr(ref Vector3d point, ref Triangle3d triangle, out Vector3d closestPoint, out Vector3d baryCoords) {
			var diff = triangle.v0 - point;
			var edge0 = triangle.v1 - triangle.v0;
			var edge1 = triangle.v2 - triangle.v0;
			var a00 = edge0.LengthSquared;
			var a01 = edge0.Dot( edge1);
			var a11 = edge1.LengthSquared;
			var b0 = diff.Dot( edge0);
			var b1 = diff.Dot( edge1);
			var c = diff.LengthSquared;
			var det = Math.Abs((a00 * a11) - (a01 * a01));
			var s = (a01 * b1) - (a11 * b0);
			var t = (a01 * b0) - (a00 * b1);
			double sqrDistance;

			if (s + t <= det) {
				if (s < 0) {
					if (t < 0)  // region 4
					{
						if (b0 < 0) {
							t = 0;
							if (-b0 >= a00) {
								s = 1;
								sqrDistance = a00 + (2 * b0) + c;
							}
							else {
								s = -b0 / a00;
								sqrDistance = (b0 * s) + c;
							}
						}
						else {
							s = 0;
							if (b1 >= 0) {
								t = 0;
								sqrDistance = c;
							}
							else if (-b1 >= a11) {
								t = 1;
								sqrDistance = a11 + (2 * b1) + c;
							}
							else {
								t = -b1 / a11;
								sqrDistance = (b1 * t) + c;
							}
						}
					}
					else  // region 3
					{
						s = 0;
						if (b1 >= 0) {
							t = 0;
							sqrDistance = c;
						}
						else if (-b1 >= a11) {
							t = 1;
							sqrDistance = a11 + (2 * b1) + c;
						}
						else {
							t = -b1 / a11;
							sqrDistance = (b1 * t) + c;
						}
					}
				}
				else if (t < 0)  // region 5
				{
					t = 0;
					if (b0 >= 0) {
						s = 0;
						sqrDistance = c;
					}
					else if (-b0 >= a00) {
						s = 1;
						sqrDistance = a00 + (2 * b0) + c;
					}
					else {
						s = -b0 / a00;
						sqrDistance = (b0 * s) + c;
					}
				}
				else  // region 0
				{
					// minimum at interior point
					var invDet = 1 / det;
					s *= invDet;
					t *= invDet;
					sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
						(t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
				}
			}
			else {
				double tmp0, tmp1, numer, denom;

				if (s < 0)  // region 2
				{
					tmp0 = a01 + b0;
					tmp1 = a11 + b1;
					if (tmp1 > tmp0) {
						numer = tmp1 - tmp0;
						denom = a00 - (2 * a01) + a11;
						if (numer >= denom) {
							s = 1;
							t = 0;
							sqrDistance = a00 + (2 * b0) + c;
						}
						else {
							s = numer / denom;
							t = 1 - s;
							sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
								(t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
						}
					}
					else {
						s = 0;
						if (tmp1 <= 0) {
							t = 1;
							sqrDistance = a11 + (2 * b1) + c;
						}
						else if (b1 >= 0) {
							t = 0;
							sqrDistance = c;
						}
						else {
							t = -b1 / a11;
							sqrDistance = (b1 * t) + c;
						}
					}
				}
				else if (t < 0)  // region 6
				{
					tmp0 = a01 + b1;
					tmp1 = a00 + b0;
					if (tmp1 > tmp0) {
						numer = tmp1 - tmp0;
						denom = a00 - (2 * a01) + a11;
						if (numer >= denom) {
							t = 1;
							s = 0;
							sqrDistance = a11 + (2 * b1) + c;
						}
						else {
							t = numer / denom;
							s = 1 - t;
							sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
								(t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
						}
					}
					else {
						t = 0;
						if (tmp1 <= 0) {
							s = 1;
							sqrDistance = a00 + (2 * b0) + c;
						}
						else if (b0 >= 0) {
							s = 0;
							sqrDistance = c;
						}
						else {
							s = -b0 / a00;
							sqrDistance = (b0 * s) + c;
						}
					}
				}
				else  // region 1
				{
					numer = a11 + b1 - a01 - b0;
					if (numer <= 0) {
						s = 0;
						t = 1;
						sqrDistance = a11 + (2 * b1) + c;
					}
					else {
						denom = a00 - (2 * a01) + a11;
						if (numer >= denom) {
							s = 1;
							t = 0;
							sqrDistance = a00 + (2 * b0) + c;
						}
						else {
							s = numer / denom;
							t = 1 - s;
							sqrDistance = (s * ((a00 * s) + (a01 * t) + (2 * b0))) +
								(t * ((a01 * s) + (a11 * t) + (2 * b1))) + c;
						}
					}
				}
			}
			closestPoint = triangle.v0 + (s * edge0) + (t * edge1);
			baryCoords = new Vector3d(1 - s - t, s, t);

			// Account for numerical round-off error.
			return Math.Max(sqrDistance, 0);
		}






	}
}
