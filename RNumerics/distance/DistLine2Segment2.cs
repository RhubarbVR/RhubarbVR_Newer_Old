using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public class DistLine2Segment2
	{
		Line2d _line;
		public Line2d Line
		{
			get => _line;
			set { _line = value; DistanceSquared = -1.0; }
		}

		Segment2d _segment;
		public Segment2d Segment
		{
			get => _segment;
			set { _segment = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector2d LineClosest;
		public double LineParameter;
		public Vector2d SegmentClosest;
		public double SegmentParameter;


		public DistLine2Segment2(Line2d LineIn, Segment2d SegmentIn) {
			_segment = SegmentIn;
			_line = LineIn;
		}

		static public double MinDistance(Line2d line, Segment2d segment) {
			return new DistLine2Segment2(line, segment).Get();
		}


		public DistLine2Segment2 Compute() {
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

			var diff = _line.Origin - _segment.Center;
			var a01 = -_line.Direction.Dot(_segment.Direction);
			var b0 = diff.Dot(_line.Direction);
			var c = diff.LengthSquared;
			var det = Math.Abs(1 - (a01 * a01));
			double b1, s0, s1, sqrDist, extDet;

			if (det >= MathUtil.ZERO_TOLERANCE) {
				// The line and segment are not parallel.
				b1 = -diff.Dot(_segment.Direction);
				s1 = (a01 * b0) - b1;
				extDet = _segment.Extent * det;

				if (s1 >= -extDet) {
					if (s1 <= extDet) {
						// Two interior points are closest, one on the line and one
						// on the segment.
						var invDet = ((double)1) / det;
						s0 = ((a01 * b1) - b0) * invDet;
						s1 *= invDet;
						sqrDist = (double)0;
					}
					else {
						// The endpoint e1 of the segment and an interior point of
						// the line are closest.
						s1 = _segment.Extent;
						s0 = -((a01 * s1) + b0);
						sqrDist = (-s0 * s0) + (s1 * (s1 + (((double)2) * b1))) + c;
					}
				}
				else {
					// The endpoint e0 of the segment and an interior point of the
					// line are closest.
					s1 = -_segment.Extent;
					s0 = -((a01 * s1) + b0);
					sqrDist = (-s0 * s0) + (s1 * (s1 + (2 * b1))) + c;
				}
			}
			else {
				// The line and segment are parallel.  Choose the closest pair so that
				// one point is at segment origin.
				s1 = (double)0;
				s0 = -b0;
				sqrDist = (b0 * s0) + c;
			}

			LineParameter = s0;
			LineClosest = _line.Origin + (s0 * _line.Direction);
			SegmentParameter = s1;
			SegmentClosest = _segment.Center + (s1 * _segment.Direction);

			// Account for numerical round-off errors
			if (sqrDist < 0) {
				sqrDist = (double)0;
			}

			DistanceSquared = sqrDist;
			return sqrDist;
		}
	}

}
