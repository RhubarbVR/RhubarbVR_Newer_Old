using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public sealed class DistRay3Ray3
	{
		Ray3d _ray1;
		public Ray3d Ray1
		{
			get => _ray1;
			set { _ray1 = value; DistanceSquared = -1.0; }
		}

		Ray3d _ray2;
		public Ray3d Ray2
		{
			get => _ray2;
			set { _ray2 = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d Ray1Closest;
		public double Ray1Parameter;
		public Vector3d Ray2Closest;
		public double Ray2Parameter;


		public DistRay3Ray3(in Ray3d ray1, in Ray3d ray2) {
			_ray1 = ray1;
			_ray2 = ray2;
		}

		static public double MinDistance(in Ray3d r1, in Ray3d r2) {
			return new DistRay3Ray3(r1, r2).Get();
		}
		static public double MinDistanceRay2Param(in Ray3d r1, in Ray3d r2) {
			return new DistRay3Ray3(r1, r2).Compute().Ray2Parameter;
		}


		public DistRay3Ray3 Compute() {
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

			var diff = _ray1.Origin - _ray2.Origin;
			var a01 = -_ray1.Direction.Dot(_ray2.Direction);
			var b0 = diff.Dot(_ray1.Direction);
			var c = diff.LengthSquared;
			var det = Math.Abs(1.0 - (a01 * a01));
			double b1, s0, s1, sqrDist;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				// Rays are not parallel.
				b1 = -diff.Dot(_ray2.Direction);
				s0 = (a01 * b1) - b0;
				s1 = (a01 * b0) - b1;

				if (s0 >= 0) {
					if (s1 >= 0) { // region 0 (interior)
								   // Minimum at two interior points of rays.
						var invDet = 1.0 / det;
						s0 *= invDet;
						s1 *= invDet;
						sqrDist = (s0 * (s0 + (a01 * s1) + (2.0 * b0))) +
							(s1 * ((a01 * s0) + s1 + (2.0 * b1))) + c;
					}
					else { // region 3 (side)
						s1 = 0;
						if (b0 >= 0) {
							s0 = 0;
							sqrDist = c;
						}
						else {
							s0 = -b0;
							sqrDist = (b0 * s0) + c;
						}
					}
				}
				else {
					if (s1 >= 0) {  // region 1 (side)
						s0 = 0;
						if (b1 >= 0) {
							s1 = 0;
							sqrDist = c;
						}
						else {
							s1 = -b1;
							sqrDist = (b1 * s1) + c;
						}
					}
					else {  // region 2 (corner)
						if (b0 < 0) {
							s0 = -b0;
							s1 = 0;
							sqrDist = (b0 * s0) + c;
						}
						else {
							s0 = 0;
							if (b1 >= 0) {
								s1 = 0;
								sqrDist = c;
							}
							else {
								s1 = -b1;
								sqrDist = (b1 * s1) + c;
							}
						}
					}
				}
			}
			else {
				// Rays are parallel.
				if (a01 > 0) {
					// Opposite direction vectors.
					s1 = 0;
					if (b0 >= 0) {
						s0 = 0;
						sqrDist = c;
					}
					else {
						s0 = -b0;
						sqrDist = (b0 * s0) + c;
					}
				}
				else {
					// Same direction vectors.
					if (b0 >= 0) {
						b1 = -diff.Dot(_ray2.Direction);
						s0 = 0;
						s1 = -b1;
						sqrDist = (b1 * s1) + c;
					}
					else {
						s0 = -b0;
						s1 = 0;
						sqrDist = (b0 * s0) + c;
					}
				}
			}

			Ray1Closest = _ray1.Origin + (s0 * _ray1.Direction);
			Ray2Closest = _ray2.Origin + (s1 * _ray2.Direction);
			Ray1Parameter = s0;
			Ray2Parameter = s1;

			// Account for numerical round-off errors.
			if (sqrDist < 0) {
				sqrDist = 0;
			}
			DistanceSquared = sqrDist;

			return sqrDist;
		}


	}
}
