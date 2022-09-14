using System;

namespace RNumerics
{
	// ported from WildMagic5 
	public sealed class IntrSegment2Triangle2
	{
		Segment2d _segment;
		public Segment2d Segment
		{
			get => _segment;
			set { _segment = value; Result = IntersectionResult.NotComputed; }
		}

		Triangle2d _triangle;
		public Triangle2d Triangle
		{
			get => _triangle;
			set { _triangle = value; Result = IntersectionResult.NotComputed; }
		}

		public int Quantity = 0;
		public IntersectionResult Result = IntersectionResult.NotComputed;
		public IntersectionType Type = IntersectionType.Empty;

		public bool IsSimpleIntersection => Result == IntersectionResult.Intersects && Type == IntersectionType.Point;


		public Vector2d Point0;
		public Vector2d Point1;
		public double Param0;
		public double Param1;


		public IntrSegment2Triangle2(in Segment2d s, in Triangle2d t) {
			_segment = s;
			_triangle = t;
		}


		public IntrSegment2Triangle2 Compute() {
			Find();
			return this;
		}


		public bool Find() {
			if (Result != IntersectionResult.NotComputed) {
				return Result == IntersectionResult.Intersects;
			}

			// [RMS] if either line direction is not a normalized vector, 
			//   results are garbage, so fail query
			if (_segment.Direction.IsNormalized == false) {
				Type = IntersectionType.Empty;
				Result = IntersectionResult.InvalidQuery;
				return false;
			}

			var dist = Vector3d.Zero;
			var sign = Vector3i.Zero;
			int positive = 0, negative = 0, zero = 0;
			IntrLine2Triangle2.TriangleLineRelations(_segment.Center, _segment.Direction, _triangle,
								  ref dist, ref sign, ref positive, ref negative, ref zero);

			if (positive == 3 || negative == 3) {
				// No intersections.
				Quantity = 0;
				Type = IntersectionType.Empty;
			}
			else {
				var param = Vector2d.Zero;
				IntrLine2Triangle2.GetInterval(_segment.Center, _segment.Direction, _triangle, dist, sign, ref param);

				var intr = new Intersector1(param[0], param[1], -_segment.Extent, +_segment.Extent);
				intr.Find();

				Quantity = intr.NumIntersections;
				if (Quantity == 2) {
					// Segment intersection.
					Type = IntersectionType.Segment;
					Param0 = intr.GetIntersection(0);
					Point0 = _segment.Center + (Param0 * _segment.Direction);
					Param1 = intr.GetIntersection(1);
					Point1 = _segment.Center + (Param1 * _segment.Direction);
				}
				else if (Quantity == 1) {
					// Point intersection.
					Type = IntersectionType.Point;
					Param0 = intr.GetIntersection(0);
					Point0 = _segment.Center + (Param0 * _segment.Direction);
				}
				else {
					// No intersections.
					Type = IntersectionType.Empty;
				}
			}

			Result = (Type != IntersectionType.Empty) ?
				IntersectionResult.Intersects : IntersectionResult.NoIntersection;
			return Result == IntersectionResult.Intersects;
		}



	}
}
