using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace RNumerics
{

	/// <summary>
	/// Base class for Arbitrary-Topology Graphs. Similar structure to topology parts of DMesh3.
	/// Each vertex can be connected to an arbitrary number of edges.
	/// Each edge can have an integer GroupID.
	/// See DGraph2 and DGraph3 for 3d implementations.
	/// Use DGraphN if you would like a topology-only graph.
	/// You cannot instantiate DGraph directly.
	/// </summary>
	public abstract class ADGraph
	{
		public const int INVALID_ID = -1;
		public const int DUPLICATE_EDGE_ID = -2;

		public static readonly Index2i InvalidEdgeV = new(INVALID_ID, INVALID_ID);
		public static readonly Index3i InvalidEdge3 = new(INVALID_ID, INVALID_ID, INVALID_ID);


		protected RefCountVector vertices_refcount;

		protected DVector<List<int>> vertex_edges;

		protected RefCountVector edges_refcount;
		protected DVector<int> edges;   // each edge is a tuple (v0,v0,group_id)

		protected int timestamp = 0;
		protected int shape_timestamp = 0;

		protected int max_group_id = 0;


		public ADGraph() {
			vertex_edges = new DVector<List<int>>();
			vertices_refcount = new RefCountVector();

			edges = new DVector<int>();
			edges_refcount = new RefCountVector();
			max_group_id = 0;
		}


		protected void UpdateTimeStamp(in bool bShapeChange) {
			timestamp++;
			if (bShapeChange) {
				shape_timestamp++;
			}
		}
		public int Timestamp => timestamp;
		public int ShapeTimestamp => shape_timestamp;



		public int VertexCount => vertices_refcount.Count;
		public int EdgeCount => edges_refcount.Count;


		// these values are (max_used+1), ie so an iteration should be < MaxVertexID, not <=
		public int MaxVertexID => vertices_refcount.Max_index;
		public int MaxEdgeID => edges_refcount.Max_index;
		public int MaxGroupID => max_group_id;


		public bool IsVertex(in int vID) {
			return vertices_refcount.IsValid(vID);
		}
		public bool IsEdge(in int eID) {
			return edges_refcount.IsValid(eID);
		}

		public ReadOnlyCollection<int> GetVtxEdges(in int vID) {
			return vertices_refcount.IsValid(vID) ?
				vertex_edges[vID].AsReadOnly() : null;
		}

		public int GetVtxEdgeCount(in int vID) {
			return vertices_refcount.IsValid(vID) ?
				vertex_edges[vID].Count : -1;
		}


		public int GetMaxVtxEdgeCount() {
			var max = 0;
			foreach (int vid in vertices_refcount) {
				max = Math.Max(max, vertex_edges[vid].Count);
			}

			return max;
		}





		public int GetEdgeGroup(in int eid) {
			return edges_refcount.IsValid(eid) ? edges[(3 * eid) + 2] : -1;
		}

		public void SetEdgeGroup(in int eid, in int group_id) {
			Debug.Assert(edges_refcount.IsValid(eid));
			if (edges_refcount.IsValid(eid)) {
				edges[(3 * eid) + 2] = group_id;
				max_group_id = Math.Max(max_group_id, group_id + 1);
				UpdateTimeStamp(false);
			}
		}

		public int AllocateEdgeGroup() {
			return max_group_id++;
		}



		public Index2i GetEdgeV(in int eID) {
			return edges_refcount.IsValid(eID) ?
				new Index2i(edges[3 * eID], edges[(3 * eID) + 1]) : InvalidEdgeV;
		}


		public Index3i GetEdge(in int eID) {
			var j = 3 * eID;
			return edges_refcount.IsValid(eID) ?
				new Index3i(edges[j], edges[j + 1], edges[j + 2]) : InvalidEdge3;
		}




		protected int Append_vertex_internal() {
			var vid = vertices_refcount.Allocate();
			vertex_edges.Insert(new List<int>(), vid);
			UpdateTimeStamp(true);
			return vid;
		}



		public int AppendEdge(in int v0, in int v1, in int gid = -1) {
			return AppendEdge(new Index2i(v0, v1), gid);
		}
		public int AppendEdge(in Index2i ev, in int gid = -1) {
			if (IsVertex(ev[0]) == false || IsVertex(ev[1]) == false) {
				return INVALID_ID;
			}
			if (ev[0] == ev[1]) {
				return INVALID_ID;
			}
			var e0 = FindEdge(ev[0], ev[1]);
			if (e0 != INVALID_ID) {
				return DUPLICATE_EDGE_ID;
			}

			// increment ref counts and update/create edges
			vertices_refcount.Increment(ev[0]);
			vertices_refcount.Increment(ev[1]);
			max_group_id = Math.Max(max_group_id, gid + 1);

			// now safe to insert edge
			var eid = Add_edge(ev[0], ev[1], gid);

			UpdateTimeStamp(true);
			return eid;
		}

		protected int Add_edge(int a, int b, in int gid) {
			if (b < a) {
				(a, b) = (b, a);
			}
			var eid = edges_refcount.Allocate();
			var i = 3 * eid;
			edges.Insert(a, i);
			edges.Insert(b, i + 1);
			edges.Insert(gid, i + 2);

			vertex_edges[a].Add(eid);
			vertex_edges[b].Add(eid);
			return eid;
		}




		// iterators

		public IEnumerable<int> VertexIndices() {
			foreach (int vid in vertices_refcount) {
				yield return vid;
			}
		}
		public IEnumerable<int> EdgeIndices() {
			foreach (int eid in edges_refcount) {
				yield return eid;
			}
		}




		// return value is [v0,v1,gid]
		public IEnumerable<Index3i> Edges() {
			foreach (int eid in edges_refcount) {
				var i = 3 * eid;
				yield return new Index3i(edges[i], edges[i + 1], edges[i + 2]);
			}
		}


		public IEnumerable<int> VtxVerticesItr(int vID) {
			if (vertices_refcount.IsValid(vID)) {
				var vedges = vertex_edges[vID];
				var N = vedges.Count;
				for (var i = 0; i < N; ++i) {
					yield return Edge_other_v(vedges[i], vID);
				}
			}
		}


		public IEnumerable<int> VtxEdgesItr(int vID) {
			if (vertices_refcount.IsValid(vID)) {
				var vedges = vertex_edges[vID];
				var N = vedges.Count;
				for (var i = 0; i < N; ++i) {
					yield return vedges[i];
				}
			}
		}


		public int FindEdge(in int vA, in int vB) {
			var vO = Math.Max(vA, vB);
			var e0 = vertex_edges[Math.Min(vA, vB)];
			var N = e0.Count;
			for (var i = 0; i < N; ++i) {
				if (Edge_has_v(e0[i], vO)) {
					return e0[i];
				}
			}
			return INVALID_ID;
		}


		protected bool Edge_has_v(in int eid, in int vid) {
			var i = 3 * eid;
			return (edges[i] == vid) || (edges[i + 1] == vid);
		}
		protected int Edge_other_v(in int eID, in int vID) {
			var i = 3 * eID;
			int ev0 = edges[i], ev1 = edges[i + 1];
			return (ev0 == vID) ? ev1 : ((ev1 == vID) ? ev0 : INVALID_ID);
		}
		protected int Replace_edge_vertex(in int eID, in int vOld, in int vNew) {
			var i = 3 * eID;
			int a = edges[i], b = edges[i + 1];
			if (a == vOld) {
				edges[i] = Math.Min(b, vNew);
				edges[i + 1] = Math.Max(b, vNew);
				return 0;
			}
			else if (b == vOld) {
				edges[i] = Math.Min(a, vNew);
				edges[i + 1] = Math.Max(a, vNew);
				return 1;
			}
			else {
				return -1;
			}
		}


		public bool IsCompact => vertices_refcount.Is_dense && edges_refcount.Is_dense;
		public bool IsCompactV => vertices_refcount.Is_dense;



		public bool IsBoundaryVertex(in int vID) {
			return vertices_refcount.IsValid(vID) && vertex_edges[vID].Count == 1;
		}

		public bool IsJunctionVertex(in int vID) {
			return vertices_refcount.IsValid(vID) && vertex_edges[vID].Count > 2;
		}

		public bool IsRegularVertex(in int vID) {
			return vertices_refcount.IsValid(vID) && vertex_edges[vID].Count == 2;
		}





		public enum FailMode { DebugAssert, gDevAssert, Throw, ReturnOnly }

		/// <summary>
		// This function checks that the graph is well-formed, ie all internal data
		// structures are consistent
		/// </summary>
		public virtual bool CheckValidity(in FailMode eFailMode = FailMode.Throw) {
			var is_ok = true;
			Action<bool> CheckOrFailF = (b) => is_ok = is_ok && b;
			if (eFailMode == FailMode.DebugAssert) {
				CheckOrFailF = (b) => {
					Debug.Assert(b);
					is_ok = is_ok && b;
				};
			}
			else if (eFailMode == FailMode.gDevAssert) {
				CheckOrFailF = (b) => is_ok = is_ok && b;
			}
			else if (eFailMode == FailMode.Throw) {
				CheckOrFailF = (b) => {
					if (b == false) {
						throw new Exception("DGraph3.CheckValidity: check failed");
					}
				};
			}

			// edge verts/tris must exist
			foreach (var eID in EdgeIndices()) {
				CheckOrFailF(IsEdge(eID));
				CheckOrFailF(edges_refcount.RefCount(eID) == 1);
				var ev = GetEdgeV(eID);
				CheckOrFailF(IsVertex(ev[0]));
				CheckOrFailF(IsVertex(ev[1]));
				CheckOrFailF(ev[0] < ev[1]);
			}

			// verify compact check
			var is_compact = vertices_refcount.Is_dense;
			if (is_compact) {
				for (var vid = 0; vid < VertexCount; ++vid) {
					CheckOrFailF(vertices_refcount.IsValid(vid));
				}
			}

			// vertex edges must exist and reference this vert
			foreach (var vID in VertexIndices()) {
				CheckOrFailF(IsVertex(vID));

				//Vector3d v = GetVertex(vID);
				//CheckOrFailF(double.IsNaN(v.LengthSquared) == false);
				//CheckOrFailF(double.IsInfinity(v.LengthSquared) == false);

				var l = vertex_edges[vID];
				foreach (var edgeid in l) {
					CheckOrFailF(IsEdge(edgeid));
					CheckOrFailF(Edge_has_v(edgeid, vID));

					var otherV = Edge_other_v(edgeid, vID);
					var e2 = FindEdge(vID, otherV);
					CheckOrFailF(e2 != INVALID_ID);
					CheckOrFailF(e2 == edgeid);
					e2 = FindEdge(otherV, vID);
					CheckOrFailF(e2 != INVALID_ID);
					CheckOrFailF(e2 == edgeid);
				}

				CheckOrFailF(vertices_refcount.RefCount(vID) == l.Count + 1);

			}

			Subclass_validity_checks(CheckOrFailF);

			return is_ok;
		}
		protected virtual void Subclass_validity_checks(in Action<bool> CheckOrFailF) {
		}
	}





	/// <summary>
	/// Implementation of DGraph that has no dimensionality, ie no data
	/// stored for vertieces besides indices. 
	/// </summary>
	public sealed class DGraphN : DGraph
	{
		public int AppendVertex() {
			return Append_vertex_internal();
		}


		// internal used in SplitEdge
		protected override int Append_new_split_vertex(in int a, in int b) {
			return AppendVertex();
		}

	}



}
