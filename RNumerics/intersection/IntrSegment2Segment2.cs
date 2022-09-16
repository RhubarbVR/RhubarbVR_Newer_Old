using System;
using System.Diagnostics;

namespace RNumerics
{
	// ported from WildMagic5
	//
	//
	// double IntervalThreshold
	// 		The intersection testing uses the center-extent form for line segments.
	// 		If you start with endpoints (Vector2<Real>) and create Segment2<Real>
	// 		objects, the conversion to center-extent form can contain small
	// 		numerical round-off errors.  Testing for the intersection of two
	// 		segments that share an endpoint might lead to a failure due to the
	// 		round-off errors.  To allow for this, you may specify a small positive
	// 		threshold that slightly enlarges the intervals for the segments.  The
	// 		default value is zero.
	//
	// double DotThreshold
	// 		The computation for determining whether the linear components are
	// 		parallel might contain small floating-point round-off errors.  The
	// 		default threshold is MathUtil.ZeroTolerance.  If you set the value,
	// 		pass in a nonnegative number.
	//
	//
	// The intersection set:  Let q = Quantity.  The cases are
	//
	//   q = 0: The segments do not intersect.  Type is Empty
	//
	//   q = 1: The segments intersect in a single point.  Type is Point
	//          Intersection point is Point0.
	//          
	//   q = 2: The segments are collinear and intersect in a segment.
	//			Type is Segment. Points are Point0 and Point1


	public sealed class IntrSegment2Segment2
	{
		Segment2d _segment1;
		public Segment2d Segment1
		{
			get => _segment1;
			set { _segment1 = value; Result = IntersectionResult.NotComputed; }
		}

		Segment2d _segment2;
		public Segment2d Segment2
		{
			get => _segment2;
			set { _segment2 = value; Result = IntersectionResult.NotComputed; }
		}

		double _intervalThresh = 0;
		public double IntervalThreshold
		{
			get => _intervalThresh;
			set { _intervalThresh = Math.Max(value, 0); Result = IntersectionResult.NotComputed; }
		}

		double _dotThresh = MathUtil.ZERO_TOLERANCE;
		public double DotThreshold
		{
			get => _dotThresh;
			set { _dotThresh = Math.Max(value, 0); Result = IntersectionResult.NotComputed; }
		}

		public int Quantity = 0;
		public IntersectionResult Result = IntersectionResult.NotComputed;
		public IntersectionType Type = IntersectionType.Empty;

		public bool IsSimpleIntersection => Result == IntersectionResult.Intersects && Type == IntersectionType.Point;

		// these values are all on segment 1, unlike many other tests!!

		public Vector2d Point0;
		public Vector2d Point1;     // only set if Quantity == 2, ie segment overlap

		public double Parameter0;
		public double Parameter1;     // only set if Quantity == 2, ie segment overlap

		public IntrSegment2Segment2(in Segment2d seg1, in Segment2d seg2) {
			_segment1 = seg1;
			_segment2 = seg2;
		}

		public IntrSegment2Segment2 Compute() {
			Find();
			return this;
		}


		public bool Find() {
			if (Result != IntersectionResult.NotComputed) {
				return Result == IntersectionResult.Intersects;
			}

			// [RMS] if either segment direction is not a normalized vector, 
			//   results are garbage, so fail query
			if (_segment1.Direction.IsNormalized == false || _segment2.Direction.IsNormalized == false) {
				Type = IntersectionType.Empty;
				Result = IntersectionResult.InvalidQuery;
				return false;
			}


			var s = Vector2d.Zero;
			Type = IntrLine2Line2.Classify(_segment1.Center, _segment1.Direction,
										   _segment2.Center, _segment2.Direction,
										   _dotThresh, ref s);

			if (Type == IntersectionType.Point) {
				// Test whether the line-line intersection is on the segments.
				if (Math.Abs(s[0]) <= _segment1.Extent + _intervalThresh
					&& Math.Abs(s[1]) <= _segment2.Extent + _intervalThresh) {
					Quantity = 1;
					Point0 = _segment1.Center + (s[0] * _segment1.Direction);
					Parameter0 = s[0];
				}
				else {
					Quantity = 0;
					Type = IntersectionType.Empty;
				}
			}
			else if (Type == IntersectionType.Line) {
				// Compute the location of segment1 endpoints relative to segment0.
				var diff = _segment2.Center - _segment1.Center;
				var t1 = _segment1.Direction.Dot(diff);
				var tmin = t1 - _segment2.Extent;
				var tmax = t1 + _segment2.Extent;
				var calc = new Intersector1(-_segment1.Extent, _segment1.Extent, tmin, tmax);
				calc.Find();
				Quantity = calc.NumIntersections;
				if (Quantity == 2) {
					Type = IntersectionType.Segment;
					Parameter0 = calc.GetIntersection(0);
					Point0 = _segment1.Center +
						(Parameter0 * _segment1.Direction);
					Parameter1 = calc.GetIntersection(1);
					Point1 = _segment1.Center +
						(Parameter1 * _segment1.Direction);
				}
				else if (Quantity == 1) {
					Type = IntersectionType.Point;
					Parameter0 = calc.GetIntersection(0);
					Point0 = _segment1.Center +
						(Parameter0 * _segment1.Direction);
				}
				else {
					Type = IntersectionType.Empty;
				}
			}
			else {
				Quantity = 0;
			}

			Result = (Type != IntersectionType.Empty) ?
				IntersectionResult.Intersects : IntersectionResult.NoIntersection;

			// [RMS] for debugging...
			//sanity_check();

			return Result == IntersectionResult.Intersects;
		}
	}
}
