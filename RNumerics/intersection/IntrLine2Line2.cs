using System;

namespace RNumerics
{
	// ported from WildMagic5 
	public sealed class IntrLine2Line2
	{
		Line2d _line1;
		public Line2d Line1
		{
			get => _line1;
			set { _line1 = value; Result = IntersectionResult.NotComputed; }
		}

		Line2d _line2;
		public Line2d Line2
		{
			get => _line2;
			set { _line2 = value; Result = IntersectionResult.NotComputed; }
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


		public Vector2d Point;
		public double Segment1Parameter;
		public double Segment2Parameter;


		public IntrLine2Line2(in Line2d l1, in Line2d l2) {
			_line1 = l1;
			_line2 = l2;
		}


		public IntrLine2Line2 Compute() {
			Find();
			return this;
		}


		public bool Find() {
			if (Result != IntersectionResult.NotComputed) {
				return Result == IntersectionResult.Intersects;
			}

			// [RMS] if either line direction is not a normalized vector, 
			//   results are garbage, so fail query
			if (_line1.direction.IsNormalized == false || _line2.direction.IsNormalized == false) {
				Type = IntersectionType.Empty;
				Result = IntersectionResult.InvalidQuery;
				return false;
			}

			var s = Vector2d.Zero;
			Type = Classify(_line1.origin, _line1.direction,
							_line2.origin, _line2.direction, _dotThresh, ref s);

			if (Type == IntersectionType.Point) {
				Quantity = 1;
				Point = _line1.origin + (s.x * _line1.direction);
				Segment1Parameter = s.x;
				Segment2Parameter = s.y;
			}
			else {
				Quantity = Type == IntersectionType.Line ? int.MaxValue : 0;
			}

			Result = (Type != IntersectionType.Empty) ?
				IntersectionResult.Intersects : IntersectionResult.NoIntersection;
			return Result == IntersectionResult.Intersects;
		}



		public static IntersectionType Classify(in Vector2d P0, in Vector2d D0, in Vector2d P1, in Vector2d D1,
					 double dotThreshold, ref Vector2d s) {
			// Ensure dotThreshold is nonnegative.
			dotThreshold = Math.Max(dotThreshold, (double)0);

			// The intersection of two lines is a solution to P0+s0*D0 = P1+s1*D1.
			// Rewrite this as s0*D0 - s1*D1 = P1 - P0 = Q.  If D0.Dot(Perp(D1)) = 0,
			// the lines are parallel.  Additionally, if Q.Dot(Perp(D1)) = 0, the
			// lines are the same.  If D0.Dot(Perp(D1)) is not zero, then
			//   s0 = Q.Dot(Perp(D1))/D0.Dot(Perp(D1))
			// produces the point of intersection.  Also,
			//   s1 = Q.Dot(Perp(D0))/D0.Dot(Perp(D1))

			var diff = P1 - P0;
			var D0DotPerpD1 = D0.DotPerp(D1);
			if (Math.Abs(D0DotPerpD1) > dotThreshold) {
				// Lines intersect in a single point.
				var invD0DotPerpD1 = ((double)1) / D0DotPerpD1;
				var diffDotPerpD0 = diff.DotPerp(D0);
				var diffDotPerpD1 = diff.DotPerp(D1);
				s[0] = diffDotPerpD1 * invD0DotPerpD1;
				s[1] = diffDotPerpD0 * invD0DotPerpD1;
				return IntersectionType.Point;
			}

			// Lines are parallel.
			diff.Normalize();
			var diffNDotPerpD1 = diff.DotPerp(D1);
			if (Math.Abs(diffNDotPerpD1) <= dotThreshold) {
				// Lines are colinear.
				return IntersectionType.Line;
			}

			// Lines are parallel, but distinct.
			return IntersectionType.Empty;
		}

	}
}
