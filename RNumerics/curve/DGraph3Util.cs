using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{


	/// <summary>
	/// Utility functions for DGraph3 data structure
	/// </summary>
	public static class DGraph3Util
	{
		public struct Curves
		{
			public List<DCurve3> Loops;
			public List<DCurve3> Paths;

			public HashSet<int> BoundaryV;
			public HashSet<int> JunctionV;

			public List<List<int>> LoopEdges;
			public List<List<int>> PathEdges;
		}


		/// <summary>
		/// Decompose graph into simple polylines and polygons. 
		/// </summary>
		public static Curves ExtractCurves(in DGraph3 graph,
			bool bWantLoopIndices = false,
			Func<int, bool> CurveOrientationF = null) {
			var c = new Curves {
				Loops = new List<DCurve3>(),
				Paths = new List<DCurve3>()
			};
			if (bWantLoopIndices) {
				c.LoopEdges = new List<List<int>>();
				c.PathEdges = new List<List<int>>();
			}

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

				var reverse = (CurveOrientationF != null) && CurveOrientationF(eid);

				var path = new DCurve3() { Closed = false };
				var pathE = bWantLoopIndices ? new List<int>() : null;
				path.AppendVertex(graph.GetVertex(vid));
				if (pathE != null) {
					pathE.Add(eid);
				}

				while (true) {
					used.Add(eid);
					var next = NextEdgeAndVtx(eid, vid, graph);
					eid = next.a;
					vid = next.b;
					path.AppendVertex(graph.GetVertex(vid));
					if (boundaries.Contains(vid) || junctions.Contains(vid)) {
						break;  // done!
					}

					if (pathE != null) {
						pathE.Add(eid);
					}
				}
				if (reverse) {
					path.Reverse();
				}

				c.Paths.Add(path);

				if (pathE != null) {
					if (reverse) {
						pathE.Reverse();
					}

					c.PathEdges.Add(pathE);
				}
			}

			// ok we should be done w/ boundary verts now...
			//boundaries.Clear();
			c.BoundaryV = boundaries;


			foreach (var start_vid in junctions) {
				foreach (var outgoing_eid in graph.VtxEdgesItr(start_vid)) {
					if (used.Contains(outgoing_eid)) {
						continue;
					}

					var vid = start_vid;
					var eid = outgoing_eid;

					var reverse = (CurveOrientationF != null) && CurveOrientationF(eid);

					var path = new DCurve3() { Closed = false };
					var pathE = bWantLoopIndices ? new List<int>() : null;
					path.AppendVertex(graph.GetVertex(vid));
					if (pathE != null) {
						pathE.Add(eid);
					}

					while (true) {
						used.Add(eid);
						var next = NextEdgeAndVtx(eid, vid, graph);
						eid = next.a;
						vid = next.b;
						path.AppendVertex(graph.GetVertex(vid));
						if (eid == int.MaxValue || junctions.Contains(vid)) {
							break;  // done!
						}

						if (pathE != null) {
							pathE.Add(eid);
						}
					}

					// we could end up back at our start junction vertex!
					if (vid == start_vid) {
						path.RemoveVertex(path.VertexCount - 1);
						path.Closed = true;
						if (reverse) {
							path.Reverse();
						}

						c.Loops.Add(path);

						if (pathE != null) {
							if (reverse) {
								pathE.Reverse();
							}

							c.LoopEdges.Add(pathE);
						}

						// need to mark incoming edge as used...but is it valid now?
						//Util.gDevAssert(eid != int.MaxValue);
						if (eid != int.MaxValue) {
							used.Add(eid);
						}
					}
					else {
						if (reverse) {
							path.Reverse();
						}

						c.Paths.Add(path);

						if (pathE != null) {
							if (reverse) {
								pathE.Reverse();
							}

							c.PathEdges.Add(pathE);
						}
					}
				}

			}
			c.JunctionV = junctions;


			// all that should be left are continuous loops...
			foreach (var start_eid in graph.EdgeIndices()) {
				if (used.Contains(start_eid)) {
					continue;
				}

				var eid = start_eid;
				var ev = graph.GetEdgeV(eid);
				var vid = ev.a;

				var reverse = (CurveOrientationF != null) && CurveOrientationF(eid);

				var poly = new DCurve3() { Closed = true };
				var polyE = bWantLoopIndices ? new List<int>() : null;
				poly.AppendVertex(graph.GetVertex(vid));
				if (polyE != null) {
					polyE.Add(eid);
				}

				while (true) {
					used.Add(eid);
					var next = NextEdgeAndVtx(eid, vid, graph);
					eid = next.a;
					vid = next.b;
					poly.AppendVertex(graph.GetVertex(vid));
					if (polyE != null) {
						polyE.Add(eid);
					}

					if (eid == int.MaxValue || junctions.Contains(vid)) {
						throw new Exception("how did this happen??");
					}

					if (used.Contains(eid)) {
						break;
					}
				}
				poly.RemoveVertex(poly.VertexCount - 1);
				if (reverse) {
					poly.Reverse();
				}

				c.Loops.Add(poly);

				if (polyE != null) {
					polyE.RemoveAt(polyE.Count - 1);
					if (reverse) {
						polyE.Reverse();
					}

					c.LoopEdges.Add(polyE);
				}
			}

			return c;
		}




		/// <summary>
		/// If we are at edge eid, which as one vertex prev_vid, find 'other' vertex, and other edge connected to that vertex,
		/// and return pair [next_edge, shared_vtx]
		/// Returns [int.MaxValue, shared_vtx] if shared_vtx is not valence=2   (ie stops at boundaries and complex junctions)
		/// </summary>
		public static Index2i NextEdgeAndVtx(in int eid, in int prev_vid, in DGraph3 graph) {
			var ev = graph.GetEdgeV(eid);
			if (ev.a == DGraph3.INVALID_ID) {
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
		public static List<int> WalkToNextNonRegularVtx(in DGraph3 graph, in int fromVtx, in int eid) {
			var path = new List<int> {
				fromVtx
			};
			var cur_vid = fromVtx;
			var cur_eid = eid;
			var bContinue = true;
			while (bContinue) {
				var next = DGraph3Util.NextEdgeAndVtx(cur_eid, cur_vid, graph);
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
	}
}
