using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;


namespace RNumerics
{
	/// <summary>
	/// Arbitrary-Topology 2D Graph. This is similar to DMesh3 but without faces. 
	/// Each vertex can be connected to an arbitrary number of edges.
	/// Each vertex can have a 3-float color, and edge edge can have an integer GroupID
	/// </summary>
	public class DGraph : ADGraph
	{
		public static readonly Vector2d InvalidVertex = new(double.MaxValue, 0);
		readonly DVector<double> _vertices;
		private DVector<float> _colors;


		public DGraph() : base() {
			_vertices = new DVector<double>();
		}

		public DGraph(in DGraph copy):base() {
			_vertices = new DVector<double>();
			AppendGraph(copy);
		}


		public Vector2d GetVertex(in int vID) {
			return vertices_refcount.IsValid(vID) ?
				new Vector2d(_vertices[2 * vID], _vertices[(2 * vID) + 1]) : InvalidVertex;
		}

		public void SetVertex(in int vID, in Vector2d vNewPos) {
			Debug.Assert(vNewPos.IsFinite);     // this will really catch a lot of bugs...
			if (vertices_refcount.IsValid(vID)) {
				var i = 2 * vID;
				_vertices[i] = vNewPos.x;
				_vertices[i + 1] = vNewPos.y;
				UpdateTimeStamp(true);
			}
		}


		public Vector3f GetVertexColor(in int vID) {
			if (_colors == null) {
				return Vector3f.One;
			}
			else {
				var i = 3 * vID;
				return new Vector3f(_colors[i], _colors[i + 1], _colors[i + 2]);
			}
		}

		public void SetVertexColor(in int vID, in Vector3f vNewColor) {
			if (HasVertexColors) {
				var i = 3 * vID;
				_colors[i] = vNewColor.x;
				_colors[i + 1] = vNewColor.y;
				_colors[i + 2] = vNewColor.z;
				UpdateTimeStamp(false);
			}
		}


		public bool GetEdgeV(in int eID, ref Vector2d a, ref Vector2d b) {
			if (edges_refcount.IsValid(eID)) {
				var iv0 = 2 * edges[3 * eID];
				a.x = _vertices[iv0];
				a.y = _vertices[iv0 + 1];
				var iv1 = 2 * edges[(3 * eID) + 1];
				b.x = _vertices[iv1];
				b.y = _vertices[iv1 + 1];
				return true;
			}
			return false;
		}


		public Segment2d GetEdgeSegment(in int eID) {
			if (edges_refcount.IsValid(eID)) {
				var iv0 = 2 * edges[3 * eID];
				var iv1 = 2 * edges[(3 * eID) + 1];
				return new Segment2d(new Vector2d(_vertices[iv0], _vertices[iv0 + 1]),
									 new Vector2d(_vertices[iv1], _vertices[iv1 + 1]));
			}
			throw new Exception("DGraph2.GetEdgeSegment: invalid segment with id " + eID);
		}

		public Vector2d GetEdgeCenter(in int eID) {
			if (edges_refcount.IsValid(eID)) {
				var iv0 = 2 * edges[3 * eID];
				var iv1 = 2 * edges[(3 * eID) + 1];
				return new Vector2d((_vertices[iv0] + _vertices[iv1]) * 0.5,
									(_vertices[iv0 + 1] + _vertices[iv1 + 1]) * 0.5);
			}
			throw new Exception("DGraph2.GetEdgeCenter: invalid segment with id " + eID);
		}

		public int AppendVertex(in Vector2d v) {
			return AppendVertex(v, Vector3f.One);
		}
		public int AppendVertex(in Vector2d v, in Vector3f c) {
			var vid = Append_vertex_internal();
			var i = 2 * vid;
			_vertices.Insert(v[1], i + 1);
			_vertices.Insert(v[0], i);

			if (_colors != null) {
				i = 3 * vid;
				_colors.Insert(c.z, i + 2);
				_colors.Insert(c.y, i + 1);
				_colors.Insert(c.x, i);
			}

			return vid;
		}





		public void AppendPolygon(in Polygon2d poly, in int gid = -1) {
			var first = -1;
			var prev = -1;
			var N = poly.VertexCount;
			for (var i = 0; i < N; ++i) {
				var cur = AppendVertex(poly[i]);
				if (prev == -1) {
					first = cur;
				}
				else {
					AppendEdge(prev, cur, gid);
				}

				prev = cur;
			}
			AppendEdge(prev, first, gid);
		}
		public void AppendPolygon(in GeneralPolygon2d poly, in int gid = -1) {
			AppendPolygon(poly.Outer, gid);
			foreach (var hole in poly.Holes) {
				AppendPolygon(hole, gid);
			}
		}


		public void AppendPolyline(in PolyLine2d poly, in int gid = -1) {
			var prev = -1;
			var N = poly.VertexCount;
			for (var i = 0; i < N; ++i) {
				var cur = AppendVertex(poly[i]);
				if (i > 0) {
					AppendEdge(prev, cur, gid);
				}

				prev = cur;
			}
		}


		public void AppendGraph(in DGraph graph, in int gid = -1) {
			var mapV = new int[graph.MaxVertexID];
			foreach (var vid in graph.VertexIndices()) {
				mapV[vid] = AppendVertex(graph.GetVertex(vid));
			}
			foreach (var eid in graph.EdgeIndices()) {
				var ev = graph.GetEdgeV(eid);
				var use_gid = (gid == -1) ? graph.GetEdgeGroup(eid) : gid;
				AppendEdge(mapV[ev.a], mapV[ev.b], use_gid);
			}
		}



		public bool HasVertexColors => _colors != null;

		public void EnableVertexColors(in Vector3f initial_color) {
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





		// iterators


		public IEnumerable<Vector2d> Vertices() {
			foreach (int vid in vertices_refcount) {
				var i = 2 * vid;
				yield return new Vector2d(_vertices[i], _vertices[i + 1]);
			}
		}


		/// <summary>
		/// return edges around vID sorted by angle, in clockwise order
		/// </summary>
		public int[] SortedVtxEdges(in int vID) {

			if (vertices_refcount.IsValid(vID) == false) {
				return null;
			}

			var vedges = vertex_edges[vID];
			var N = vedges.Count;
			var sorted = new int[N];
			var angles = new double[N];
			var v = new Vector2d(_vertices[2 * vID], _vertices[(2 * vID) + 1]);
			for (var i = 0; i < N; ++i) {
				var nbr_vid = Edge_other_v(vedges[i], vID);
				var dx = _vertices[2 * nbr_vid] - v.x;
				var dy = _vertices[(2 * nbr_vid) + 1] - v.y;
				//angles[i] = Math.Atan2(dy, dx) + Math.PI;   // shift to range [0,2pi]
				angles[i] = MathUtil.Atan2Positive(dy, dx);
				sorted[i] = vedges[i];
			}
			Array.Sort(angles, sorted);
			return sorted;
		}









		// compute vertex bounding box
		public AxisAlignedBox2d GetBounds() {
			double x = 0, y = 0;
			foreach (int vi in vertices_refcount) {
				x = _vertices[2 * vi];
				y = _vertices[(2 * vi) + 1];
				break;
			}
			double minx = x, maxx = x, miny = y, maxy = y;
			foreach (int vi in vertices_refcount) {
				x = _vertices[2 * vi];
				y = _vertices[(2 * vi) + 1];
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
			}
			return new AxisAlignedBox2d(minx, miny, maxx, maxy);
		}

		private AxisAlignedBox2d _cached_bounds;
		int _cached_bounds_timestamp = -1;

		//! cached bounding box, lazily re-computed on access if mesh has changed
		public AxisAlignedBox2d CachedBounds
		{
			get {
				if (_cached_bounds_timestamp != Timestamp) {
					_cached_bounds = GetBounds();
					_cached_bounds_timestamp = Timestamp;
				}
				return _cached_bounds;
			}
		}





		/// <summary>
		/// Compute opening angle at vertex vID. 
		/// If not a vertex, or valence != 2, returns invalidValue argument.
		/// If either edge is degenerate, returns invalidValue argument.
		/// </summary>
		public double OpeningAngle(in int vID, in double invalidValue = double.MaxValue) {
			if (vertices_refcount.IsValid(vID) == false) {
				return invalidValue;
			}

			var vedges = vertex_edges[vID];
			if (vedges.Count != 2) {
				return invalidValue;
			}

			var nbra = Edge_other_v(vedges[0], vID);
			var nbrb = Edge_other_v(vedges[1], vID);

			var v = new Vector2d(_vertices[2 * vID], _vertices[(2 * vID) + 1]);
			var a = new Vector2d(_vertices[2 * nbra], _vertices[(2 * nbra) + 1]);
			var b = new Vector2d(_vertices[2 * nbrb], _vertices[(2 * nbrb) + 1]);
			a -= v;
			if (a.Normalize() == 0) {
				return invalidValue;
			}

			b -= v;
			return b.Normalize() == 0 ? invalidValue : Vector2d.AngleD(a, b);
		}






		// internal used in SplitEdge
		protected virtual int Append_new_split_vertex(in int a, in int b) {
			var vNew = 0.5 * (GetVertex(a) + GetVertex(b));
			var cNew = HasVertexColors ? (0.5f * (GetVertexColor(a) + GetVertexColor(b))) : Vector3f.One;
			var f = AppendVertex(vNew, cNew);
			return f;
		}


		protected override void Subclass_validity_checks(in Action<bool> CheckOrFailF) {
			foreach (var vID in VertexIndices()) {
				var v = GetVertex(vID);
				CheckOrFailF(double.IsNaN(v.LengthSquared) == false);
				CheckOrFailF(double.IsInfinity(v.LengthSquared) == false);
			}
		}




	}
}
