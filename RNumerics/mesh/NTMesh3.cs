using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RNumerics
{

	//
	// NTMesh3 is a variant of DMesh3 that supports non-manifold mesh topology. 
	// See DMesh3 comments for most details. 
	// Main change is that edges buffer only stores 2-tuple vertex pairs.
	// Each edge can be connected to arbitrary number of triangle, which are
	// stored in edge_triangles
	//
	// per-vertex UVs have been removed (perhaps temporarily)
	//
	// Currently poke-face and split-edge are supported, but not collapse or flip.
	// 
	public partial class NTMesh3 : IDeformableMesh
	{
		public IEnumerable<Vector3f> VertexPos() {
			for (var i = 0; i < VertexCount; i++) {
				yield return GetVertexf(i);
			}
		}
		public const int INVALID_ID = -1;
		public const int NON_MANIFOLD_ID = -2;

		public static readonly Vector3d InvalidVertex = new(double.MaxValue, 0, 0);
		public static readonly Index3i InvalidTriangle = new(INVALID_ID, INVALID_ID, INVALID_ID);
		public static readonly Index2i InvalidEdge = new(INVALID_ID, INVALID_ID);

		RefCountVector _vertices_refcount;
		DVector<double> _vertices;
		DVector<float> _normals;
		DVector<float> _colors;

		SmallListSet _vertex_edges;

		RefCountVector _triangles_refcount;
		DVector<int> _triangles;
		DVector<int> _triangle_edges;
		DVector<int> _triangle_groups;

		RefCountVector _edges_refcount;
		DVector<int> _edges;
		SmallListSet _edge_triangles;

		public NTMesh3(bool bWantNormals = true, bool bWantColors = false, bool bWantTriGroups = false) {
			Allocate(bWantNormals, bWantColors, bWantTriGroups);
		}

		private void Allocate(bool bWantNormals, bool bWantColors, bool bWantTriGroups) {
			_vertices = new DVector<double>();
			if (bWantNormals) {
				_normals = new DVector<float>();
			}

			if (bWantColors) {
				_colors = new DVector<float>();
			}

			_vertex_edges = new SmallListSet();

			_vertices_refcount = new RefCountVector();

			_triangles = new DVector<int>();
			_triangle_edges = new DVector<int>();
			_triangles_refcount = new RefCountVector();
			if (bWantTriGroups) {
				_triangle_groups = new DVector<int>();
			}

			MaxGroupID = 0;

			_edges = new DVector<int>();
			_edges_refcount = new RefCountVector();
			_edge_triangles = new SmallListSet();
		}



		public NTMesh3(NTMesh3 copy) {
			Copy(copy, true, true);
		}

		public void Copy(NTMesh3 copy, bool bNormals = true, bool bColors = true) {
			_vertices = new DVector<double>(copy._vertices);
			_normals = (bNormals && copy._normals != null) ? new DVector<float>(copy._normals) : null;
			_colors = (bColors && copy._colors != null) ? new DVector<float>(copy._colors) : null;

			_vertices_refcount = new RefCountVector(copy._vertices_refcount);
			_vertex_edges = new SmallListSet(copy._vertex_edges);

			_triangles = new DVector<int>(copy._triangles);
			_triangle_edges = new DVector<int>(copy._triangle_edges);
			_triangles_refcount = new RefCountVector(copy._triangles_refcount);
			if (copy._triangle_groups != null) {
				_triangle_groups = new DVector<int>(copy._triangle_groups);
			}

			MaxGroupID = copy.MaxGroupID;

			_edges = new DVector<int>(copy._edges);
			_edges_refcount = new RefCountVector(copy._edges_refcount);
			_edge_triangles = new SmallListSet(copy._edge_triangles);
		}




		void UpdateTimeStamp(bool bShapeChange) {
			Timestamp++;
			if (bShapeChange) {
				ShapeTimestamp++;
			}
		}
		public int Timestamp { get; private set; } = 0;
		public int ShapeTimestamp { get; private set; } = 0;


		// IMesh impl

		public int VertexCount => _vertices_refcount.Count;
		public int TriangleCount => _triangles_refcount.Count;
		public int EdgeCount => _edges_refcount.Count;

		// these values are (max_used+1), ie so an iteration should be < MaxTriangleID, not <=
		public int MaxVertexID => _vertices_refcount.Max_index;
		public int MaxTriangleID => _triangles_refcount.Max_index;
		public int MaxEdgeID => _edges_refcount.Max_index;
		public int MaxGroupID { get; private set; } = 0;

		public bool HasVertexColors => _colors != null;
		public bool HasVertexNormals => _normals != null;
		public bool HasVertexUVs => false;
		public bool HasTriangleGroups => _triangle_groups != null;


		// info

		public bool IsVertex(int vID) {
			return _vertices_refcount.IsValid(vID);
		}
		public bool IsTriangle(int tID) {
			return _triangles_refcount.IsValid(tID);
		}
		public bool IsEdge(int eID) {
			return _edges_refcount.IsValid(eID);
		}


		// getters


		public Vector3d GetVertex(int vID) {
			var i = 3 * vID;
			return new Vector3d(_vertices[i], _vertices[i + 1], _vertices[i + 2]);
		}
		public Vector3f GetVertexf(int vID) {
			var i = 3 * vID;
			return new Vector3f((float)_vertices[i], (float)_vertices[i + 1], (float)_vertices[i + 2]);
		}

		public void SetVertex(int vID, Vector3d vNewPos) {
			Debug.Assert(vNewPos.IsFinite);     // this will really catch a lot of bugs...
			var i = 3 * vID;
			_vertices[i] = vNewPos.x;
			_vertices[i + 1] = vNewPos.y;
			_vertices[i + 2] = vNewPos.z;
			UpdateTimeStamp(true);
		}

		public Vector3f GetVertexNormal(int vID) {
			if (_normals == null) {
				return Vector3f.AxisY;
			}
			else {
				var i = 3 * vID;
				return new Vector3f(_normals[i], _normals[i + 1], _normals[i + 2]);
			}
		}

		public Vector2f GetVertexUV(int i, int channel) {
			return Vector2f.Zero;
		}

		public void SetVertexNormal(int vID, Vector3f vNewNormal) {
			if (HasVertexNormals) {
				var i = 3 * vID;
				_normals[i] = vNewNormal.x;
				_normals[i + 1] = vNewNormal.y;
				_normals[i + 2] = vNewNormal.z;
				UpdateTimeStamp(false);
			}
		}

		public Vector3f GetVertexColor(int vID) {
			if (_colors == null) {
				return Vector3f.One;
			}
			else {
				var i = 3 * vID;
				return new Vector3f(_colors[i], _colors[i + 1], _colors[i + 2]);
			}
		}

		public void SetVertexColor(int vID, Vector3f vNewColor) {
			if (HasVertexColors) {
				var i = 3 * vID;
				_colors[i] = vNewColor.x;
				_colors[i + 1] = vNewColor.y;
				_colors[i + 2] = vNewColor.z;
				UpdateTimeStamp(false);
			}
		}


		public bool GetVertex(int vID, ref NewVertexInfo vinfo, bool bWantNormals, bool bWantColors) {
			if (_vertices_refcount.IsValid(vID) == false) {
				return false;
			}

			vinfo.v.Set(_vertices[3 * vID], _vertices[(3 * vID) + 1], _vertices[(3 * vID) + 2]);
			vinfo.bHaveN = vinfo.bHaveUV = vinfo.bHaveC = false;
			if (HasVertexColors && bWantNormals) {
				vinfo.bHaveN = true;
				vinfo.n.Set(_normals[3 * vID], _normals[(3 * vID) + 1], _normals[(3 * vID) + 2]);
			}
			if (HasVertexColors && bWantColors) {
				vinfo.bHaveC = true;
				vinfo.c.Set(_colors[3 * vID], _colors[(3 * vID) + 1], _colors[(3 * vID) + 2]);
			}
			return true;
		}


		public int GetVtxEdgeCount(int vID) {
			return _vertices_refcount.IsValid(vID) ? _vertex_edges.Count(vID) : -1;
		}


		public int GetMaxVtxEdgeCount() {
			var max = 0;
			foreach (int vid in _vertices_refcount) {
				max = Math.Max(max, _vertex_edges.Count(vid));
			}

			return max;
		}

		public NewVertexInfo GetVertexAll(int i) {
			var vi = new NewVertexInfo {
				v = GetVertex(i)
			};
			if (HasVertexNormals) {
				vi.bHaveN = true;
				vi.n = GetVertexNormal(i);
			}
			else {
				vi.bHaveN = false;
			}

			if (HasVertexColors) {
				vi.bHaveC = true;
				vi.c = GetVertexColor(i);
			}
			else {
				vi.bHaveC = false;
			}

			vi.bHaveUV = false;
			return vi;
		}


		public IEnumerable<int> RenderIndices() {
			var N = TriangleCount;
			for (var i = 0; i < N; ++i) {
				yield return GetTriangle(i).a;
				yield return GetTriangle(i).b;
				yield return GetTriangle(i).c;
			}
		}

		public Index3i GetTriangle(int tID) {
			var i = 3 * tID;
			return new Index3i(_triangles[i], _triangles[i + 1], _triangles[i + 2]);
		}

		public Index3i GetTriEdges(int tID) {
			var i = 3 * tID;
			return new Index3i(_triangle_edges[i], _triangle_edges[i + 1], _triangle_edges[i + 2]);
		}

		public int GetTriEdge(int tid, int j) {
			return _triangle_edges[(3 * tid) + j];
		}


		public IEnumerable<int> TriTrianglesItr(int tID) {
			if (_triangles_refcount.IsValid(tID)) {
				var tei = 3 * tID;
				for (var j = 0; j < 3; ++j) {
					var eid = _triangle_edges[tei + j];
					foreach (var nbr_t in _edge_triangles.ValueItr(eid)) {
						if (nbr_t != tID) {
							yield return nbr_t;
						}
					}
				}
			}
		}



		public int GetTriangleGroup(int tID) {
			return (_triangle_groups == null) ? -1
				: (_triangles_refcount.IsValid(tID) ? _triangle_groups[tID] : 0);
		}

		public void SetTriangleGroup(int tid, int group_id) {
			if (_triangle_groups != null) {
				_triangle_groups[tid] = group_id;
				MaxGroupID = Math.Max(MaxGroupID, group_id + 1);
				UpdateTimeStamp(false);
			}
		}

		public int AllocateTriangleGroup() {
			return MaxGroupID++;
		}


		public void GetTriVertices(int tID, ref Vector3d v0, ref Vector3d v1, ref Vector3d v2) {
			var ai = 3 * _triangles[3 * tID];
			v0.x = _vertices[ai];
			v0.y = _vertices[ai + 1];
			v0.z = _vertices[ai + 2];
			var bi = 3 * _triangles[(3 * tID) + 1];
			v1.x = _vertices[bi];
			v1.y = _vertices[bi + 1];
			v1.z = _vertices[bi + 2];
			var ci = 3 * _triangles[(3 * tID) + 2];
			v2.x = _vertices[ci];
			v2.y = _vertices[ci + 1];
			v2.z = _vertices[ci + 2];
		}

		public Vector3d GetTriVertex(int tid, int j) {
			var a = _triangles[(3 * tid) + j];
			return new Vector3d(_vertices[3 * a], _vertices[(3 * a) + 1], _vertices[(3 * a) + 2]);
		}


		public Vector3d GetTriNormal(int tID) {
			Vector3d v0 = Vector3d.Zero, v1 = Vector3d.Zero, v2 = Vector3d.Zero;
			GetTriVertices(tID, ref v0, ref v1, ref v2);
			return MathUtil.Normal(ref v0, ref v1, ref v2);
		}

		public double GetTriArea(int tID) {
			Vector3d v0 = Vector3d.Zero, v1 = Vector3d.Zero, v2 = Vector3d.Zero;
			GetTriVertices(tID, ref v0, ref v1, ref v2);
			return MathUtil.Area(ref v0, ref v1, ref v2);
		}

		/// <summary>
		/// Compute triangle normal, area, and centroid all at once. Re-uses vertex
		/// lookups and computes normal & area simultaneously. *However* does not produce
		/// the same normal/area as separate calls, because of this.
		/// </summary>
		public void GetTriInfo(int tID, out Vector3d normal, out double fArea, out Vector3d vCentroid) {
			Vector3d v0 = Vector3d.Zero, v1 = Vector3d.Zero, v2 = Vector3d.Zero;
			GetTriVertices(tID, ref v0, ref v1, ref v2);
			vCentroid = 1.0 / 3.0 * (v0 + v1 + v2);
			normal = MathUtil.FastNormalArea(ref v0, ref v1, ref v2, out fArea);
		}




		public AxisAlignedBox3d GetTriBounds(int tID) {
			var vi = 3 * _triangles[3 * tID];
			double x = _vertices[vi], y = _vertices[vi + 1], z = _vertices[vi + 2];
			double minx = x, maxx = x, miny = y, maxy = y, minz = z, maxz = z;
			for (var i = 1; i < 3; ++i) {
				vi = 3 * _triangles[(3 * tID) + i];
				x = _vertices[vi];
				y = _vertices[vi + 1];
				z = _vertices[vi + 2];
				if (x < minx) {
					minx = x;
				}
				else if (x > maxx) {
					maxx = x;
				}

				if (y < miny) {
					miny = y;
				}
				else if (y > maxy) {
					maxy = y;
				}

				if (z < minz) {
					minz = z;
				}
				else if (z > maxz) {
					maxz = z;
				}
			}
			return new AxisAlignedBox3d(minx, miny, minz, maxx, maxy, maxz);
		}


		public Frame3f GetTriFrame(int tID, int nEdge = 0) {
			var ti = 3 * tID;
			var a = _triangles[ti + (nEdge % 3)];
			var b = _triangles[ti + ((nEdge + 1) % 3)];
			var c = _triangles[ti + ((nEdge + 2) % 3)];
			var v0 = new Vector3d(_vertices[3 * a], _vertices[(3 * a) + 1], _vertices[(3 * a) + 2]);
			var v1 = new Vector3d(_vertices[3 * b], _vertices[(3 * b) + 1], _vertices[(3 * b) + 2]);
			var v2 = new Vector3d(_vertices[3 * c], _vertices[(3 * c) + 1], _vertices[(3 * c) + 2]);
			var edge = (Vector3f)(v1 - v0).Normalized;
			var normal = (Vector3f)MathUtil.Normal(ref v0, ref v1, ref v2);
			var other = edge.Cross(normal);
			var center = (Vector3f)(v0 + v1 + v2) / 3;
			return new Frame3f(center, edge, other, normal);
		}





		public Index2i GetEdgeV(int eID) {
			var i = 2 * eID;
			return new Index2i(_edges[i], _edges[i + 1]);
		}
		public bool GetEdgeV(int eID, ref Vector3d a, ref Vector3d b) {
			var iv0 = 3 * _edges[2 * eID];
			a.x = _vertices[iv0];
			a.y = _vertices[iv0 + 1];
			a.z = _vertices[iv0 + 2];
			var iv1 = 3 * _edges[(2 * eID) + 1];
			b.x = _vertices[iv1];
			b.y = _vertices[iv1 + 1];
			b.z = _vertices[iv1 + 2];
			return true;
		}


		public IEnumerable<int> EdgeTrianglesItr(int eID) {
			return _edge_triangles.ValueItr(eID);
		}

		public int EdgeTrianglesCount(int eID) {
			return _edge_triangles.Count(eID);
		}


		// return same indices as GetEdgeV, but oriented based on attached triangle
		public Index2i GetOrientedBoundaryEdgeV(int eID) {
			if (_edges_refcount.IsValid(eID) && Edge_is_boundary(eID)) {
				var ei = 2 * eID;
				int a = _edges[ei], b = _edges[ei + 1];

				var ti = _edge_triangles.First(eID);
				var tri = new Index3i(_triangles[ti], _triangles[ti + 1], _triangles[ti + 2]);
				var ai = IndexUtil.Find_edge_index_in_tri(a, b, ref tri);
				return new Index2i(tri[ai], tri[(ai + 1) % 3]);
			}
			return InvalidEdge;
		}


		// mesh-building


		public int AppendVertex(Vector3d v) {
			return AppendVertex(new NewVertexInfo() {
				v = v,
				bHaveC = false,
				bHaveUV = false,
				bHaveN = false
			});
		}
		public int AppendVertex(NewVertexInfo info) {
			var vid = _vertices_refcount.Allocate();
			var i = 3 * vid;
			_vertices.Insert(info.v[2], i + 2);
			_vertices.Insert(info.v[1], i + 1);
			_vertices.Insert(info.v[0], i);

			if (_normals != null) {
				var n = info.bHaveN ? info.n : Vector3f.AxisY;
				_normals.Insert(n[2], i + 2);
				_normals.Insert(n[1], i + 1);
				_normals.Insert(n[0], i);
			}

			if (_colors != null) {
				var c = info.bHaveC ? info.c : Vector3f.One;
				_colors.Insert(c[2], i + 2);
				_colors.Insert(c[1], i + 1);
				_colors.Insert(c[0], i);
			}

			Allocate_vertex_edges_list(vid);

			UpdateTimeStamp(true);
			return vid;
		}


		public int AppendTriangle(int v0, int v1, int v2, int gid = -1) {
			return AppendTriangle(new Index3i(v0, v1, v2), gid);
		}
		public int AppendTriangle(Index3i tv, int gid = -1) {
			if (IsVertex(tv[0]) == false || IsVertex(tv[1]) == false || IsVertex(tv[2]) == false) {
				return INVALID_ID;
			}
			if (tv[0] == tv[1] || tv[0] == tv[2] || tv[1] == tv[2]) {
				return INVALID_ID;
			}

			// look up edges. 
			var e0 = Find_edge(tv[0], tv[1]);
			var e1 = Find_edge(tv[1], tv[2]);
			var e2 = Find_edge(tv[2], tv[0]);

			// now safe to insert triangle
			var tid = _triangles_refcount.Allocate();
			var i = 3 * tid;
			_triangles.Insert(tv[2], i + 2);
			_triangles.Insert(tv[1], i + 1);
			_triangles.Insert(tv[0], i);
			if (_triangle_groups != null) {
				_triangle_groups.Insert(gid, tid);
				MaxGroupID = Math.Max(MaxGroupID, gid + 1);
			}

			// increment ref counts and update/create edges
			_vertices_refcount.Increment(tv[0]);
			_vertices_refcount.Increment(tv[1]);
			_vertices_refcount.Increment(tv[2]);

			Add_tri_edge(tid, tv[0], tv[1], 0, e0);
			Add_tri_edge(tid, tv[1], tv[2], 1, e1);
			Add_tri_edge(tid, tv[2], tv[0], 2, e2);

			UpdateTimeStamp(true);
			return tid;
		}
		// helper fn for above, just makes code cleaner
		void Add_tri_edge(int tid, int v0, int v1, int j, int eid) {
			if (eid != INVALID_ID) {
				_edge_triangles.Insert(eid, tid);
				_triangle_edges.Insert(eid, (3 * tid) + j);
			}
			else {
				eid = Add_edge(v0, v1, tid);
				_triangle_edges.Insert(eid, (3 * tid) + j);
			}
		}





		public void EnableVertexNormals(Vector3f initial_normal) {
			if (HasVertexNormals) {
				return;
			}

			_normals = new DVector<float>();
			var NV = MaxVertexID;
			_normals.Resize(3 * NV);
			for (var i = 0; i < NV; ++i) {
				var vi = 3 * i;
				_normals[vi] = initial_normal.x;
				_normals[vi + 1] = initial_normal.y;
				_normals[vi + 2] = initial_normal.z;
			}
		}
		public void DiscardVertexNormals() {
			_normals = null;
		}

		public void EnableVertexColors(Vector3f initial_color) {
			if (HasVertexColors) {
				return;
			}

			_colors = new DVector<float>();
			var NV = MaxVertexID;
			_colors.Resize(3 * NV);
			for (var i = 0; i < NV; ++i) {
				var vi = 3 * i;
				_colors[vi] = initial_color.x;
				_colors[vi + 1] = initial_color.y;
				_colors[vi + 2] = initial_color.z;
			}
		}
		public void DiscardVertexColors() {
			_colors = null;
		}


		public void EnableTriangleGroups(int initial_group = 0) {
			if (HasTriangleGroups) {
				return;
			}

			_triangle_groups = new DVector<int>();
			var NT = MaxTriangleID;
			_triangle_groups.Resize(NT);
			for (var i = 0; i < NT; ++i) {
				_triangle_groups[i] = initial_group;
			}

			MaxGroupID = 0;
		}
		public void DiscardTriangleGroups() {
			_triangle_groups = null;
			MaxGroupID = 0;
		}









		// iterators

		public IEnumerable<int> VertexIndices() {
			foreach (int vid in _vertices_refcount) {
				yield return vid;
			}
		}
		public IEnumerable<int> TriangleIndices() {
			foreach (int tid in _triangles_refcount) {
				yield return tid;
			}
		}
		public IEnumerable<int> EdgeIndices() {
			foreach (int eid in _edges_refcount) {
				yield return eid;
			}
		}


		public IEnumerable<int> BoundaryEdgeIndices() {
			foreach (int eid in _edges_refcount) {
				if (_edge_triangles.Count(eid) == 1) {
					yield return eid;
				}
			}
		}


		public IEnumerable<Vector3d> Vertices() {
			foreach (int vid in _vertices_refcount) {
				var i = 3 * vid;
				yield return new Vector3d(_vertices[i], _vertices[i + 1], _vertices[i + 2]);
			}
		}
		public IEnumerable<Index3i> Triangles() {
			foreach (int tid in _triangles_refcount) {
				var i = 3 * tid;
				yield return new Index3i(_triangles[i], _triangles[i + 1], _triangles[i + 2]);
			}
		}


		// queries

		// linear search through edges of vA
		public int FindEdge(int vA, int vB) {
			return Find_edge(vA, vB);
		}

		// faster than FindEdge
		public int FindEdgeFromTri(int vA, int vB, int t) {
			return Find_edge_from_tri(vA, vB, t);
		}

		// [RMS] not just 2 in NTMesh...
		//     public Index2i GetEdgeOpposingV(int eID)
		//     {
		//// ** it is important that verts returned maintain [c,d] order!!
		//int i = 4*eID;
		//         int a = edges[i], b = edges[i + 1];
		//         int t0 = edges[i + 2], t1 = edges[i + 3];
		//int c = IndexUtil.find_tri_other_vtx(a, b, triangles, t0);
		//         if (t1 != InvalidID) {
		//	int d = IndexUtil.find_tri_other_vtx(a, b, triangles, t1);
		//             return new Index2i(c, d);
		//         } else
		//             return new Index2i(c, InvalidID);
		//     }



		public IEnumerable<int> VtxVerticesItr(int vID) {
			if (_vertices_refcount.IsValid(vID)) {
				foreach (var eid in _vertex_edges.ValueItr(vID)) {
					yield return Edge_other_v(eid, vID);
				}
			}
		}


		public IEnumerable<int> VtxEdgesItr(int vID) {
			return _vertices_refcount.IsValid(vID) ? _vertex_edges.ValueItr(vID) : Enumerable.Empty<int>();
		}


		/// <summary>
		/// Returns count of boundary edges at vertex
		/// </summary>
		public int VtxBoundaryEdges(int vID) {
			if (_vertices_refcount.IsValid(vID)) {
				var count = 0;
				foreach (var eid in _vertex_edges.ValueItr(vID)) {
					if (_edge_triangles.Count(eid) == 1) {
						count++;
					}
				}
				return count;
			}
			Debug.Assert(false);
			return -1;
		}

		/// <summary>
		/// e needs to be large enough (ie call VtxBoundaryEdges, or as large as max one-ring)
		/// returns count, ie number of elements of e that were filled
		/// </summary>
		public int VtxAllBoundaryEdges(int vID, int[] e) {
			if (_vertices_refcount.IsValid(vID)) {
				var count = 0;
				foreach (var eid in _vertex_edges.ValueItr(vID)) {
					if (_edge_triangles.Count(eid) == 1) {
						e[count++] = eid;
					}
				}
				return count;
			}
			Debug.Assert(false);
			return -1;
		}






		protected bool Tri_has_v(int tID, int vID) {
			var i = 3 * tID;
			return _triangles[i] == vID
				|| _triangles[i + 1] == vID
				|| _triangles[i + 2] == vID;
		}

		protected bool Tri_is_boundary(int tID) {
			var i = 3 * tID;
			return Edge_is_boundary(_triangle_edges[i])
				|| Edge_is_boundary(_triangle_edges[i + 1])
				|| Edge_is_boundary(_triangle_edges[i + 2]);
		}

		protected bool Tri_has_neighbour_t(int tCheck, int tNbr) {
			var i = 3 * tCheck;
			return Edge_has_t(_triangle_edges[i], tNbr)
				|| Edge_has_t(_triangle_edges[i + 1], tNbr)
				|| Edge_has_t(_triangle_edges[i + 2], tNbr);
		}

		protected bool Tri_has_sequential_v(int tID, int vA, int vB) {
			var i = 3 * tID;
			int v0 = _triangles[i], v1 = _triangles[i + 1], v2 = _triangles[i + 2];
			return (v0 == vA && v1 == vB) || (v1 == vA && v2 == vB) || (v2 == vA && v0 == vB);
		}

		//! returns edge ID
		protected int Find_tri_neighbour_edge(int tID, int vA, int vB) {
			var i = 3 * tID;
			int tv0 = _triangles[i], tv1 = _triangles[i + 1];
			if (IndexUtil.Same_pair_unordered(tv0, tv1, vA, vB)) {
				return _triangle_edges[3 * tID];
			}

			var tv2 = _triangles[i + 2];
			return IndexUtil.Same_pair_unordered(tv1, tv2, vA, vB)
				? _triangle_edges[(3 * tID) + 1]
				: IndexUtil.Same_pair_unordered(tv2, tv0, vA, vB) ? _triangle_edges[(3 * tID) + 2] : INVALID_ID;
		}

		// returns 0/1/2
		protected int Find_tri_neighbour_index(int tID, int vA, int vB) {
			var i = 3 * tID;
			int tv0 = _triangles[i], tv1 = _triangles[i + 1];
			if (IndexUtil.Same_pair_unordered(tv0, tv1, vA, vB)) {
				return 0;
			}

			var tv2 = _triangles[i + 2];
			return IndexUtil.Same_pair_unordered(tv1, tv2, vA, vB) ? 1 : IndexUtil.Same_pair_unordered(tv2, tv0, vA, vB) ? 2 : INVALID_ID;
		}


		public bool IsNonManifoldEdge(int eid) {
			return _edge_triangles.Count(eid) > 2;
		}
		public bool IsBoundaryEdge(int eid) {
			return _edge_triangles.Count(eid) == 1;
		}


		protected bool Edge_is_boundary(int eid) {
			return _edge_triangles.Count(eid) == 1;
		}
		protected bool Edge_has_v(int eid, int vid) {
			var i = 2 * eid;
			return (_edges[i] == vid) || (_edges[i + 1] == vid);
		}
		protected bool Edge_has_t(int eid, int tid) {
			return _edge_triangles.Contains(eid, tid);
		}
		protected int Edge_other_v(int eID, int vID) {
			var i = 2 * eID;
			int ev0 = _edges[i], ev1 = _edges[i + 1];
			return (ev0 == vID) ? ev1 : ((ev1 == vID) ? ev0 : INVALID_ID);
		}

		public bool Vertex_is_boundary(int vID) {
			return IsBoundaryVertex(vID);
		}
		public bool IsBoundaryVertex(int vID) {
			foreach (var eid in _vertex_edges.ValueItr(vID)) {
				if (_edge_triangles.Count(eid) == 1) {
					return true;
				}
			}
			return false;
		}


		public bool IsBoundaryTriangle(int tID) {
			var i = 3 * tID;
			return IsBoundaryEdge(_triangle_edges[i]) || IsBoundaryEdge(_triangle_edges[i + 1]) || IsBoundaryEdge(_triangle_edges[i + 2]);
		}



		int Find_edge(int vA, int vB) {
			// [RMS] edge vertices must be sorted (min,max),
			//   that means we only need one index-check in inner loop.
			//   commented out code is robust to incorrect ordering, but slower.
			var vO = Math.Max(vA, vB);
			var vI = Math.Min(vA, vB);
			foreach (var eid in _vertex_edges.ValueItr(vI)) {
				if (_edges[(2 * eid) + 1] == vO) {
					//if (edge_has_v(eid, vO))
					return eid;
				}
			}
			return INVALID_ID;

			// this is slower, likely because it creates new func<> every time. can we do w/o that?
			//return vertex_edges.Find(vI, (eid) => { return edges[4 * eid + 1] == vO; }, InvalidID);
		}

		int Find_edge_from_tri(int vA, int vB, int tID) {
			var i = 3 * tID;
			int t0 = _triangles[i], t1 = _triangles[i + 1];
			if (IndexUtil.Same_pair_unordered(vA, vB, t0, t1)) {
				return _triangle_edges[i];
			}

			var t2 = _triangles[i + 2];
			return IndexUtil.Same_pair_unordered(vA, vB, t1, t2)
				? _triangle_edges[i + 1]
				: IndexUtil.Same_pair_unordered(vA, vB, t2, t0) ? _triangle_edges[i + 2] : INVALID_ID;
		}



		// compute vertex bounding box
		public AxisAlignedBox3d GetBounds() {
			double x = 0, y = 0, z = 0;
			foreach (int vi in _vertices_refcount) {
				x = _vertices[3 * vi];
				y = _vertices[(3 * vi) + 1];
				z = _vertices[(3 * vi) + 2];
				break;
			}
			double minx = x, maxx = x, miny = y, maxy = y, minz = z, maxz = z;
			foreach (int vi in _vertices_refcount) {
				x = _vertices[3 * vi];
				y = _vertices[(3 * vi) + 1];
				z = _vertices[(3 * vi) + 2];
				if (x < minx) {
					minx = x;
				}
				else if (x > maxx) {
					maxx = x;
				}

				if (y < miny) {
					miny = y;
				}
				else if (y > maxy) {
					maxy = y;
				}

				if (z < minz) {
					minz = z;
				}
				else if (z > maxz) {
					maxz = z;
				}
			}
			return new AxisAlignedBox3d(minx, miny, minz, maxx, maxy, maxz);
		}

		AxisAlignedBox3d _cached_bounds;
		int _cached_bounds_timestamp = -1;

		//! cached bounding box, lazily re-computed on access if mesh has changed
		public AxisAlignedBox3d CachedBounds
		{
			get {
				if (_cached_bounds_timestamp != Timestamp) {
					_cached_bounds = GetBounds();
					_cached_bounds_timestamp = Timestamp;
				}
				return _cached_bounds;
			}
		}




		bool _cached_is_closed = false;
		int _cached_is_closed_timstamp = -1;

		public bool IsClosed() {
			if (TriangleCount == 0) {
				return false;
			}
			// [RMS] under possibly-mistaken belief that foreach() has some overhead...
			if (MaxEdgeID / EdgeCount > 5) {
				foreach (int eid in _edges_refcount) {
					if (Edge_is_boundary(eid)) {
						return false;
					}
				}
			}
			else {
				var N = MaxEdgeID;
				for (var i = 0; i < N; ++i) {
					if (_edges_refcount.IsValid(i) && Edge_is_boundary(i)) {
						return false;
					}
				}
			}
			return true;
		}

		public bool CachedIsClosed
		{
			get {
				if (_cached_is_closed_timstamp != Timestamp) {
					_cached_is_closed = IsClosed();
					_cached_is_closed_timstamp = Timestamp;
				}
				return _cached_is_closed;
			}
		}




		public bool IsCompact => _vertices_refcount.Is_dense && _edges_refcount.Is_dense && _triangles_refcount.Is_dense;
		public bool IsCompactV => _vertices_refcount.Is_dense;


















		// internal

		public void Set_triangle(int tid, int v0, int v1, int v2) {
			var i = 3 * tid;
			_triangles[i] = v0;
			_triangles[i + 1] = v1;
			_triangles[i + 2] = v2;
		}
		public void Set_triangle_edges(int tid, int e0, int e1, int e2) {
			var i = 3 * tid;
			_triangle_edges[i] = e0;
			_triangle_edges[i + 1] = e1;
			_triangle_edges[i + 2] = e2;
		}

		public int Add_edge(int vA, int vB, int tA, int tB = INVALID_ID) {
			if (vB < vA) {
				(vA, vB) = (vB, vA);
			}
			var eid = _edges_refcount.Allocate();
			Allocate_edge_triangles_list(eid);

			var i = 2 * eid;
			_edges.Insert(vA, i);
			_edges.Insert(vB, i + 1);

			if (tA != INVALID_ID) {
				_edge_triangles.Insert(eid, tA);
			}

			if (tB != INVALID_ID) {
				_edge_triangles.Insert(eid, tB);
			}

			_vertex_edges.Insert(vA, eid);
			_vertex_edges.Insert(vB, eid);
			return eid;
		}

		public int Replace_tri_vertex(int tID, int vOld, int vNew) {
			var i = 3 * tID;
			if (_triangles[i] == vOld) { _triangles[i] = vNew; return 0; }
			if (_triangles[i + 1] == vOld) { _triangles[i + 1] = vNew; return 1; }
			if (_triangles[i + 2] == vOld) { _triangles[i + 2] = vNew; return 2; }
			return -1;
		}

		public int Add_triangle_only(int a, int b, int c, int e0, int e1, int e2) {
			var tid = _triangles_refcount.Allocate();
			var i = 3 * tid;
			_triangles.Insert(c, i + 2);
			_triangles.Insert(b, i + 1);
			_triangles.Insert(a, i);
			_triangle_edges.Insert(e2, i + 2);
			_triangle_edges.Insert(e1, i + 1);
			_triangle_edges.Insert(e0, i + 0);
			return tid;
		}



		public void Allocate_vertex_edges_list(int vid) {
			if (vid < _vertex_edges.Size) {
				_vertex_edges.Clear(vid);
			}

			_vertex_edges.AllocateAt(vid);
		}
		public List<int> Vertex_edges_list(int vid) {
			return new List<int>(_vertex_edges.ValueItr(vid));
		}


		public void Allocate_edge_triangles_list(int eid) {
			if (eid < _edge_triangles.Size) {
				_edge_triangles.Clear(eid);
			}

			_edge_triangles.AllocateAt(eid);
		}


		public void Set_edge_vertices(int eID, int a, int b) {
			var i = 2 * eID;
			_edges[i] = Math.Min(a, b);
			_edges[i + 1] = Math.Max(a, b);
		}

		public int Replace_edge_vertex(int eID, int vOld, int vNew) {
			var i = 2 * eID;
			int a = _edges[i], b = _edges[i + 1];
			if (a == vOld) {
				_edges[i] = Math.Min(b, vNew);
				_edges[i + 1] = Math.Max(b, vNew);
				return 0;
			}
			else if (b == vOld) {
				_edges[i] = Math.Min(a, vNew);
				_edges[i + 1] = Math.Max(a, vNew);
				return 1;
			}
			else {
				return -1;
			}
		}


		public bool Replace_edge_triangle(int eID, int tOld, int tNew) {
			var found = _edge_triangles.Remove(eID, tOld);
			_edge_triangles.Insert(eID, tNew);
			return found;
		}

		public void Add_edge_triangle(int eID, int tID) {
			_edge_triangles.Insert(eID, tID);
		}
		public bool Remove_edge_triangle(int eID, int tID) {
			return _edge_triangles.Remove(eID, tID);
		}

		public int Replace_triangle_edge(int tID, int eOld, int eNew) {
			var i = 3 * tID;
			if (_triangle_edges[i] == eOld) {
				_triangle_edges[i] = eNew;
				return 0;
			}
			else if (_triangle_edges[i + 1] == eOld) {
				_triangle_edges[i + 1] = eNew;
				return 1;
			}
			else if (_triangle_edges[i + 2] == eOld) {
				_triangle_edges[i + 2] = eNew;
				return 2;
			}
			else {
				return -1;
			}
		}
	}
}

