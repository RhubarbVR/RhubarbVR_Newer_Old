using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// 2D Polyline/Polygon simplification.
	/// 
	/// This is a more complex approach than Polygon.Simplify(), which uses sequential vtx clustering
	/// and then runs douglas-peucker algorithm. That method can end up modifying long straight segments,
	/// which is not ideal in many contexts (eg manufacturing).
	/// 
	/// Strategy here is :
	///   1) find runs of vertices that are very close to straight lines (default 0.01mm deviation tol)
	///   2) find all straight segments longer than threshold distance (default 2mm)
	///   3) discard vertices that deviate less than tolerance (default = 0.2mm)
	///      from sequential-points-segment, unless they are required to preserve
	///      straight segments
	///      
	/// [TODO] currently doing greedy search in 1,3. Could do more optimal search.
	/// [TODO] currently measuring deviation of p1...pN-1 from line [p0,pN] for points [p0,p1,...pN].
	///   could alternately fit best segment to p1...pN (p0 is already fixed).
	/// [TODO] 2d variant of variational shape segmentation?
	/// 
	/// </summary>
	public sealed class PolySimplification2
	{
		readonly List<Vector2d> _vertices;
		readonly bool _isLoop;

		/// <summary>
		/// A series of points that deviates less than this distance from
		/// a line segment are considered 'on' that line
		/// </summary>
		public double StraightLineDeviationThreshold = 0.01;

		/// <summary>
		/// After collapsing straight lines, any segment longer than
		/// this distance is explicitly preserved
		/// </summary>
		public double PreserveStraightSegLen = 2.0f;

		/// <summary>
		/// we skip vertices that deviate less than this distance from
		/// the currently-accumulated line segment
		/// </summary>
		public double SimplifyDeviationThreshold = 0.2;

		public List<Vector2d> Result;


		public PolySimplification2(in Polygon2d polygon) {
			_vertices = new List<Vector2d>(polygon.Vertices);
			_isLoop = true;
		}

		public PolySimplification2(in PolyLine2d polycurve) {
			_vertices = new List<Vector2d>(polycurve.Vertices);
			_isLoop = false;
		}



		/// <summary>
		/// simplify outer and holes of a polygon solid with same thresholds
		/// </summary>
		public static void Simplify(in GeneralPolygon2d solid, in double deviationThresh) {
			var simp = new PolySimplification2(solid.Outer) {
				SimplifyDeviationThreshold = deviationThresh
			};
			simp.Simplify();
			solid.Outer.SetVertices(simp.Result, true);

			foreach (var hole in solid.Holes) {
				var holesimp = new PolySimplification2(hole) {
					SimplifyDeviationThreshold = deviationThresh
				};
				holesimp.Simplify();
				hole.SetVertices(holesimp.Result, true);
			}
		}



		public void Simplify() {
			var keep_seg = new bool[_vertices.Count];
			Array.Clear(keep_seg, 0, keep_seg.Length);

			// collapse straight lines
			var linear = Collapse_by_deviation_tol(_vertices, keep_seg, StraightLineDeviationThreshold);

			Find_constrained_segments(linear, keep_seg);

			Result = Collapse_by_deviation_tol(linear, keep_seg, SimplifyDeviationThreshold);
		}



		void Find_constrained_segments(in List<Vector2d> vertices, in bool[] markers) {
			var N = vertices.Count;
			var NStop = _isLoop ? vertices.Count : vertices.Count - 1;
			for (var si = 0; si < NStop; si++) {
				int i0 = si, i1 = (si + 1) % N;
				if (vertices[i0].DistanceSquared(vertices[i1]) > PreserveStraightSegLen) {
					markers[i0] = true;
				}
			}

		}



		List<Vector2d> Collapse_by_deviation_tol(in List<Vector2d> input, in bool[] keep_segments, in double offset_threshold) {
			var N = input.Count;
			var NStop = _isLoop ? input.Count : input.Count - 1;

			var result = new List<Vector2d> {
				input[0]
			};

			var thresh_sqr = offset_threshold * offset_threshold;

			var last_i = 0;
			var cur_i = 1;
			var skip_count = 0;

			if (keep_segments[0]) {         // if first segment is constrained
				result.Add(input[1]);
				last_i = 1;
				cur_i = 2;
			}

			while (cur_i < NStop) {
				int i0 = cur_i, i1 = (cur_i + 1) % N;

				if (keep_segments[i0]) {
					if (last_i != i0) {
						// skip join segment if it is degenerate
						var join_dist = input[i0].Distance(result[result.Count - 1]);
						if (join_dist > MathUtil.EPSILON) {
							result.Add(input[i0]);
						}
					}
					result.Add(input[i1]);
					last_i = i1;
					skip_count = 0;
					cur_i = i1 == 0 ? NStop : i1;
					continue;
				}

				var dir = input[i1] - input[last_i];
				var accum_line = new Line2d(input[last_i], dir.Normalized);

				// find deviation of vertices between last_i and next
				double max_dev_sqr = 0;
				for (var k = last_i + 1; k <= cur_i; k++) {
					var distSqr = accum_line.DistanceSquared(input[k]);
					if (distSqr > max_dev_sqr) {
						max_dev_sqr = distSqr;
					}
				}

				// if we deviated too much, we keep this first vertex
				if (max_dev_sqr > thresh_sqr) {
					result.Add(input[cur_i]);
					last_i = cur_i;
					cur_i++;
					skip_count = 0;
				}
				else {
					// skip this vertex
					cur_i++;
					skip_count++;
				}
			}


			if (_isLoop) {
				// if we skipped everything, rest of code doesn't work
				if (result.Count < 3) {
					return Handle_tiny_case(result, input);
				}

				var last_line = Line2d.FromPoints(input[last_i], input[cur_i % N]);
				var collinear_startv = last_line.DistanceSquared(result[0]) < thresh_sqr;
				var collinear_starts = last_line.DistanceSquared(result[1]) < thresh_sqr;
				if (collinear_startv && collinear_starts && result.Count > 3) {
					// last seg is collinear w/ start seg, merge them
					result[0] = input[last_i];
					result.RemoveAt(result.Count - 1);

				}
				else if (collinear_startv) {
					// skip last vertex

				}
				else {
					result.Add(input[input.Count - 1]);
				}

			}
			else {
				// in polyline we always add last vertex
				result.Add(input[input.Count - 1]);
			}

			return result;
		}



		List<Vector2d> Handle_tiny_case(in List<Vector2d> result, in List<Vector2d> input) {
			var N = input.Count;
			if (N == 3) {
				return input;       // not much we can really do here...
			}

			result.Clear();
			result.Add(input[0]);
			result.Add(input[N / 3]);
			result.Add(input[N - (N / 3)]);
			return result;
		}


	}
}
