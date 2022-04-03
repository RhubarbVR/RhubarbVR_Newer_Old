using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;


namespace RNumerics
{
	/// <summary>
	/// Arbitrary-Topology 3D Graph. This is similar to DMesh3 but without faces. 
	/// Each vertex can be connected to an arbitrary number of edges.
	/// Each vertex can have a 3-float color, and edge edge can have an integer GroupID
	/// </summary>
	public class DGraph3 : ADGraph
	{

		public static readonly Vector3d InvalidVertex = new(double.MaxValue, 0, 0);
		readonly DVector<double> _vertices;
		DVector<float> _colors;

		public DGraph3() : base() {
			_vertices = new DVector<double>();
		}

		public DGraph3(DGraph3 copy) : base() {
			_vertices = new DVector<double>();
			AppendGraph(copy);
		}



		public Vector3d GetVertex(int vID) {
			var i = 3 * vID;
			return new Vector3d(_vertices[i], _vertices[i + 1], _vertices[i + 2]);
		}

		public void SetVertex(int vID, Vector3d vNewPos) {
			Debug.Assert(vNewPos.IsFinite);     // this will really catch a lot of bugs...
			if (vertices_refcount.IsValid(vID)) {
				var i = 3 * vID;
				_vertices[i] = vNewPos.x;
				_vertices[i + 1] = vNewPos.y;
				_vertices[i + 2] = vNewPos.z;
				UpdateTimeStamp(true);
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


		public bool GetEdgeV(int eID, ref Vector3d a, ref Vector3d b) {
			if (edges_refcount.IsValid(eID)) {
				var iv0 = 3 * edges[3 * eID];
				a.x = _vertices[iv0];
				a.y = _vertices[iv0 + 1];
				a.z = _vertices[iv0 + 2];
				var iv1 = 3 * edges[(3 * eID) + 1];
				b.x = _vertices[iv1];
				b.y = _vertices[iv1 + 1];
				b.z = _vertices[iv1 + 2];
				return true;
			}
			return false;
		}

		public Segment3d GetEdgeSegment(int eID) {
			if (edges_refcount.IsValid(eID)) {
				var iv0 = 3 * edges[3 * eID];
				var iv1 = 3 * edges[(3 * eID) + 1];
				return new Segment3d(new Vector3d(_vertices[iv0], _vertices[iv0 + 1], _vertices[iv0 + 2]),
									 new Vector3d(_vertices[iv1], _vertices[iv1 + 1], _vertices[iv1 + 2]));
			}
			throw new Exception("DGraph3.GetEdgeSegment: invalid segment with id " + eID);
		}

		public Vector3d GetEdgeCenter(int eID) {
			if (edges_refcount.IsValid(eID)) {
				var iv0 = 3 * edges[3 * eID];
				var iv1 = 3 * edges[(3 * eID) + 1];
				return new Vector3d((_vertices[iv0] + _vertices[iv1]) * 0.5,
									(_vertices[iv0 + 1] + _vertices[iv1 + 1]) * 0.5,
									(_vertices[iv0 + 2] + _vertices[iv1 + 2]) * 0.5);
			}
			throw new Exception("DGraph3.GetEdgeCenter: invalid segment with id " + eID);
		}


		public IEnumerable<Segment3d> Segments() {
			foreach (int eid in edges_refcount) {
				yield return GetEdgeSegment(eid);
			}
		}




		public int AppendVertex(Vector3d v) {
			return AppendVertex(v, Vector3f.One);
		}
		public int AppendVertex(Vector3d v, Vector3f c) {
			var vid = Append_vertex_internal();
			var i = 3 * vid;
			_vertices.Insert(v[2], i + 2);
			_vertices.Insert(v[1], i + 1);
			_vertices.Insert(v[0], i);

			if (_colors != null) {
				_colors.Insert(c.z, i + 2);
				_colors.Insert(c.y, i + 1);
				_colors.Insert(c.x, i);
			}
			return vid;
		}




		public void AppendGraph(DGraph3 graph, int gid = -1) {
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





		// iterators


		public IEnumerable<Vector3d> Vertices() {
			foreach (int vid in vertices_refcount) {
				var i = 3 * vid;
				yield return new Vector3d(_vertices[i], _vertices[i + 1], _vertices[i + 2]);
			}
		}






		// compute vertex bounding box
		public AxisAlignedBox3d GetBounds() {
			double x = 0, y = 0, z = 0;
			foreach (int vi in vertices_refcount) {
				x = _vertices[3 * vi];
				y = _vertices[(3 * vi) + 1];
				z = _vertices[(3 * vi) + 2];
				break;
			}
			double minx = x, maxx = x, miny = y, maxy = y, minz = z, maxz = z;
			foreach (int vi in vertices_refcount) {
				var i = 3 * vi;
				x = _vertices[i];
				y = _vertices[i + 1];
				z = _vertices[i + 2];
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







		// internal used in SplitEdge
		protected virtual int Append_new_split_vertex(int a, int b) {
			var vNew = 0.5 * (GetVertex(a) + GetVertex(b));
			var cNew = HasVertexColors ? (0.5f * (GetVertexColor(a) + GetVertexColor(b))) : Vector3f.One;
			var f = AppendVertex(vNew, cNew);
			return f;
		}



		protected override void Subclass_validity_checks(Action<bool> CheckOrFailF) {
			foreach (var vID in VertexIndices()) {
				var v = GetVertex(vID);
				CheckOrFailF(double.IsNaN(v.LengthSquared) == false);
				CheckOrFailF(double.IsInfinity(v.LengthSquared) == false);
			}
		}



	}
}
