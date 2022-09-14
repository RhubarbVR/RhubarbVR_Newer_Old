using System;

namespace RNumerics
{
	// variant of Wm5::Query2, for special case of 3 points. This is used in
	// IntrTriangle3Triangle3, to avoid allocating arrays
	public sealed class QueryTuple2d
	{
		Vector2dTuple3 _mVertices;

		public QueryTuple2d(in Vector2d v0, in Vector2d v1, in Vector2d v2) {
			_mVertices = new Vector2dTuple3(v0, v1, v2);
		}
		public QueryTuple2d(in Vector2dTuple3 tuple) {
			_mVertices = tuple;
		}

		// Returns:
		//   +1, on right of line
		//   -1, on left of line
		//    0, on the line
		public int ToLine(in int i, in int v0, in int v1) {
			return ToLine(_mVertices[i], v0, v1);
		}

		public int ToLine(in Vector2d test, int v0, int v1) {
			var positive = Sort(ref v0, ref v1);

			var vec0 = _mVertices[v0];
			var vec1 = _mVertices[v1];

			var x0 = test[0] - vec0[0];
			var y0 = test[1] - vec0[1];
			var x1 = vec1[0] - vec0[0];
			var y1 = vec1[1] - vec0[1];

			var det = Det2(x0, y0, x1, y1);
			if (!positive) {
				det = -det;
			}

			return det > (double)0 ? +1 : (det < (double)0 ? -1 : 0);
		}

		// Returns:
		//   +1, outside triangle
		//   -1, inside triangle
		//    0, on triangle
		public int ToTriangle(in int i, in int v0, in int v1, in int v2) {
			return ToTriangle(_mVertices[i], v0, v1, v2);
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



		// Returns:
		//   +1, outside circumcircle of triangle
		//   -1, inside circumcircle of triangle
		//    0, on circumcircle of triangle
		public int ToCircumcircle(in int i, in int v0, in int v1, in int v2) {
			return ToCircumcircle(_mVertices[i], v0, v1, v2);
		}
		//----------------------------------------------------------------------------

		public int ToCircumcircle(Vector2d test, int v0, int v1, int v2) {
			var positive = Sort(ref v0, ref v1, ref v2);

			var vec0 = _mVertices[v0];
			var vec1 = _mVertices[v1];
			var vec2 = _mVertices[v2];

			var s0x = vec0.x + test.x;
			var d0x = vec0.x - test.x;
			var s0y = vec0.y + test.y;
			var d0y = vec0.y - test.y;
			var s1x = vec1.x + test.x;
			var d1x = vec1.x - test.x;
			var s1y = vec1.y + test.y;
			var d1y = vec1.y - test.y;
			var s2x = vec2.x + test.x;
			var d2x = vec2.x - test.x;
			var s2y = vec2.y + test.y;
			var d2y = vec2.y - test.y;
			var z0 = (s0x * d0x) + (s0y * d0y);
			var z1 = (s1x * d1x) + (s1y * d1y);
			var z2 = (s2x * d2x) + (s2y * d2y);

			var det = Det3(d0x, d0y, z0, d1x, d1y, z1, d2x, d2y, z2);
			if (!positive) {
				det = -det;
			}

			return det < 0 ? 1 : (det > (double)0 ? -1 : 0);
		}


		// Helper functions.
		public static double Dot(in double x0, in double y0, in double x1, in double y1) {
			return (x0 * x1) + (y0 * y1);
		}
		public static double Det2(in double x0, in double y0, in double x1, in double y1) {
			return (x0 * y1) - (x1 * y0);
		}
		public static double Det3(in double x0, in double y0, in double z0, in double x1, in double y1,
			in double z1, in double x2, in double y2, in double z2) {
			var c00 = (y1 * z2) - (y2 * z1);
			var c01 = (y2 * z0) - (y0 * z2);
			var c02 = (y0 * z1) - (y1 * z0);
			return (x0 * c00) + (x1 * c01) + (x2 * c02);
		}





		// Support for ordering a set of unique indices into the vertex pool.  On
		// output it is guaranteed that:  v0 < v1 < v2.  This is used to guarantee
		// consistent queries when the vertex ordering of a primitive is permuted,
		// a necessity when using floating-point arithmetic that suffers from
		// numerical round-off errors.  The input indices are considered the
		// positive ordering.  The output indices are either positively ordered
		// (an even number of transpositions occurs during sorting) or negatively
		// ordered (an odd number of transpositions occurs during sorting).  The
		// functions return 'true' for a positive ordering and 'false' for a
		// negative ordering.

		public static bool Sort(ref int v0, ref int v1) {
			int j0, j1;
			bool positive;

			if (v0 < v1) {
				j0 = 0;
				j1 = 1;
				positive = true;
			}
			else {
				j0 = 1;
				j1 = 0;
				positive = false;
			}
			var value = new Index2i(v0, v1);
			v0 = value[j0];
			v1 = value[j1];
			return positive;
		}

		public static bool Sort(ref int v0, ref int v1, ref int v2) {
			int j0, j1, j2;
			bool positive;

			if (v0 < v1) {
				if (v2 < v0) {
					j0 = 2;
					j1 = 0;
					j2 = 1;
					positive = true;
				}
				else if (v2 < v1) {
					j0 = 0;
					j1 = 2;
					j2 = 1;
					positive = false;
				}
				else {
					j0 = 0;
					j1 = 1;
					j2 = 2;
					positive = true;
				}
			}
			else {
				if (v2 < v1) {
					j0 = 2;
					j1 = 1;
					j2 = 0;
					positive = false;
				}
				else if (v2 < v0) {
					j0 = 1;
					j1 = 2;
					j2 = 0;
					positive = true;
				}
				else {
					j0 = 1;
					j1 = 0;
					j2 = 2;
					positive = false;
				}
			}
			var value = new Index3i(v0, v1, v2);
			v0 = value[j0];
			v1 = value[j1];
			v2 = value[j2];
			return positive;
		}

		public static bool Sort(ref int v0, ref int v1, ref int v2, ref int v3) {
			int j0, j1, j2, j3;
			bool positive;

			if (v0 < v1) {
				if (v2 < v3) {
					if (v1 < v2) {
						j0 = 0;
						j1 = 1;
						j2 = 2;
						j3 = 3;
						positive = true;
					}
					else if (v3 < v0) {
						j0 = 2;
						j1 = 3;
						j2 = 0;
						j3 = 1;
						positive = true;
					}
					else if (v2 < v0) {
						if (v3 < v1) {
							j0 = 2;
							j1 = 0;
							j2 = 3;
							j3 = 1;
							positive = false;
						}
						else {
							j0 = 2;
							j1 = 0;
							j2 = 1;
							j3 = 3;
							positive = true;
						}
					}
					else {
						if (v3 < v1) {
							j0 = 0;
							j1 = 2;
							j2 = 3;
							j3 = 1;
							positive = true;
						}
						else {
							j0 = 0;
							j1 = 2;
							j2 = 1;
							j3 = 3;
							positive = false;
						}
					}
				}
				else {
					if (v1 < v3) {
						j0 = 0;
						j1 = 1;
						j2 = 3;
						j3 = 2;
						positive = false;
					}
					else if (v2 < v0) {
						j0 = 3;
						j1 = 2;
						j2 = 0;
						j3 = 1;
						positive = false;
					}
					else if (v3 < v0) {
						if (v2 < v1) {
							j0 = 3;
							j1 = 0;
							j2 = 2;
							j3 = 1;
							positive = true;
						}
						else {
							j0 = 3;
							j1 = 0;
							j2 = 1;
							j3 = 2;
							positive = false;
						}
					}
					else {
						if (v2 < v1) {
							j0 = 0;
							j1 = 3;
							j2 = 2;
							j3 = 1;
							positive = false;
						}
						else {
							j0 = 0;
							j1 = 3;
							j2 = 1;
							j3 = 2;
							positive = true;
						}
					}
				}
			}
			else {
				if (v2 < v3) {
					if (v0 < v2) {
						j0 = 1;
						j1 = 0;
						j2 = 2;
						j3 = 3;
						positive = false;
					}
					else if (v3 < v1) {
						j0 = 2;
						j1 = 3;
						j2 = 1;
						j3 = 0;
						positive = false;
					}
					else if (v2 < v1) {
						if (v3 < v0) {
							j0 = 2;
							j1 = 1;
							j2 = 3;
							j3 = 0;
							positive = true;
						}
						else {
							j0 = 2;
							j1 = 1;
							j2 = 0;
							j3 = 3;
							positive = false;
						}
					}
					else {
						if (v3 < v0) {
							j0 = 1;
							j1 = 2;
							j2 = 3;
							j3 = 0;
							positive = false;
						}
						else {
							j0 = 1;
							j1 = 2;
							j2 = 0;
							j3 = 3;
							positive = true;
						}
					}
				}
				else {
					if (v0 < v3) {
						j0 = 1;
						j1 = 0;
						j2 = 3;
						j3 = 2;
						positive = true;
					}
					else if (v2 < v1) {
						j0 = 3;
						j1 = 2;
						j2 = 1;
						j3 = 0;
						positive = true;
					}
					else if (v3 < v1) {
						if (v2 < v0) {
							j0 = 3;
							j1 = 1;
							j2 = 2;
							j3 = 0;
							positive = false;
						}
						else {
							j0 = 3;
							j1 = 1;
							j2 = 0;
							j3 = 2;
							positive = true;
						}
					}
					else {
						if (v2 < v0) {
							j0 = 1;
							j1 = 3;
							j2 = 2;
							j3 = 0;
							positive = true;
						}
						else {
							j0 = 1;
							j1 = 3;
							j2 = 0;
							j3 = 2;
							positive = false;
						}
					}
				}
			}

			var value = new Index4i(v0, v1, v2, v3);
			v0 = value[j0];
			v1 = value[j1];
			v2 = value[j2];
			v3 = value[j3];
			return positive;
		}


	}
}
