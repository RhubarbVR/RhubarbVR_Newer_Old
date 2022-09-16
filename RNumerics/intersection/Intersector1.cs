using System;

namespace RNumerics
{
	// ported from WildMagic5
	//
	// A class for intersection of intervals [u0,u1] and [v0,v1].  The end
	// points must be ordered:  u0 <= u1 and v0 <= v1.  Values of MAX_REAL
	// and -MAX_REAL are allowed, and degenerate intervals are allowed:
	// u0 = u1 or v0 = v1.
	//
	// [TODO] could this be struct? is not used in contexts where we necessarily need a new object...
	//
	public sealed class Intersector1
	{
		// intervals to intersect
		public Interval1d U;
		public Interval1d V;


		// Information about the intersection set.  The number of intersections
		// is 0 (intervals do not overlap), 1 (intervals are just touching), or
		// 2 (intervals intersect in an inteval).
		public int NumIntersections = 0;

		// intersection point/interval, access via GetIntersection
		private Interval1d _intersections = Interval1d.Zero;

		public Intersector1(in double u0, in double u1, in double v0, in double v1) {
			// [TODO] validate 0 < 1
			U = new Interval1d(u0, u1);
			V = new Interval1d(v0, v1);
		}
		public Intersector1(in Interval1d u, in Interval1d v) {
			U = u;
			V = v;
		}

		public bool Test => U.a <= V.b && U.b >= V.a;


		public double GetIntersection(in int i) {
			return _intersections[i];
		}

		public bool Find() {
			if (U.b < V.a || U.a > V.b) {
				NumIntersections = 0;
			}
			else if (U.b > V.a) {
				if (U.a < V.b) {
					NumIntersections = 2;
					_intersections.a = U.a < V.a ? V.a : U.a;
					_intersections.b = U.b > V.b ? V.b : U.b;
					if (_intersections.a == _intersections.b) {
						NumIntersections = 1;
					}
				}
				else {
					// U.a == V.b
					NumIntersections = 1;
					_intersections.a = U.a;
				}
			}
			else {
				// U.b == V.a
				NumIntersections = 1;
				_intersections.a = U.b;
			}

			return NumIntersections > 0;
		}
	}
}
