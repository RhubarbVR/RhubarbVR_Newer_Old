using System;
using System.Collections.Generic;

namespace RNumerics
{
	public static class CurveUtils2
	{


		public static IParametricCurve2d Convert(in Polygon2d poly) {
			var seq = new ParametricCurveSequence2();
			var N = poly.VertexCount;
			for (var i = 0; i < N; ++i) {
				seq.Append(new Segment2d(poly[i], poly[(i + 1) % N]));
			}

			seq.IsClosed = true;
			return seq;
		}


		// 2D curve utils?
		public static double SampledDistance(in IParametricCurve2d c, in Vector2d point, in int N = 100) {
			var tMax = c.ParamLength;
			var min_dist = double.MaxValue;
			for (var i = 0; i <= N; ++i) {
				var fT = (double)i / (double)N;
				fT *= tMax;
				var p = c.SampleT(fT);
				var d = p.DistanceSquared(point);
				if (d < min_dist) {
					min_dist = d;
				}
			}
			return Math.Sqrt(min_dist);
		}


		/// <summary>
		/// if the children of C are a tree, iterate through all the leaves
		/// </summary>
		public static IEnumerable<IParametricCurve2d> LeafCurvesIteration(IParametricCurve2d c) {
			if (c is IMultiCurve2d) {
				var multiC = c as IMultiCurve2d;
				foreach (var c2 in multiC.Curves) {
					foreach (var c3 in LeafCurvesIteration(c2)) {
						yield return c3;
					}
				}
			}
			else {
				yield return c;
			}
		}


		public static List<IParametricCurve2d> Flatten(in List<IParametricCurve2d> curves) {
			var l = new List<IParametricCurve2d>();
			foreach (var sourceC in curves) {
				foreach (var c in LeafCurvesIteration(sourceC)) {
					l.Add(c);
				}
			}
			return l;
		}
		public static List<IParametricCurve2d> Flatten(in IParametricCurve2d curve) {
			return new List<IParametricCurve2d>(LeafCurvesIteration(curve));
		}


		// returns largest scalar coordinate value, useful for converting to integer coords
		public static Vector2d GetMaxOriginDistances(in IEnumerable<Vector2d> vertices) {
			var max = Vector2d.Zero;
			foreach (var v in vertices) {
				var x = Math.Abs(v.x);
				if (x > max.x) {
					max.x = x;
				}

				var y = Math.Abs(v.y);
				if (y > max.y) {
					max.y = y;
				}
			}
			return max;
		}


		public static int FindNearestVertex(in Vector2d pt, in IEnumerable<Vector2d> vertices) {
			var i = 0;
			var iNearest = -1;
			var nearestSqr = double.MaxValue;
			foreach (var v in vertices) {
				var d = v.DistanceSquared(pt);
				if (d < nearestSqr) {
					nearestSqr = d;
					iNearest = i;
				}
				i++;
			}
			return iNearest;
		}


		public static Vector2d CentroidVtx(in IEnumerable<Vector2d> vertices) {
			var c = Vector2d.Zero;
			var count = 0;
			foreach (var v in vertices) {
				c += v;
				count++;
			}
			if (count > 1) {
				c /= (double)count;
			}

			return c;
		}




		public static void LaplacianSmooth(in IList<Vector2d> vertices, in double alpha, in int iterations, in bool is_loop, in bool in_place = false) {
			var N = vertices.Count;
			Vector2d[] temp = null;
			if (in_place == false) {
				temp = new Vector2d[N];
			}

			var set = in_place ? vertices : temp;

			var beta = 1.0 - alpha;
			for (var ii = 0; ii < iterations; ++ii) {
				if (is_loop) {
					for (var i = 0; i < N; ++i) {
						var c = (vertices[(i + N - 1) % N] + vertices[(i + 1) % N]) * 0.5;
						set[i] = (beta * vertices[i]) + (alpha * c);
					}
				}
				else {
					set[0] = vertices[0];
					set[N - 1] = vertices[N - 1];
					for (var i = 1; i < N - 1; ++i) {
						var c = (vertices[i - 1] + vertices[i + 1]) * 0.5;
						set[i] = (beta * vertices[i]) + (alpha * c);
					}
				}

				if (in_place == false) {
					for (var i = 0; i < N; ++i) {
						vertices[i] = set[i];
					}
				}
			}
		}





		/// <summary>
		/// Constrained laplacian smoothing of input polygon, alpha X iterations.
		/// vertices are only allowed to move at most max_dist from constraint
		/// if bAllowShrink == false, vertices are kept outside input polygon
		/// if bAllowGrow == false, vertices are kept inside input polygon
		/// 
		/// max_dist is measured from vertex[i] to original_vertex[i], unless
		/// you set bPerVertexDistances = false, then distance to original polygon
		/// is used (which is much more expensive)
		/// 
		/// [TODO] this is pretty hacky...could be better in lots of ways...
		/// 
		/// </summary>
		public static void LaplacianSmoothConstrained(in Polygon2d poly, in double alpha, in int iterations,
			in double max_dist, in bool bAllowShrink, in bool bAllowGrow, in bool bPerVertexDistances = true) {
			var origPoly = new Polygon2d(poly);

			var N = poly.VertexCount;
			var newV = new Vector2d[poly.VertexCount];

			var max_dist_sqr = max_dist * max_dist;

			var beta = 1.0 - alpha;
			for (var ii = 0; ii < iterations; ++ii) {
				for (var i = 0; i < N; ++i) {
					var curpos = poly[i];
					var smoothpos = (poly[(i + N - 1) % N] + poly[(i + 1) % N]) * 0.5;

					var do_smooth = true;
					if (bAllowShrink == false || bAllowGrow == false) {
						var is_inside = origPoly.Contains(smoothpos);
						do_smooth = is_inside == true ? bAllowShrink : bAllowGrow;
					}

					// [RMS] this is old code...I think not correct?
					//bool contained = true;
					//if (bAllowShrink == false || bAllowGrow == false)
					//    contained = origPoly.Contains(smoothpos);
					//bool do_smooth = true;
					//if (bAllowShrink && contained == false)
					//    do_smooth = false;
					//if (bAllowGrow && contained == true)
					//    do_smooth = false;

					if (do_smooth) {
						var newpos = (beta * curpos) + (alpha * smoothpos);
						if (bPerVertexDistances) {
							while (origPoly[i].DistanceSquared(newpos) > max_dist_sqr) {
								newpos = (newpos + curpos) * 0.5;
							}
						}
						else {
							while (origPoly.DistanceSquared(newpos) > max_dist_sqr) {
								newpos = (newpos + curpos) * 0.5;
							}
						}
						newV[i] = newpos;
					}
					else {
						newV[i] = curpos;
					}
				}

				for (var i = 0; i < N; ++i) {
					poly[i] = newV[i];
				}
			}
		}




		public static void LaplacianSmoothConstrained(in GeneralPolygon2d solid, in double alpha, in int iterations, in double max_dist, in bool bAllowShrink, in bool bAllowGrow) {
			LaplacianSmoothConstrained(solid.Outer, alpha, iterations, max_dist, bAllowShrink, bAllowGrow);
			foreach (var hole in solid.Holes) {
				CurveUtils2.LaplacianSmoothConstrained(hole, alpha, iterations, max_dist, bAllowShrink, bAllowGrow);
			}
		}



		/// <summary>
		/// return list of objects for which keepF(obj) returns true
		/// </summary>
		public static List<T> Filter<T>(in List<T> objects, in Func<T, bool> keepF) {
			var result = new List<T>(objects.Count);
			foreach (var obj in objects) {
				if (keepF(obj)) {
					result.Add(obj);
				}
			}
			return result;
		}


		/// <summary>
		/// Split the input list into two new lists, based on predicate (set1 == true)
		/// </summary>
		public static void Split<T>(in List<T> objects, out List<T> set1, out List<T> set2, in Func<T, bool> splitF) {
			set1 = new List<T>();
			set2 = new List<T>();
			foreach (var obj in objects) {
				if (splitF(obj)) {
					set1.Add(obj);
				}
				else {
					set2.Add(obj);
				}
			}
		}



		public static Polygon2d SplitToTargetLength(in Polygon2d poly, in double length) {
			var result = new Polygon2d();
			result.AppendVertex(poly[0]);
			for (var j = 0; j < poly.VertexCount; ++j) {
				var next = (j + 1) % poly.VertexCount;
				var len = poly[j].Distance(poly[next]);
				if (len < length) {
					result.AppendVertex(poly[next]);
					continue;
				}

				var steps = (int)Math.Ceiling(len / length);
				for (var k = 1; k < steps; ++k) {
					var t = k / (double)steps;
					var v = ((1.0 - t) * poly[j]) + (t * poly[next]);
					result.AppendVertex(v);
				}

				if (j < poly.VertexCount - 1) {
					result.AppendVertex(poly[next]);
				}
			}

			return result;
		}




		/// <summary>
		/// Remove polygons and polygon-holes smaller than minArea
		/// </summary>
		public static List<GeneralPolygon2d> FilterDegenerate(in List<GeneralPolygon2d> polygons, in double minArea) {
			var result = new List<GeneralPolygon2d>(polygons.Count);
			var filteredHoles = new List<Polygon2d>();
			foreach (var poly in polygons) {
				if (poly.Outer.Area < minArea) {
					continue;
				}

				if (poly.Holes.Count == 0) {
					result.Add(poly);
					continue;
				}
				filteredHoles.Clear();
				for (var i = 0; i < poly.Holes.Count; ++i) {
					var hole = poly.Holes[i];
					if (hole.Area > minArea) {
						filteredHoles.Add(hole);
					}
				}
				if (filteredHoles.Count != poly.Holes.Count) {
					poly.ClearHoles();
					foreach (var h in filteredHoles) {
						poly.AddHole(h, false, false);
					}
				}
				result.Add(poly);
			}
			return result;
		}



	}
}
