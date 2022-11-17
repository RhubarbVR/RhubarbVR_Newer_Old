using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public sealed class DistLine3Ray3
	{
		Line3d _line;
		public Line3d Line
		{
			get => _line;
			set { _line = value; DistanceSquared = -1.0; }
		}

		Ray3d _ray;
		public Ray3d Ray
		{
			get => _ray;
			set { _ray = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d LineClosest;
		public double LineParameter;
		public Vector3d RayClosest;
		public double RayParameter;


		public DistLine3Ray3(in Ray3d rayIn, in Line3d LineIn) {
			_ray = rayIn;
			_line = LineIn;
		}

		static public double MinDistance(in Ray3d r, in Line3d s) {
			return new DistLine3Ray3(r, s).Get();
		}
		static public double MinDistanceLineParam(in Ray3d r, in Line3d s) {
			return new DistLine3Ray3(r, s).Compute().LineParameter;
		}


		public DistLine3Ray3 Compute() {
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

			var kDiff = _line.origin - _ray.origin;
			var a01 = -_line.direction.Dot(_ray.direction);
			var b0 = kDiff.Dot(_line.direction);
			var c = kDiff.LengthSquared;
			var det = Math.Abs(1.0 - (a01 * a01));
			double b1, s0, s1, sqrDist;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				b1 = -kDiff.Dot(_ray.direction);
				s1 = (a01 * b0) - b1;

				if (s1 >= (double)0) {
					// Two interior points are closest, one on line and one on ray.
					var invDet = ((double)1) / det;
					var v = (a01 * b1) - b0;
					s0 = v * invDet;
					s1 *= invDet;
					sqrDist = (s0 * (s0 + (a01 * s1) + (((double)2) * b0))) +
						(s1 * ((a01 * s0) + s1 + (((double)2) * b1))) + c;
				}
				else {
					// Origin of ray and interior point of line are closest.
					s0 = -b0;
					s1 = (double)0;
					sqrDist = (b0 * s0) + c;
				}
			}
			else {
				// Lines are parallel, closest pair with one point at ray origin.
				s0 = -b0;
				s1 = (double)0;
				sqrDist = (b0 * s0) + c;
			}

			LineClosest = _line.origin + (s0 * _line.direction);
			RayClosest = _ray.origin + (s1 * _ray.direction);
			LineParameter = s0;
			RayParameter = s1;

			// Account for numerical round-off errors.
			if (sqrDist < (double)0) {
				sqrDist = (double)0;
			}
			DistanceSquared = sqrDist;

			return sqrDist;
		}



	}
}
