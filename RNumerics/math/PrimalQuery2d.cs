using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{

	// Queries about the relation of a point to various geometric objects.  
	// Ported from https://www.geometrictools.com/GTEngine/Include/Mathematics/GtePrimalQuery2.h
	sealed class PrimalQuery2d
	{
		readonly Func<int, Vector2d> _pointF;


		public PrimalQuery2d(in Func<int, Vector2d> PositionFunc) {
			_pointF = PositionFunc;
		}


		// In the following, point P refers to vertices[i] or 'test' and Vi refers
		// to vertices[vi].

		// For a line with origin V0 and direction <V0,V1>, ToLine returns
		//   +1, P on right of line
		//   -1, P on left of line
		//    0, P on the line
		public int ToLine(in int i, in int v0, in int v1) {
			return ToLine(_pointF(i), v0, v1);
		}
		public int ToLine(in Vector2d test, in int v0, in int v1) {
			var vec0 = _pointF(v0);
			var vec1 = _pointF(v1);

			var x0 = test[0] - vec0[0];
			var y0 = test[1] - vec0[1];
			var x1 = vec1[0] - vec0[0];
			var y1 = vec1[1] - vec0[1];
			var x0y1 = x0 * y1;
			var x1y0 = x1 * y0;
			var det = x0y1 - x1y0;
			const double ZERO = 0.0;

			return det > ZERO ? +1 : (det < ZERO ? -1 : 0);
		}

		// For a line with origin V0 and direction <V0,V1>, ToLine returns
		//   +1, P on right of line
		//   -1, P on left of line
		//    0, P on the line
		// The 'order' parameter is
		//   -3, points not collinear, P on left of line
		//   -2, P strictly left of V0 on the line
		//   -1, P = V0
		//    0, P interior to line segment [V0,V1]
		//   +1, P = V1
		//   +2, P strictly right of V0 on the line
		// This is the same as the first-listed ToLine calls because the worst-case
		// path has the same computational complexity.
		public int ToLine(in int i, in int v0, in int v1, out int order) {
			return ToLine(_pointF(i), v0, v1, out order);

		}
		public int ToLine(in Vector2d test, in int v0, in int v1, out int order) {
			var vec0 = _pointF(v0);
			var vec1 = _pointF(v1);

			var x0 = test[0] - vec0[0];
			var y0 = test[1] - vec0[1];
			var x1 = vec1[0] - vec0[0];
			var y1 = vec1[1] - vec0[1];
			var x0y1 = x0 * y1;
			var x1y0 = x1 * y0;
			var det = x0y1 - x1y0;
			const double ZERO = 0.0;

			if (det > ZERO) {
				order = +3;
				return +1;
			}

			if (det < ZERO) {
				order = -3;
				return -1;
			}

			var x0x1 = x0 * x1;
			var y0y1 = y0 * y1;
			var dot = x0x1 + y0y1;
			if (dot == ZERO) {
				order = -1;
			}
			else if (dot < ZERO) {
				order = -2;
			}
			else {
				var x0x0 = x0 * x0;
				var y0y0 = y0 * y0;
				var sqrLength = x0x0 + y0y0;
				order = dot == sqrLength ? +1 : dot > sqrLength ? +2 : 0;
			}

			return 0;
		}

		// For a triangle with counterclockwise vertices V0, V1, and V2,
		// ToTriangle returns
		//   +1, P outside triangle
		//   -1, P inside triangle
		//    0, P on triangle
		// The query involves three calls to ToLine, so the numbers match those
		// of ToLine.
		public int ToTriangle(in int i, in int v0, in int v1, in int v2) {
			return ToTriangle(_pointF(i), v0, v1, v2);

		}
		public int ToTriangle(in Vector2d test, in int v0, in int v1, in int v2) {
			var sign0 = ToLine(test, v1, v2);
			if (sign0 > 0) {
				return +1;
			}

			var sign1 = ToLine(test, v0, v2);
			if (sign1 < 0) {
				return +1;
			}

			var sign2 = ToLine(test, v0, v1);
			return sign2 > 0 ? +1 : (sign0 != 0 && sign1 != 0 && sign2 != 0) ? -1 : 0;
		}



		// [RMS] added to handle queries where mesh is not consistently oriented
		// For a triangle with vertices V0, V1, and V2, oriented cw or ccw,
		// ToTriangleUnsigned returns
		//   +1, P outside triangle
		//   -1, P inside triangle
		//    0, P on triangle
		// The query involves three calls to ToLine, so the numbers match those
		// of ToLine.
		public int ToTriangleUnsigned(in int i, in int v0, in int v1, in int v2) {
			return ToTriangleUnsigned(_pointF(i), v0, v1, v2);

		}
		public int ToTriangleUnsigned(in Vector2d test, in int v0, in int v1, in int v2) {
			var sign0 = ToLine(test, v1, v2);
			var sign1 = ToLine(test, v0, v2);
			var sign2 = ToLine(test, v0, v1);

			// valid sign patterns are -+- and +-+, but also we might
			// have zeros...can't figure out a more clever test right now
			return (sign0 <= 0 && sign1 >= 0 && sign2 <= 0) ||
				 (sign0 >= 0 && sign1 <= 0 && sign2 >= 0)
				? (sign0 != 0 && sign1 != 0 && sign2 != 0) ? -1 : 0
				: +1;
		}



		// For a triangle with counterclockwise vertices V0, V1, and V2,
		// ToCircumcircle returns
		//   +1, P outside circumcircle of triangle
		//   -1, P inside circumcircle of triangle
		//    0, P on circumcircle of triangle
		// The query involves three calls of ToLine, so the numbers match those
		// of ToLine.
		public int ToCircumcircle(in int i, in int v0, in int v1, in int v2) {
			return ToCircumcircle(_pointF(i), v0, v1, v2);
		}
		public int ToCircumcircle(in Vector2d test, in int v0, in int v1, in int v2) {
			var vec0 = _pointF(v0);
			var vec1 = _pointF(v1);
			var vec2 = _pointF(v2);

			var x0 = vec0[0] - test[0];
			var y0 = vec0[1] - test[1];
			var s00 = vec0[0] + test[0];
			var s01 = vec0[1] + test[1];
			var t00 = s00 * x0;
			var t01 = s01 * y0;
			var z0 = t00 + t01;

			var x1 = vec1[0] - test[0];
			var y1 = vec1[1] - test[1];
			var s10 = vec1[0] + test[0];
			var s11 = vec1[1] + test[1];
			var t10 = s10 * x1;
			var t11 = s11 * y1;
			var z1 = t10 + t11;

			var x2 = vec2[0] - test[0];
			var y2 = vec2[1] - test[1];
			var s20 = vec2[0] + test[0];
			var s21 = vec2[1] + test[1];
			var t20 = s20 * x2;
			var t21 = s21 * y2;
			var z2 = t20 + t21;

			var y0z1 = y0 * z1;
			var y0z2 = y0 * z2;
			var y1z0 = y1 * z0;
			var y1z2 = y1 * z2;
			var y2z0 = y2 * z0;
			var y2z1 = y2 * z1;
			var c0 = y1z2 - y2z1;
			var c1 = y2z0 - y0z2;
			var c2 = y0z1 - y1z0;
			var x0c0 = x0 * c0;
			var x1c1 = x1 * c1;
			var x2c2 = x2 * c2;
			var term = x0c0 + x1c1;
			var det = term + x2c2;
			const double ZERO = 0.0;

			return det < ZERO ? 1 : (det > ZERO ? -1 : 0);
		}

		// An extended classification of the relationship of a point to a line
		// segment.  For noncollinear points, the return value is
		//   ORDER_POSITIVE when <P,Q0,Q1> is a counterclockwise triangle
		//   ORDER_NEGATIVE when <P,Q0,Q1> is a clockwise triangle
		// For collinear points, the line direction is Q1-Q0.  The return value is
		//   ORDER_COLLINEAR_LEFT when the line ordering is <P,Q0,Q1>
		//   ORDER_COLLINEAR_RIGHT when the line ordering is <Q0,Q1,P>
		//   ORDER_COLLINEAR_CONTAIN when the line ordering is <Q0,P,Q1>
		public enum OrderType
		{
			ORDER_Q0_EQUALS_Q1,
			ORDER_P_EQUALS_Q0,
			ORDER_P_EQUALS_Q1,
			ORDER_POSITIVE,
			ORDER_NEGATIVE,
			ORDER_COLLINEAR_LEFT,
			ORDER_COLLINEAR_RIGHT,
			ORDER_COLLINEAR_CONTAIN
		};

		// Choice of N for UIntegerFP32<N>.
		//    input type | compute type | N
		//    -----------+--------------+-----
		//    float      | BSNumber     |   18
		//    double     | BSNumber     |  132
		//    float      | BSRational   |  214
		//    double     | BSRational   | 1587
		// This is the same as the first-listed ToLine calls because the worst-case
		// path has the same computational complexity.
		public static OrderType ToLineExtended(in Vector2d P, in Vector2d Q0, in Vector2d Q1) {
			const double ZERO = 0.0;

			var x0 = Q1[0] - Q0[0];
			var y0 = Q1[1] - Q0[1];
			if (x0 == ZERO && y0 == ZERO) {
				return OrderType.ORDER_Q0_EQUALS_Q1;
			}

			var x1 = P[0] - Q0[0];
			var y1 = P[1] - Q0[1];
			if (x1 == ZERO && y1 == ZERO) {
				return OrderType.ORDER_P_EQUALS_Q0;
			}

			var x2 = P[0] - Q1[0];
			var y2 = P[1] - Q1[1];
			if (x2 == ZERO && y2 == ZERO) {
				return OrderType.ORDER_P_EQUALS_Q1;
			}

			// The theoretical classification relies on computing exactly the sign of
			// the determinant.  Numerical roundoff errors can cause misclassification.
			var x0y1 = x0 * y1;
			var x1y0 = x1 * y0;
			var det = x0y1 - x1y0;

			if (det != ZERO) {
				if (det > ZERO) {
					// The points form a counterclockwise triangle <P,Q0,Q1>.
					return OrderType.ORDER_POSITIVE;
				}
				else {
					// The points form a clockwise triangle <P,Q1,Q0>.
					return OrderType.ORDER_NEGATIVE;
				}
			}
			else {
				// The points are collinear; P is on the line through Q0 and Q1.
				var x0x1 = x0 * x1;
				var y0y1 = y0 * y1;
				var dot = x0x1 + y0y1;
				if (dot < ZERO) {
					// The line ordering is <P,Q0,Q1>.
					return OrderType.ORDER_COLLINEAR_LEFT;
				}

				var x0x0 = x0 * x0;
				var y0y0 = y0 * y0;
				var sqrLength = x0x0 + y0y0;
				if (dot > sqrLength) {
					// The line ordering is <Q0,Q1,P>.
					return OrderType.ORDER_COLLINEAR_RIGHT;
				}

				// The line ordering is <Q0,P,Q1> with P strictly between Q0 and Q1.
				return OrderType.ORDER_COLLINEAR_CONTAIN;
			}
		}
	}

}
