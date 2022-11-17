using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// DCurve3 is a 3D polyline, either open or closed (via .Closed)
	/// Despite the D prefix, it is *not* dynamic
	/// </summary>
	public sealed class DCurve3 : ISampledCurve3d
	{
		// [TODO] use dvector? or double-indirection indexing?
		//   question is how to insert efficiently...
		private List<Vector3d> _vertices;
		public bool Closed { get; set; }
		public int Timestamp;

		public DCurve3() {
			_vertices = new List<Vector3d>();
			Closed = false;
			Timestamp = 1;
		}

		public DCurve3(in List<Vector3d> verticesIn, in bool bClosed, in bool bTakeOwnership = false) {
			_vertices = bTakeOwnership ? verticesIn : new List<Vector3d>(verticesIn);
			Closed = bClosed;
			Timestamp = 1;
		}
		public DCurve3(in IEnumerable<Vector3d> verticesIn, in bool bClosed) {
			_vertices = new List<Vector3d>(verticesIn);
			Closed = bClosed;
			Timestamp = 1;
		}

		public DCurve3(in DCurve3 copy) {
			_vertices = new List<Vector3d>(copy._vertices);
			Closed = copy.Closed;
			Timestamp = 1;
		}

		public DCurve3(in ISampledCurve3d icurve) {
			_vertices = new List<Vector3d>(icurve.Vertices);
			Closed = icurve.Closed;
			Timestamp = 1;
		}

		public DCurve3(in Polygon2d poly, in int ix = 0, in int iy = 1) {
			var NV = poly.VertexCount;
			_vertices = new List<Vector3d>(NV);
			for (var k = 0; k < NV; ++k) {
				var v = Vector3d.Zero;
				v[ix] = poly[k].x;
				v[iy] = poly[k].y;
				_vertices.Add(v);
			}
			Closed = true;
			Timestamp = 1;
		}

		public void AppendVertex(in Vector3d v) {
			_vertices.Add(v);
			Timestamp++;
		}

		public int VertexCount => _vertices.Count;
		public int SegmentCount => Closed ? _vertices.Count : _vertices.Count - 1;

		public Vector3d GetVertex(in int i) {
			return _vertices[i];
		}
		public void SetVertex(in int i, in Vector3d v) {
			_vertices[i] = v;
			Timestamp++;
		}

		public void SetVertices(in VectorArray3d v) {
			_vertices = new List<Vector3d>();
			for (var i = 0; i < v.Count; ++i) {
				_vertices.Add(v[i]);
			}

			Timestamp++;
		}

		public void SetVertices(in IEnumerable<Vector3d> v) {
			_vertices = new List<Vector3d>(v);
			Timestamp++;
		}

		public void SetVertices(in List<Vector3d> vertices, in bool bTakeOwnership) {
			var dCurve3 = this;
			dCurve3._vertices = bTakeOwnership ? vertices : new List<Vector3d>(vertices);
			Timestamp++;
		}

		public void ClearVertices() {
			_vertices = new List<Vector3d>();
			Closed = false;
			Timestamp++;
		}

		public void RemoveVertex(in int idx) {
			_vertices.RemoveAt(idx);
			Timestamp++;
		}

		public void Reverse() {
			_vertices.Reverse();
			Timestamp++;
		}


		public Vector3d this[in int key]
		{
			get => _vertices[key];
			set { _vertices[key] = value; Timestamp++; }
		}

		public Vector3d Start => _vertices[0];
		public Vector3d End => Closed ? _vertices[0] : _vertices.Last();

		public IEnumerable<Vector3d> Vertices => _vertices;


		public Segment3d GetSegment(in int iSegment) {
			return Closed ? new Segment3d(_vertices[iSegment], _vertices[(iSegment + 1) % _vertices.Count])
				: new Segment3d(_vertices[iSegment], _vertices[iSegment + 1]);
		}

		public IEnumerable<Segment3d> SegmentItr() {
			if (Closed) {
				var NV = _vertices.Count;
				for (var i = 0; i < NV; ++i) {
					yield return new Segment3d(_vertices[i], _vertices[(i + 1) % NV]);
				}
			}
			else {
				var NV = _vertices.Count - 1;
				for (var i = 0; i < NV; ++i) {
					yield return new Segment3d(_vertices[i], _vertices[i + 1]);
				}
			}
		}

		public Vector3d PointAt(in int iSegment, in double fSegT) {
			var seg = new Segment3d(_vertices[iSegment], _vertices[(iSegment + 1) % _vertices.Count]);
			return seg.PointAt(fSegT);
		}


		public AxisAlignedBox3d GetBoundingBox() {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in _vertices) {
				box.Contain(v);
			}

			return box;
		}

		public double ArcLength => CurveUtils.ArcLength(_vertices, Closed);

		public Vector3d Tangent(in int i) {
			return CurveUtils.GetTangent(_vertices, i, Closed);
		}

		public Vector3d Centroid(in int i) {
			if (Closed) {
				var NV = _vertices.Count;
				return i == 0 ? 0.5 * (_vertices[1] + _vertices[NV - 1]) : 0.5 * (_vertices[(i + 1) % NV] + _vertices[i - 1]);
			}
			else {
				return i == 0 || i == _vertices.Count - 1 ? _vertices[i] : 0.5 * (_vertices[i + 1] + _vertices[i - 1]);
			}
		}


		public Index2i Neighbours(in int i) {
			var NV = _vertices.Count;
			return Closed
				? i == 0 ? new Index2i(NV - 1, 1) : new Index2i(i - 1, (i + 1) % NV)
				: i == 0 ? new Index2i(-1, 1) : i == NV - 1 ? new Index2i(NV - 2, -1) : new Index2i(i - 1, i + 1);
		}


		/// <summary>
		/// Compute opening angle at vertex i in degrees
		/// </summary>
		public double OpeningAngleDeg(in int i) {
			int prev = i - 1, next = i + 1;
			if (Closed) {
				var NV = _vertices.Count;
				prev = (i == 0) ? NV - 1 : prev;
				next %= NV;
			}
			else {
				if (i == 0 || i == _vertices.Count - 1) {
					return 180;
				}
			}
			var e1 = _vertices[prev] - _vertices[i];
			var e2 = _vertices[next] - _vertices[i];
			e1.Normalize();
			e2.Normalize();
			return Vector3d.AngleD(e1, e2);
		}


		/// <summary>
		/// Find nearest vertex to point p
		/// </summary>
		public int NearestVertex(in Vector3d p) {
			var nearSqr = double.MaxValue;
			var i = -1;
			var N = _vertices.Count;
			for (var vi = 0; vi < N; ++vi) {
				var distSqr = _vertices[vi].DistanceSquared(p);
				if (distSqr < nearSqr) {
					nearSqr = distSqr;
					i = vi;
				}
			}
			return i;
		}


		/// <summary>
		/// find squared distance from p to nearest segment on polyline
		/// </summary>
		public double DistanceSquared(in Vector3d p, out int iNearSeg, out double fNearSegT) {
			iNearSeg = -1;
			fNearSegT = double.MaxValue;
			var dist = double.MaxValue;
			var N = Closed ? _vertices.Count : _vertices.Count - 1;
			for (var vi = 0; vi < N; ++vi) {
				var a = vi;
				var b = (vi + 1) % _vertices.Count;
				var seg = new Segment3d(_vertices[a], _vertices[b]);
				var t = (p - seg.center).Dot(seg.direction);
				var d = t >= seg.extent
					? seg.P1.DistanceSquared(p)
					: t <= -seg.extent ? seg.P0.DistanceSquared(p) : (seg.PointAt(t) - p).LengthSquared;

				if (d < dist) {
					dist = d;
					iNearSeg = vi;
					fNearSegT = t;
				}
			}
			return dist;
		}
		public double DistanceSquared(in Vector3d p) {
			return DistanceSquared(p, out var iseg, out var segt);
		}



		/// <summary>
		/// Resample curve so that:
		///   - if opening angle at vertex is > sharp_thresh, we emit two more vertices at +/- corner_t, where the t is used in prev/next lerps
		///   - if opening angle is > flat_thresh, we skip the vertex entirely (simplification)
		/// This is mainly useful to get nicer polylines to use as the basis for (eg) creating 3D tubes, rendering, etc
		/// 
		/// [TODO] skip tiny segments?
		/// </summary>
		public DCurve3 ResampleSharpTurns(in double sharp_thresh = 90, in double flat_thresh = 189, in double corner_t = 0.01) {
			var NV = _vertices.Count;
			var resampled = new DCurve3() { Closed = Closed };
			var prev_t = 1.0 - corner_t;
			for (var k = 0; k < NV; ++k) {
				var open_angle = Math.Abs(OpeningAngleDeg(k));
				if (open_angle > flat_thresh && k > 0) {
					// ignore skip this vertex
				}
				else if (open_angle > sharp_thresh) {
					resampled.AppendVertex(_vertices[k]);
				}
				else {
					var n = _vertices[(k + 1) % NV];
					var p = _vertices[k == 0 ? NV - 1 : k - 1];
					resampled.AppendVertex(Vector3d.Lerp(p, _vertices[k], prev_t));
					resampled.AppendVertex(_vertices[k]);
					resampled.AppendVertex(Vector3d.Lerp(_vertices[k], n, corner_t));
				}
			}
			return resampled;
		}

	}
}
