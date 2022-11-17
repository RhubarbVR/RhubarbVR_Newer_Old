using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public sealed class DistLine3Segment3
	{
		Line3d _line;
		public Line3d Line
		{
			get => _line;
			set { _line = value; DistanceSquared = -1.0; }
		}

		Segment3d _segment;
		public Segment3d Segment
		{
			get => _segment;
			set { _segment = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d LineClosest;
		public double LineParameter;
		public Vector3d SegmentClosest;
		public double SegmentParameter;


		public DistLine3Segment3(in Line3d LineIn, in Segment3d SegmentIn) {
			_segment = SegmentIn;
			_line = LineIn;
		}

		static public double MinDistance(in Line3d line, in Segment3d segment) {
			return new DistLine3Segment3(line, segment).Get();
		}
		static public double MinDistanceLineParam(in Line3d line, in Segment3d segment) {
			return new DistLine3Segment3(line, segment).Compute().LineParameter;
		}


		public DistLine3Segment3 Compute() {
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

			var diff = _line.origin - _segment.center;
			var a01 = -_line.direction.Dot(_segment.direction);
			var b0 = diff.Dot(_line.direction);
			var c = diff.LengthSquared;
			var det = Math.Abs(1 - (a01 * a01));
			double b1, s0, s1, sqrDist, extDet;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				// The line and segment are not parallel.
				b1 = -diff.Dot(_segment.direction);
				s1 = (a01 * b0) - b1;
				extDet = _segment.extent * det;

				if (s1 >= -extDet) {
					if (s1 <= extDet) {
						// Two interior points are closest, one on the line and one
						// on the segment.
						var invDet = 1 / det;
						s0 = ((a01 * b1) - b0) * invDet;
						s1 *= invDet;
						sqrDist = (s0 * (s0 + (a01 * s1) + (2 * b0))) +
							(s1 * ((a01 * s0) + s1 + (2 * b1))) + c;
					}
					else {
						// The endpoint e1 of the segment and an interior point of
						// the line are closest.
						s1 = _segment.extent;
						s0 = -((a01 * s1) + b0);
						sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
					}
				}
				else {
					// The end point e0 of the segment and an interior point of the
					// line are closest.
					s1 = -_segment.extent;
					s0 = -((a01 * s1) + b0);
					sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
				}
			}
			else {
				// The line and segment are parallel.  Choose the closest pair so that
				// one point is at segment center.
				s1 = 0;
				s0 = -b0;
				sqrDist = (b0 * s0) + c;
			}

			LineClosest = _line.origin + (s0 * _line.direction);
			SegmentClosest = _segment.center + (s1 * _segment.direction);
			LineParameter = s0;
			SegmentParameter = s1;

			// Account for numerical round-off errors.
			if (sqrDist < 0) {
				sqrDist = 0;
			}

			DistanceSquared = sqrDist;
			return sqrDist;
		}
	}

}
