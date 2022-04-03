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
	public class DCurve3 : ISampledCurve3d
	{
		// [TODO] use dvector? or double-indirection indexing?
		//   question is how to insert efficiently...
		protected List<Vector3d> vertices;
		public bool Closed { get; set; }
		public int Timestamp;

		public DCurve3() {
			vertices = new List<Vector3d>();
			Closed = false;
			Timestamp = 1;
		}

		public DCurve3(List<Vector3d> verticesIn, bool bClosed, bool bTakeOwnership = false) {
			vertices = bTakeOwnership ? verticesIn : new List<Vector3d>(verticesIn);
			Closed = bClosed;
			Timestamp = 1;
		}
		public DCurve3(IEnumerable<Vector3d> verticesIn, bool bClosed) {
			vertices = new List<Vector3d>(verticesIn);
			Closed = bClosed;
			Timestamp = 1;
		}

		public DCurve3(DCurve3 copy) {
			vertices = new List<Vector3d>(copy.vertices);
			Closed = copy.Closed;
			Timestamp = 1;
		}

		public DCurve3(ISampledCurve3d icurve) {
			vertices = new List<Vector3d>(icurve.Vertices);
			Closed = icurve.Closed;
			Timestamp = 1;
		}

		public DCurve3(Polygon2d poly, int ix = 0, int iy = 1) {
			var NV = poly.VertexCount;
			vertices = new List<Vector3d>(NV);
			for (var k = 0; k < NV; ++k) {
				var v = Vector3d.Zero;
				v[ix] = poly[k].x;
				v[iy] = poly[k].y;
				vertices.Add(v);
			}
			Closed = true;
			Timestamp = 1;
		}

		public void AppendVertex(Vector3d v) {
			vertices.Add(v);
			Timestamp++;
		}

		public int VertexCount => vertices.Count;
		public int SegmentCount => Closed ? vertices.Count : vertices.Count - 1;

		public Vector3d GetVertex(int i) {
			return vertices[i];
		}
		public void SetVertex(int i, Vector3d v) {
			vertices[i] = v;
			Timestamp++;
		}

		public void SetVertices(VectorArray3d v) {
			vertices = new List<Vector3d>();
			for (var i = 0; i < v.Count; ++i) {
				vertices.Add(v[i]);
			}

			Timestamp++;
		}

		public void SetVertices(IEnumerable<Vector3d> v) {
			vertices = new List<Vector3d>(v);
			Timestamp++;
		}

		public void SetVertices(List<Vector3d> vertices, bool bTakeOwnership) {
			var dCurve3 = this;
			dCurve3.vertices = bTakeOwnership ? vertices : new List<Vector3d>(vertices);
			Timestamp++;
		}

		public void ClearVertices() {
			vertices = new List<Vector3d>();
			Closed = false;
			Timestamp++;
		}

		public void RemoveVertex(int idx) {
			vertices.RemoveAt(idx);
			Timestamp++;
		}

		public void Reverse() {
			vertices.Reverse();
			Timestamp++;
		}


		public Vector3d this[int key]
		{
			get => vertices[key];
			set { vertices[key] = value; Timestamp++; }
		}

		public Vector3d Start => vertices[0];
		public Vector3d End => Closed ? vertices[0] : vertices.Last();

		public IEnumerable<Vector3d> Vertices => vertices;


		public Segment3d GetSegment(int iSegment) {
			return Closed ? new Segment3d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count])
				: new Segment3d(vertices[iSegment], vertices[iSegment + 1]);
		}

		public IEnumerable<Segment3d> SegmentItr() {
			if (Closed) {
				var NV = vertices.Count;
				for (var i = 0; i < NV; ++i) {
					yield return new Segment3d(vertices[i], vertices[(i + 1) % NV]);
				}
			}
			else {
				var NV = vertices.Count - 1;
				for (var i = 0; i < NV; ++i) {
					yield return new Segment3d(vertices[i], vertices[i + 1]);
				}
			}
		}

		public Vector3d PointAt(int iSegment, double fSegT) {
			var seg = new Segment3d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]);
			return seg.PointAt(fSegT);
		}


		public AxisAlignedBox3d GetBoundingBox() {
			var box = AxisAlignedBox3d.Empty;
			foreach (var v in vertices) {
				box.Contain(v);
			}

			return box;
		}

		public double ArcLength => CurveUtils.ArcLength(vertices, Closed);

		public Vector3d Tangent(int i) {
			return CurveUtils.GetTangent(vertices, i, Closed);
		}

		public Vector3d Centroid(int i) {
			if (Closed) {
				var NV = vertices.Count;
				return i == 0 ? 0.5 * (vertices[1] + vertices[NV - 1]) : 0.5 * (vertices[(i + 1) % NV] + vertices[i - 1]);
			}
			else {
				return i == 0 || i == vertices.Count - 1 ? vertices[i] : 0.5 * (vertices[i + 1] + vertices[i - 1]);
			}
		}


		public Index2i Neighbours(int i) {
			var NV = vertices.Count;
			return Closed
				? i == 0 ? new Index2i(NV - 1, 1) : new Index2i(i - 1, (i + 1) % NV)
				: i == 0 ? new Index2i(-1, 1) : i == NV - 1 ? new Index2i(NV - 2, -1) : new Index2i(i - 1, i + 1);
		}


		/// <summary>
		/// Compute opening angle at vertex i in degrees
		/// </summary>
		public double OpeningAngleDeg(int i) {
			int prev = i - 1, next = i + 1;
			if (Closed) {
				var NV = vertices.Count;
				prev = (i == 0) ? NV - 1 : prev;
				next %= NV;
			}
			else {
				if (i == 0 || i == vertices.Count - 1) {
					return 180;
				}
			}
			var e1 = vertices[prev] - vertices[i];
			var e2 = vertices[next] - vertices[i];
			e1.Normalize();
			e2.Normalize();
			return Vector3d.AngleD(e1, e2);
		}


		/// <summary>
		/// Find nearest vertex to point p
		/// </summary>
		public int NearestVertex(Vector3d p) {
			var nearSqr = double.MaxValue;
			var i = -1;
			var N = vertices.Count;
			for (var vi = 0; vi < N; ++vi) {
				var distSqr = vertices[vi].DistanceSquared(ref p);
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
		public double DistanceSquared(Vector3d p, out int iNearSeg, out double fNearSegT) {
			iNearSeg = -1;
			fNearSegT = double.MaxValue;
			var dist = double.MaxValue;
			var N = Closed ? vertices.Count : vertices.Count - 1;
			for (var vi = 0; vi < N; ++vi) {
				var a = vi;
				var b = (vi + 1) % vertices.Count;
				var seg = new Segment3d(vertices[a], vertices[b]);
				var t = (p - seg.Center).Dot(seg.Direction);
				var d = t >= seg.Extent
					? seg.P1.DistanceSquared(p)
					: t <= -seg.Extent ? seg.P0.DistanceSquared(p) : (seg.PointAt(t) - p).LengthSquared;

				if (d < dist) {
					dist = d;
					iNearSeg = vi;
					fNearSegT = t;
				}
			}
			return dist;
		}
		public double DistanceSquared(Vector3d p) {
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
		public DCurve3 ResampleSharpTurns(double sharp_thresh = 90, double flat_thresh = 189, double corner_t = 0.01) {
			var NV = vertices.Count;
			var resampled = new DCurve3() { Closed = Closed };
			var prev_t = 1.0 - corner_t;
			for (var k = 0; k < NV; ++k) {
				var open_angle = Math.Abs(OpeningAngleDeg(k));
				if (open_angle > flat_thresh && k > 0) {
					// ignore skip this vertex
				}
				else if (open_angle > sharp_thresh) {
					resampled.AppendVertex(vertices[k]);
				}
				else {
					var n = vertices[(k + 1) % NV];
					var p = vertices[k == 0 ? NV - 1 : k - 1];
					resampled.AppendVertex(Vector3d.Lerp(p, vertices[k], prev_t));
					resampled.AppendVertex(vertices[k]);
					resampled.AppendVertex(Vector3d.Lerp(vertices[k], n, corner_t));
				}
			}
			return resampled;
		}

	}
}
