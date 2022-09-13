using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// Distance between ray and segment
	/// ported from WildMagic5
	/// </summary>
	public sealed class DistRay3Segment3
	{
		Ray3d _ray;
		public Ray3d Ray
		{
			get => _ray;
			set { _ray = value; DistanceSquared = -1.0; }
		}

		Segment3d _segment;
		public Segment3d Segment
		{
			get => _segment;
			set { _segment = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d RayClosest;
		public double RayParameter;
		public Vector3d SegmentClosest;
		public double SegmentParameter;


		public DistRay3Segment3(in Ray3d rayIn, in Segment3d segmentIn) {
			_ray = rayIn;
			_segment = segmentIn;
		}


		static public double MinDistance(in Ray3d r, in Segment3d s) {
			var dsqr = SquaredDistance(r, s, out var rayt, out var segt);
			return Math.Sqrt(dsqr);
		}
		static public double MinDistanceSegmentParam(in Ray3d r, in Segment3d s) {
			/*double dsqr = */
			SquaredDistance( r, s, out var rayt, out var segt);
			return segt;
		}


		public DistRay3Segment3 Compute() {
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

			var diff = _ray.Origin - _segment.Center;
			var a01 = -_ray.Direction.Dot(_segment.Direction);
			var b0 = diff.Dot(_ray.Direction);
			var b1 = -diff.Dot(_segment.Direction);
			var c = diff.LengthSquared;
			var det = Math.Abs(1 - (a01 * a01));
			double s0, s1, sqrDist, extDet;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				// The Ray and Segment are not parallel.
				s0 = (a01 * b1) - b0;
				s1 = (a01 * b0) - b1;
				extDet = _segment.Extent * det;

				if (s0 >= 0) {
					if (s1 >= -extDet) {
						if (s1 <= extDet)  // region 0
						{
							// Minimum at interior points of Ray and Segment.
							var invDet = 1 / det;
							s0 *= invDet;
							s1 *= invDet;
							sqrDist = (s0 * (s0 + (a01 * s1) + (2 * b0))) +
								(s1 * ((a01 * s0) + s1 + (2 * b1))) + c;
						}
						else  // region 1
						{
							s1 = _segment.Extent;
							s0 = -((a01 * s1) + b0);
							if (s0 > 0) {
								sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
							}
							else {
								s0 = 0;
								sqrDist = (s1 * (s1 + (2 * b1))) + c;
							}
						}
					}
					else  // region 5
					{
						s1 = -_segment.Extent;
						s0 = -((a01 * s1) + b0);
						if (s0 > 0) {
							sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
						}
						else {
							s0 = 0;
							sqrDist = (s1 * (s1 + (2 * b1))) + c;
						}
					}
				}
				else {
					if (s1 <= -extDet)  // region 4
					{
						s0 = -((-a01 * _segment.Extent) + b0);
						if (s0 > 0) {
							s1 = -_segment.Extent;
							sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
						}
						else {
							s0 = 0;
							s1 = -b1;
							if (s1 < -_segment.Extent) {
								s1 = -_segment.Extent;
							}
							else if (s1 > _segment.Extent) {
								s1 = _segment.Extent;
							}
							sqrDist = (s1 * (s1 + (2 * b1))) + c;
						}
					}
					else if (s1 <= extDet)  // region 3
					{
						s0 = 0;
						s1 = -b1;
						if (s1 < -_segment.Extent) {
							s1 = -_segment.Extent;
						}
						else if (s1 > _segment.Extent) {
							s1 = _segment.Extent;
						}
						sqrDist = (s1 * (s1 + (2 * b1))) + c;
					}
					else  // region 2
					{
						s0 = -((a01 * _segment.Extent) + b0);
						if (s0 > 0) {
							s1 = _segment.Extent;
							sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
						}
						else {
							s0 = 0;
							s1 = -b1;
							if (s1 < -_segment.Extent) {
								s1 = -_segment.Extent;
							}
							else if (s1 > _segment.Extent) {
								s1 = _segment.Extent;
							}
							sqrDist = (s1 * (s1 + (2 * b1))) + c;
						}
					}
				}
			}
			else {
				// Ray and Segment are parallel.
				if (a01 > 0) {
					// Opposite direction vectors.
					s1 = -_segment.Extent;
				}
				else {
					// Same direction vectors.
					s1 = _segment.Extent;
				}

				s0 = -((a01 * s1) + b0);
				if (s0 > 0) {
					sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
				}
				else {
					s0 = 0;
					sqrDist = (s1 * (s1 + (2 * b1))) + c;
				}
			}

			RayClosest = _ray.Origin + (s0 * _ray.Direction);
			SegmentClosest = _segment.Center + (s1 * _segment.Direction);
			RayParameter = s0;
			SegmentParameter = s1;

			// Account for numerical round-off errors.
			if (sqrDist < 0) {
				sqrDist = 0;
			}
			DistanceSquared = sqrDist;
			return DistanceSquared;
		}






		/// <summary>
		/// compute w/o allocating temporaries/etc
		/// </summary>
		public static double SquaredDistance(in Ray3d ray, in Segment3d segment,
			out double rayT, out double segT) {
			var diff = ray.Origin - segment.Center;
			var a01 = -ray.Direction.Dot(segment.Direction);
			var b0 = diff.Dot(ray.Direction);
			var b1 = -diff.Dot(segment.Direction);
			var c = diff.LengthSquared;
			var det = Math.Abs(1 - (a01 * a01));
			double s0, s1, sqrDist, extDet;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				// The Ray and Segment are not parallel.
				s0 = (a01 * b1) - b0;
				s1 = (a01 * b0) - b1;
				extDet = segment.Extent * det;

				if (s0 >= 0) {
					if (s1 >= -extDet) {
						if (s1 <= extDet)  // region 0
						{
							// Minimum at interior points of Ray and Segment.
							var invDet = 1 / det;
							s0 *= invDet;
							s1 *= invDet;
							sqrDist = (s0 * (s0 + (a01 * s1) + (2 * b0))) +
								(s1 * ((a01 * s0) + s1 + (2 * b1))) + c;
						}
						else  // region 1
						{
							s1 = segment.Extent;
							s0 = -((a01 * s1) + b0);
							if (s0 > 0) {
								sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
							}
							else {
								s0 = 0;
								sqrDist = (s1 * (s1 + (2 * b1))) + c;
							}
						}
					}
					else  // region 5
					{
						s1 = -segment.Extent;
						s0 = -((a01 * s1) + b0);
						if (s0 > 0) {
							sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
						}
						else {
							s0 = 0;
							sqrDist = (s1 * (s1 + (2 * b1))) + c;
						}
					}
				}
				else {
					if (s1 <= -extDet)  // region 4
					{
						s0 = -((-a01 * segment.Extent) + b0);
						if (s0 > 0) {
							s1 = -segment.Extent;
							sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
						}
						else {
							s0 = 0;
							s1 = -b1;
							if (s1 < -segment.Extent) {
								s1 = -segment.Extent;
							}
							else if (s1 > segment.Extent) {
								s1 = segment.Extent;
							}
							sqrDist = (s1 * (s1 + (2 * b1))) + c;
						}
					}
					else if (s1 <= extDet)  // region 3
					{
						s0 = 0;
						s1 = -b1;
						if (s1 < -segment.Extent) {
							s1 = -segment.Extent;
						}
						else if (s1 > segment.Extent) {
							s1 = segment.Extent;
						}
						sqrDist = (s1 * (s1 + (2 * b1))) + c;
					}
					else  // region 2
					{
						s0 = -((a01 * segment.Extent) + b0);
						if (s0 > 0) {
							s1 = segment.Extent;
							sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
						}
						else {
							s0 = 0;
							s1 = -b1;
							if (s1 < -segment.Extent) {
								s1 = -segment.Extent;
							}
							else if (s1 > segment.Extent) {
								s1 = segment.Extent;
							}
							sqrDist = (s1 * (s1 + (2 * b1))) + c;
						}
					}
				}
			}
			else {
				// Ray and Segment are parallel.
				if (a01 > 0) {
					// Opposite direction vectors.
					s1 = -segment.Extent;
				}
				else {
					// Same direction vectors.
					s1 = segment.Extent;
				}

				s0 = -((a01 * s1) + b0);
				if (s0 > 0) {
					sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
				}
				else {
					s0 = 0;
					sqrDist = (s1 * (s1 + (2 * b1))) + c;
				}
			}

			rayT = s0;
			segT = s1;

			// Account for numerical round-off errors.
			if (sqrDist < 0) {
				sqrDist = 0;
			}

			return sqrDist;
		}



	}
}
