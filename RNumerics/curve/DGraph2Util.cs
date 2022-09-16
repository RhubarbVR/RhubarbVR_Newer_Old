using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{


	/// <summary>
	/// Utility functions for DGraph2 data structure
	/// </summary>
	public static class DGraph2Util
	{


		public class Curves
		{
			public List<Polygon2d> Loops;
			public List<PolyLine2d> Paths;
		}


		/// <summary>
		/// Decompose graph into simple polylines and polygons. 
		/// </summary>
		public static Curves ExtractCurves(in DGraph graph) {
			var c = new Curves {
				Loops = new List<Polygon2d>(),
				Paths = new List<PolyLine2d>()
			};

			var used = new HashSet<int>();

			// find boundary and junction vertices
			var boundaries = new HashSet<int>();
			var junctions = new HashSet<int>();
			foreach (var vid in graph.VertexIndices()) {
				if (graph.IsBoundaryVertex(vid)) {
					boundaries.Add(vid);
				}

				if (graph.IsJunctionVertex(vid)) {
					junctions.Add(vid);
				}
			}

			// walk paths from boundary vertices
			foreach (var start_vid in boundaries) {
				var vid = start_vid;
				var eid = graph.GetVtxEdges(vid)[0];
				if (used.Contains(eid)) {
					continue;
				}

				var path = new PolyLine2d();
				path.AppendVertex(graph.GetVertex(vid));
				while (true) {
					used.Add(eid);
					var next = NextEdgeAndVtx(eid, vid, graph);
					eid = next.a;
					vid = next.b;
					path.AppendVertex(graph.GetVertex(vid));
					if (boundaries.Contains(vid) || junctions.Contains(vid)) {
						break;  // done!
					}
				}
				c.Paths.Add(path);
			}

			// ok we should be done w/ boundary verts now...
			boundaries.Clear();


			foreach (var start_vid in junctions) {
				foreach (var outgoing_eid in graph.VtxEdgesItr(start_vid)) {
					if (used.Contains(outgoing_eid)) {
						continue;
					}

					var vid = start_vid;
					var eid = outgoing_eid;

					var path = new PolyLine2d();
					path.AppendVertex(graph.GetVertex(vid));
					var is_loop = false;
					while (true) {
						used.Add(eid);
						var next = NextEdgeAndVtx(eid, vid, graph);
						eid = next.a;
						vid = next.b;
						if (vid == start_vid) {
							is_loop = true;
							break;
						}
						path.AppendVertex(graph.GetVertex(vid));
						if (eid == int.MaxValue || junctions.Contains(vid)) {
							break;
						}
					}
					if (is_loop) {
						c.Loops.Add(new Polygon2d(path.Vertices));
					}
					else {
						c.Paths.Add(path);
					}
				}

			}


			// all that should be left are continuous loops...
			foreach (var start_eid in graph.EdgeIndices()) {
				if (used.Contains(start_eid)) {
					continue;
				}

				var eid = start_eid;
				var ev = graph.GetEdgeV(eid);
				var vid = ev.a;

				var poly = new Polygon2d();
				poly.AppendVertex(graph.GetVertex(vid));
				while (true) {
					used.Add(eid);
					var next = NextEdgeAndVtx(eid, vid, graph);
					eid = next.a;
					vid = next.b;
					poly.AppendVertex(graph.GetVertex(vid));
					if (eid == int.MaxValue || junctions.Contains(vid)) {
						throw new Exception("how did this happen??");
					}

					if (used.Contains(eid)) {
						break;
					}
				}
				poly.RemoveVertex(poly.VertexCount - 1);
				c.Loops.Add(poly);
			}

			return c;
		}





		/// <summary>
		/// merge members of c.Paths that have unique endpoint pairings.
		/// Does *not* extract closed loops that contain junction vertices,
		/// unless the 'other' end of those junctions is dangling.
		/// Also, horribly innefficient!
		/// </summary>
		public static void ChainOpenPaths(in Curves c, in double epsilon = MathUtil.EPSILON) {
			var to_process = new List<PolyLine2d>(c.Paths);
			c.Paths = new List<PolyLine2d>();

			// first we separate out 'dangling' curves that have no match on at least one side
			var dangling = new List<PolyLine2d>();
			var remaining = new List<PolyLine2d>();

			var bContinue = true;
			while (bContinue && to_process.Count > 0) {
				bContinue = false;
				foreach (var p in to_process) {
					var matches_start = Find_connected_start(p, to_process, epsilon);
					var matches_end = Find_connected_end(p, to_process, epsilon);
					if (matches_start.Count == 0 || matches_end.Count == 0) {
						dangling.Add(p);
						bContinue = true;
					}
					else {
						remaining.Add(p);
					}
				}
				to_process.Clear();
				to_process.AddRange(remaining);
				remaining.Clear();
			}

			//to_process.Clear(); to_process.AddRange(remaining); remaining.Clear();

			// now incrementally merge together unique matches
			// [TODO] this will not match across junctions!
			bContinue = true;
			while (bContinue && to_process.Count > 0) {
				bContinue = false;
			restart_itr:
				foreach (var p in to_process) {
					var matches_start = Find_connected_start(p, to_process, epsilon);
					var matches_end = Find_connected_end(p, to_process, 2 * epsilon);
					if (matches_start.Count == 1 && matches_end.Count == 1 &&
						 matches_start[0] == matches_end[0]) {
						c.Loops.Add(To_loop(p, matches_start[0], epsilon));
						to_process.Remove(p);
						to_process.Remove(matches_start[0]);
						remaining.Remove(matches_start[0]);
						bContinue = true;
						goto restart_itr;
					}
					else if (matches_start.Count == 1 && matches_end.Count < 2) {
						remaining.Add(Merge_paths(matches_start[0], p, 2 * epsilon));
						to_process.Remove(p);
						to_process.Remove(matches_start[0]);
						remaining.Remove(matches_start[0]);
						bContinue = true;
						goto restart_itr;
					}
					else if (matches_end.Count == 1 && matches_start.Count < 2) {
						remaining.Add(Merge_paths(p, matches_end[0], 2 * epsilon));
						to_process.Remove(p);
						to_process.Remove(matches_end[0]);
						remaining.Remove(matches_end[0]);
						bContinue = true;
						goto restart_itr;
					}
					else {
						remaining.Add(p);
					}
				}
				to_process.Clear();
				to_process.AddRange(remaining);
				remaining.Clear();
			}

			c.Paths.AddRange(to_process);

			// [TODO] now that we have found all loops, we can chain in dangling curves

			c.Paths.AddRange(dangling);

		}





		static List<PolyLine2d> Find_connected_start(in PolyLine2d pTest, in List<PolyLine2d> potential, in double eps = MathUtil.EPSILON) {
			var result = new List<PolyLine2d>();
			foreach (var p in potential) {
				if (pTest == p) {
					continue;
				}

				if (pTest.Start.Distance(p.Start) < eps ||
					 pTest.Start.Distance(p.End) < eps) {
					result.Add(p);
				}
			}
			return result;
		}
		static List<PolyLine2d> Find_connected_end(in PolyLine2d pTest, in List<PolyLine2d> potential, in double eps = MathUtil.EPSILON) {
			var result = new List<PolyLine2d>();
			foreach (var p in potential) {
				if (pTest == p) {
					continue;
				}

				if (pTest.End.Distance(p.Start) < eps ||
					 pTest.End.Distance(p.End) < eps) {
					result.Add(p);
				}
			}
			return result;
		}
		static Polygon2d To_loop(in PolyLine2d p1, in PolyLine2d p2, in double eps = MathUtil.EPSILON) {
			var p = new Polygon2d(p1.Vertices);
			if (p1.End.Distance(p2.Start) > eps) {
				p2.Reverse();
			}

			p.AppendVertices(p2);
			return p;
		}
		static PolyLine2d Merge_paths(in PolyLine2d p1, in PolyLine2d p2, in double eps = MathUtil.EPSILON) {
			PolyLine2d pNew;
			if (p1.End.Distance(p2.Start) < eps) {
				pNew = new PolyLine2d(p1);
				pNew.AppendVertices(p2);
			}
			else if (p1.End.Distance(p2.End) < eps) {
				pNew = new PolyLine2d(p1);
				p2.Reverse();
				pNew.AppendVertices(p2);
			}
			else if (p1.Start.Distance(p2.Start) < eps) {
				p2.Reverse();
				pNew = new PolyLine2d(p2);
				pNew.AppendVertices(p1);
			}
			else if (p1.Start.Distance(p2.End) < eps) {
				pNew = new PolyLine2d(p2);
				pNew.AppendVertices(p1);
			}
			else {
				throw new Exception("shit");
			}

			return pNew;
		}


		/// <summary>
		/// If vid has two or more neighbours, returns uniform laplacian, otherwise returns vid position
		/// </summary>
		public static Vector2d VertexLaplacian(in DGraph graph, in int vid, out bool isValid) {
			var v = graph.GetVertex(vid);
			var centroid = Vector2d.Zero;
			var n = 0;
			foreach (var vnbr in graph.VtxVerticesItr(vid)) {
				centroid += graph.GetVertex(vnbr);
				n++;
			}
			if (n == 2) {
				centroid /= n;
				isValid = true;
				return centroid - v;
			}
			isValid = false;
			return v;
		}





		public static bool FindRayIntersection(in Vector2d o, in Vector2d d, out int hit_eid, out double hit_ray_t, DGraph graph) {
			var line = new Line2d(o, d);
			Vector2d a = Vector2d.Zero, b = Vector2d.Zero;

			var near_eid = DGraph.INVALID_ID;
			var near_t = double.MaxValue;

			var intr = new IntrLine2Segment2(line, new Segment2d(a, b));
			foreach (var eid in graph.VertexIndices()) {
				graph.GetEdgeV(eid, ref a, ref b);
				intr.Segment = new Segment2d(a, b);
				if (intr.Find() && intr.IsSimpleIntersection && intr.Parameter > 0) {
					if (intr.Parameter < near_t) {
						near_eid = eid;
						near_t = intr.Parameter;
					}
				}
			}

			hit_eid = near_eid;
			hit_ray_t = near_t;
			return hit_ray_t < double.MaxValue;
		}




		/// <summary>
		/// If we are at edge eid, which as one vertex prev_vid, find 'other' vertex, and other edge connected to that vertex,
		/// and return pair [next_edge, shared_vtx]
		/// Returns [int.MaxValue, shared_vtx] if shared_vtx is not valence=2   (ie stops at boundaries and complex junctions)
		/// </summary>
		public static Index2i NextEdgeAndVtx(in int eid, in int prev_vid, in DGraph graph) {
			var ev = graph.GetEdgeV(eid);
			if (ev.a == DGraph.INVALID_ID) {
				return Index2i.Max;
			}

			var next_vid = (ev.a == prev_vid) ? ev.b : ev.a;

			if (graph.GetVtxEdgeCount(next_vid) != 2) {
				return new Index2i(int.MaxValue, next_vid);
			}

			foreach (var next_eid in graph.VtxEdgesItr(next_vid)) {
				if (next_eid != eid) {
					return new Index2i(next_eid, next_vid);
				}
			}
			return Index2i.Max;
		}




		/// <summary>
		/// walk through graph from fromVtx, in direction of eid, until we hit the next junction vertex
		/// </summary>
		public static List<int> WalkToNextNonRegularVtx(in DGraph graph, in int fromVtx, in int eid) {
			var path = new List<int> {
				fromVtx
			};
			var cur_vid = fromVtx;
			var cur_eid = eid;
			var bContinue = true;
			while (bContinue) {
				var next = DGraph2Util.NextEdgeAndVtx(cur_eid, cur_vid, graph);
				var next_eid = next.a;
				var next_vtx = next.b;
				if (next_eid == int.MaxValue) {
					if (graph.IsRegularVertex(next_vtx) == false) {
						path.Add(next_vtx);
						bContinue = false;
					}
					else {
						throw new Exception("WalkToNextNonRegularVtx: have no next edge but vtx is regular - how?");
					}
				}
				else {
					path.Add(next_vtx);
					cur_vid = next_vtx;
					cur_eid = next_eid;
				}
			}
			return path;
		}





		/// <summary>
		/// compute length of path through graph
		/// </summary>
		public static double PathLength(in DGraph graph, in IList<int> pathVertices) {
			double len = 0;
			var N = pathVertices.Count;
			var prev = graph.GetVertex(pathVertices[0]);
			for (var i = 1; i < N; ++i) {
				var next = graph.GetVertex(pathVertices[i]);
				len += prev.Distance(next);
				prev = next;
			}
			return len;
		}


	}
}
