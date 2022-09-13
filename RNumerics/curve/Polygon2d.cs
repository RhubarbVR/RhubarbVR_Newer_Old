using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public class Polygon2d : IDuplicatable<Polygon2d>
	{
		protected List<Vector2d> vertices;
		public int Timestamp;

		public Polygon2d() {
			vertices = new List<Vector2d>();
			Timestamp = 0;
		}

		public Polygon2d(in Polygon2d copy) {
			vertices = new List<Vector2d>(copy.vertices);
			Timestamp = 0;
		}

		public Polygon2d(in IList<Vector2d> copy) {
			vertices = new List<Vector2d>(copy);
			Timestamp = 0;
		}

		public Polygon2d(in IEnumerable<Vector2d> copy) {
			vertices = new List<Vector2d>(copy);
			Timestamp = 0;
		}

		public Polygon2d(in Vector2d[] v) {
			vertices = new List<Vector2d>(v);
			Timestamp = 0;
		}
		public Polygon2d(in VectorArray2d v) {
			vertices = new List<Vector2d>(v.AsVector2d());
			Timestamp = 0;
		}


		public Polygon2d(in double[] values) {
			var N = values.Length / 2;
			vertices = new List<Vector2d>(N);
			for (var k = 0; k < N; ++k) {
				vertices.Add(new Vector2d(values[2 * k], values[(2 * k) + 1]));
			}

			Timestamp = 0;
		}

		public Polygon2d(in Func<int, Vector2d> SourceF, in int N) {
			vertices = new List<Vector2d>();
			for (var k = 0; k < N; ++k) {
				vertices.Add(SourceF(k));
			}

			Timestamp = 0;
		}

		public  Polygon2d Duplicate() {
			var p = new Polygon2d(this) {
				Timestamp = Timestamp
			};
			return p;
		}


		public Vector2d this[in int key]
		{
			get => vertices[key];
			set { vertices[key] = value; Timestamp++; }
		}

		public Vector2d Start => vertices[0];

		public ReadOnlyCollection<Vector2d> Vertices => vertices.AsReadOnly();

		public int VertexCount => vertices.Count;

		public void AppendVertex(in Vector2d v) {
			vertices.Add(v);
			Timestamp++;
		}
		public void AppendVertices(in IEnumerable<Vector2d> v) {
			vertices.AddRange(v);
			Timestamp++;
		}

		public void RemoveVertex(in int idx) {
			vertices.RemoveAt(idx);
			Timestamp++;
		}


		public void SetVertices(in List<Vector2d> newVertices, in bool bTakeOwnership) {
			if (bTakeOwnership) {
				vertices = newVertices;
			}
			else {
				vertices.Clear();
				var N = newVertices.Count;
				for (var i = 0; i < N; ++i) {
					vertices.Add(newVertices[i]);
				}
			}
		}


		public void Reverse() {
			vertices.Reverse();
			Timestamp++;
		}


		public Vector2d GetTangent(in int i) {
			var next = vertices[(i + 1) % vertices.Count];
			var prev = vertices[i == 0 ? vertices.Count - 1 : i - 1];
			return (next - prev).Normalized;
		}

		/// <summary>
		/// Normal at vertex i, which is perp to tangent direction, which is not so 
		/// intuitive if edges have very different lengths. 
		/// Points "inward" for clockwise polygon, outward for counter-clockwise
		/// </summary>
		public Vector2d GetNormal(in int i) {
			return GetTangent(i).Perp;
		}

		/// <summary>
		/// Construct normal at poly vertex by averaging face normals. This is
		/// equivalent (?) to angle-based normal, ie is local/independent of segment lengths.
		/// Points "inward" for clockwise polygon, outward for counter-clockwise
		/// </summary>
		public Vector2d GetNormal_FaceAvg(in int i) {
			var next = vertices[(i + 1) % vertices.Count];
			var prev = vertices[i == 0 ? vertices.Count - 1 : i - 1];
			next -= vertices[i];
			next.Normalize();
			prev -= vertices[i];
			prev.Normalize();

			var n = next.Perp - prev.Perp;
			var len = n.Normalize();
			if (len == 0) {
				return (next + prev).Normalized;   // this gives right direction for degenerate angle
			}
			else {
				return n;
			}
		}

		public AxisAlignedBox2d GetBounds() {
			var box = AxisAlignedBox2d.Empty;
			box.Contain(vertices);
			return box;
		}
		public AxisAlignedBox2d Bounds => GetBounds();



		public IEnumerable<Segment2d> SegmentItr() {
			for (var i = 0; i < vertices.Count; ++i) {
				yield return new Segment2d(vertices[i], vertices[(i + 1) % vertices.Count]);
			}
		}

		public IEnumerable<Vector2d> VerticesItr(bool bRepeatFirstAtEnd) {
			var N = vertices.Count;
			for (var i = 0; i < N; ++i) {
				yield return vertices[i];
			}

			if (bRepeatFirstAtEnd) {
				yield return vertices[0];
			}
		}

		// [RMS] have removed IEnumerable interface because these are ambiguous - should
		//  first vertex be repeated? Has caused too many bugs!
		//public IEnumerator<Vector2d> GetEnumerator() {
		//	for ( int i = 0; i < vertices.Count; ++i )
		//		yield return vertices[i];
		//	yield return vertices[0];
		//}
		//IEnumerator IEnumerable.GetEnumerator() {
		//	for ( int i = 0; i < vertices.Count; ++i )
		//		yield return vertices[i];
		//	yield return vertices[0];
		//}


		public bool IsClockwise => SignedArea < 0;
		public double SignedArea
		{
			get {
				double fArea = 0;
				var N = vertices.Count;
				if (N == 0) {
					return 0;
				}

				var v1 = vertices[0];
				for (var i = 0; i < N; ++i) {
					var v2 = vertices[(i + 1) % N];
					fArea += (v1.x * v2.y) - (v1.y * v2.x);
					v1 = v2;
				}
				return fArea * 0.5;
			}
		}
		public double Area => Math.Abs(SignedArea);



		public double Perimeter
		{
			get {
				double fPerim = 0;
				var N = vertices.Count;
				for (var i = 0; i < N; ++i) {
					fPerim += vertices[i].Distance(vertices[(i + 1) % N]);
				}

				return fPerim;
			}
		}
		public double ArcLength => Perimeter;



		public void NeighboursP(in int iVertex, ref Vector2d p0, ref Vector2d p1) {
			var N = vertices.Count;
			p0 = vertices[(iVertex == 0) ? N - 1 : iVertex - 1];
			p1 = vertices[(iVertex + 1) % N];
		}
		public void NeighboursV(in int iVertex, ref Vector2d v0, ref Vector2d v1, in bool bNormalize = false) {
			var N = vertices.Count;
			v0 = vertices[(iVertex == 0) ? N - 1 : iVertex - 1] - vertices[iVertex];
			v1 = vertices[(iVertex + 1) % N] - vertices[iVertex];
			if (bNormalize) {
				v0.Normalize();
				v1.Normalize();
			}
		}

		public double OpeningAngleDeg(in int iVertex) {
			Vector2d e0 = Vector2d.Zero, e1 = Vector2d.Zero;
			NeighboursV(iVertex, ref e0, ref e1, true);
			return Vector2d.AngleD(e0, e1);
		}


		/// <summary>
		/// Compute winding integral at point P
		/// </summary>
		public double WindingIntegral(in Vector2d P) {
			double sum = 0;
			var N = vertices.Count;
			var a = vertices[0] - P;
			for (var i = 0; i < N; ++i) {
				var b = vertices[(i + 1) % N] - P;
				sum += Math.Atan2((a.x * b.y) - (a.y * b.x), (a.x * b.x) + (a.y * b.y));
				a = b;
			}
			return sum / MathUtil.TWO_PI;
		}



		/// <summary>
		/// Returns true if point inside polygon, using fast winding-number computation
		/// </summary>
		public bool Contains(in Vector2d P) {
			// based on http://geomalgorithms.com/a03-_inclusion.html	
			var nWindingNumber = 0;

			var N = vertices.Count;
			Vector2d a = vertices[0], b = Vector2d.Zero;
			for (var i = 0; i < N; ++i) {
				b = vertices[(i + 1) % N];

				if (a.y <= P.y) {   // y <= P.y (below)
					if (b.y > P.y) {                         // an upward crossing
						if (MathUtil.IsLeft(in a, in b, in P) > 0)  // P left of edge
{
							++nWindingNumber;                                      // have a valid up intersect
						}
					}
				}
				else {    // y > P.y  (above)
					if (b.y <= P.y) {                        // a downward crossing
						if (MathUtil.IsLeft( a,  b,  P) < 0)  // P right of edge
{
							--nWindingNumber;                                      // have a valid down intersect
						}
					}
				}
				a = b;
			}
			return nWindingNumber != 0;
		}



		public bool Contains(in Polygon2d o) {

			// [TODO] fast bbox check?

			var N = o.VertexCount;
			for (var i = 0; i < N; ++i) {
				if (Contains(o[i]) == false) {
					return false;
				}
			}

			return !Intersects(o);
		}

		/// <summary>
		/// Checks that all points on a segment are within the area defined by the Polygon2d.
		/// </summary>
		public bool Contains(in Segment2d o) {
			// [TODO] Add bbox check
			if (Contains(o.P0) == false || Contains(o.P1) == false) {
				return false;
			}

			foreach (var seg in SegmentItr()) {
				if (seg.Intersects(o)) {
					return false;
				}
			}
			return true;
		}

		public bool Intersects(in Polygon2d o) {
			if (!GetBounds().Intersects(o.GetBounds())) {
				return false;
			}

			foreach (var seg in SegmentItr()) {
				foreach (var oseg in o.SegmentItr()) {
					if (seg.Intersects(oseg)) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if any point on a segment is within the area defined by the Polygon2d.
		/// </summary>
		public bool Intersects(in Segment2d o) {
			// [TODO] Add bbox check
			if (Contains(o.P0) || Contains(o.P1) == true) {
				return true;
			}

			// [TODO] Add bbox check
			foreach (var seg in SegmentItr()) {
				if (seg.Intersects(o)) {
					return true;
				}
			}
			return false;
		}



		public List<Vector2d> FindIntersections(in Polygon2d o) {
			var v = new List<Vector2d>();
			if (!GetBounds().Intersects(o.GetBounds())) {
				return v;
			}

			foreach (var seg in SegmentItr()) {
				foreach (var oseg in o.SegmentItr()) {
					// this computes test twice for intersections, but seg.intersects doesn't
					// create any new objects so it should be much faster for majority of segments (should profile!)
					if (seg.Intersects(oseg)) {
						var intr = new IntrSegment2Segment2(seg, oseg);
						if (intr.Find()) {
							v.Add(intr.Point0);
							if (intr.Quantity == 2) {
								v.Add(intr.Point1);
							}
						}
					}
				}
			}
			return v;
		}


		public Segment2d Segment(in int iSegment) {
			return new Segment2d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]);
		}

		public Vector2d PointAt(in int iSegment, in double fSegT) {
			var seg = new Segment2d(vertices[iSegment], vertices[(iSegment + 1) % vertices.Count]);
			return seg.PointAt(fSegT);
		}

		public Vector2d GetNormal(in int iSeg, in double segT) {
			var seg = new Segment2d(vertices[iSeg], vertices[(iSeg + 1) % vertices.Count]);
			var t = ((segT / seg.Extent) + 1.0) / 2.0;

			var n0 = GetNormal(iSeg);
			var n1 = GetNormal((iSeg + 1) % vertices.Count);
			return (((1.0 - t) * n0) + (t * n1)).Normalized;
		}



		public double DistanceSquared(in Vector2d p, out int iNearSeg, out double fNearSegT) {
			iNearSeg = -1;
			fNearSegT = double.MaxValue;
			var dist = double.MaxValue;
			var N = vertices.Count;
			for (var vi = 0; vi < N; ++vi) {
				var seg = new Segment2d(vertices[vi], vertices[(vi + 1) % N]);
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
		public double DistanceSquared(in Vector2d p) {
			return DistanceSquared(p, out var seg, out var segt);
		}


		public double AverageEdgeLength
		{
			get {
				double avg = 0;
				var N = vertices.Count;
				for (var i = 1; i < N; ++i) {
					avg += vertices[i].Distance(vertices[i - 1]);
				}

				avg += vertices[N - 1].Distance(vertices[0]);
				return avg / N;
			}
		}


		public Polygon2d Translate(in Vector2d translate) {
			var N = vertices.Count;
			for (var i = 0; i < N; ++i) {
				vertices[i] += translate;
			}

			Timestamp++;
			return this;
		}

		public Polygon2d Rotate(in Matrix2d rotation, in Vector2d origin) {
			var N = vertices.Count;
			for (var i = 0; i < N; ++i) {
				vertices[i] = (rotation * (vertices[i] - origin)) + origin;
			}

			Timestamp++;
			return this;
		}

		public Polygon2d Scale(in Vector2d scale, in Vector2d origin) {
			var N = vertices.Count;
			for (var i = 0; i < N; ++i) {
				vertices[i] = (scale * (vertices[i] - origin)) + origin;
			}

			Timestamp++;
			return this;
		}

		public Polygon2d Transform(in Func<Vector2d, Vector2d> transformF) {
			var N = vertices.Count;
			for (var i = 0; i < N; ++i) {
				vertices[i] = transformF(vertices[i]);
			}

			Timestamp++;
			return this;
		}

		public Polygon2d Transform(in ITransform2 xform) {
			var N = vertices.Count;
			for (var k = 0; k < N; ++k) {
				vertices[k] = xform.TransformP(vertices[k]);
			}

			Timestamp++;
			return this;
		}


		/// <summary>
		/// Offset each point by dist along vertex normal direction (ie tangent-perp)
		/// CCW polygon offsets "outwards", CW "inwards".
		/// </summary>
		public void VtxNormalOffset(in double dist, in bool bUseFaceAvg = false) {
			var newv = new Vector2d[vertices.Count];
			if (bUseFaceAvg) {
				for (var k = 0; k < vertices.Count; ++k) {
					newv[k] = vertices[k] + (dist * GetNormal_FaceAvg(k));
				}
			}
			else {
				for (var k = 0; k < vertices.Count; ++k) {
					newv[k] = vertices[k] + (dist * GetNormal(k));
				}
			}
			for (var k = 0; k < vertices.Count; ++k) {
				vertices[k] = newv[k];
			}

			Timestamp++;
		}


		/// <summary>
		/// offset polygon by fixed distance, by offsetting and intersecting edges.
		/// CCW polygon offsets "outwards", CW "inwards".
		/// </summary>
		public void PolyOffset(in double dist) {
			// [TODO] possibly can do with half as many normalizes if we do w/ sequential edges,
			//  rather than centering on each v?
			var newv = new Vector2d[vertices.Count];
			for (var k = 0; k < vertices.Count; ++k) {
				var v = vertices[k];
				var next = vertices[(k + 1) % vertices.Count];
				var prev = vertices[k == 0 ? vertices.Count - 1 : k - 1];
				var dn = (next - v).Normalized;
				var dp = (prev - v).Normalized;
				var ln = new Line2d(v + (dist * dn.Perp), dn);
				var lp = new Line2d(v - (dist * dp.Perp), dp);

				newv[k] = ln.IntersectionPoint(lp);
				if (newv[k] == Vector2d.MaxValue) {
					newv[k] = vertices[k] + (dist * GetNormal_FaceAvg(k));
				}
			}
			for (var k = 0; k < vertices.Count; ++k) {
				vertices[k] = newv[k];
			}

			Timestamp++;
		}



		// Polygon simplification
		// code adapted from: http://softsurfer.com/Archive/algorithm_0205/algorithm_0205.htm
		// simplifyDP():
		//  This is the Douglas-Peucker recursive simplification routine
		//  It just marks vertices that are part of the simplified polyline
		//  for approximating the polyline subchain v[j] to v[k].
		//    Input:  tol = approximation tolerance
		//            v[] = polyline array of vertex points
		//            j,k = indices for the subchain v[j] to v[k]
		//    Output: mk[] = array of markers matching vertex array v[]
		static void SimplifyDP(in double tol, in Vector2d[] v, in int j, in int k, in bool[] mk) {
			if (k <= j + 1) // there is nothing to simplify
{
				return;
			}

			// check for adequate approximation by segment S from v[j] to v[k]
			var maxi = j;          // index of vertex farthest from S
			double maxd2 = 0;         // distance squared of farthest vertex
			var tol2 = tol * tol;  // tolerance squared
			var S = new Segment2d(v[j], v[k]);    // segment from v[j] to v[k]

			// test each vertex v[i] for max distance from S
			// Note: this works in any dimension (2D, 3D, ...)
			for (var i = j + 1; i < k; i++) {
				var dv2 = S.DistanceSquared(v[i]);
				if (dv2 <= maxd2) {
					continue;
				}
				// v[i] is a new max vertex
				maxi = i;
				maxd2 = dv2;
			}
			if (maxd2 > tol2) {       // error is worse than the tolerance
									  // split the polyline at the farthest vertex from S
				mk[maxi] = true;      // mark v[maxi] for the simplified polyline
									  // recursively simplify the two subpolylines at v[maxi]
				SimplifyDP(tol, v, j, maxi, mk);  // polyline v[j] to v[maxi]
				SimplifyDP(tol, v, maxi, k, mk);  // polyline v[maxi] to v[k]
			}
			// else the approximation is OK, so ignore intermediate vertices
			return;
		}



		public void Simplify(in double clusterTol = 0.0001,
							  in double lineDeviationTol = 0.01) {
			var n = vertices.Count;
			if (n < 3) {
				return;
			}

			int i, k, pv;            // misc counters
			var vt = new Vector2d[n + 1];  // vertex buffer
			var mk = new bool[n + 1];
			for (i = 0; i < n + 1; ++i)     // marker buffer
{
				mk[i] = false;
			}

			// STAGE 1.  Vertex Reduction within tolerance of prior vertex cluster
			var clusterTol2 = clusterTol * clusterTol;
			vt[0] = vertices[0];              // start at the beginning
			for (i = 1, k = 1, pv = 0; i < n; i++) {
				if ((vertices[i] - vertices[pv]).LengthSquared < clusterTol2) {
					continue;
				}

				vt[k++] = vertices[i];
				pv = i;
			}
			var skip_dp = false;
			if (k == 1) {
				vt[k++] = vertices[1];
				vt[k++] = vertices[2];
				skip_dp = true;
			}
			else if (k == 2) {
				vt[k++] = vertices[0];
				skip_dp = true;
			}

			// push on start vertex again, because simplifyDP is for polylines, not polygons
			vt[k++] = vertices[0];

			// STAGE 2.  Douglas-Peucker polyline simplification
			var nv = 0;
			if (skip_dp == false && lineDeviationTol > 0) {
				mk[0] = mk[k - 1] = true;       // mark the first and last vertices
				SimplifyDP(lineDeviationTol, vt, 0, k - 1, mk);
				for (i = 0; i < k - 1; ++i) {
					if (mk[i]) {
						nv++;
					}
				}
			}
			else {
				for (i = 0; i < k; ++i) {
					mk[i] = true;
				}

				nv = k - 1;
			}

			// polygon requires at least 3 vertices
			if (nv == 2) {
				for (i = 1; i < k - 1; ++i) {
					if (mk[1] == false) {
						mk[1] = true;
					}
					else if (mk[k - 2] == false) {
						mk[k - 2] = true;
					}
				}
			}
			else if (nv == 1) {
				mk[1] = true;
				mk[2] = true;
			}

			// copy marked vertices back to this polygon
			vertices = new List<Vector2d>();
			for (i = 0; i < k - 1; ++i) {   // last vtx is copy of first, and definitely marked
				if (mk[i]) {
					vertices.Add(vt[i]);
				}
			}

			Timestamp++;
			return;
		}




		public void Chamfer(in double chamfer_dist, in double minConvexAngleDeg = 30, in double minConcaveAngleDeg = 30) {
			if (IsClockwise) {
				throw new Exception("must be ccw?");
			}

			var NewV = new List<Vector2d>();
			var N = Vertices.Count;

			var iCur = 0;
			do {
				var center = Vertices[iCur];

				var iPrev = (iCur == 0) ? N - 1 : iCur - 1;
				var prev = Vertices[iPrev];
				var iNext = (iCur + 1) % N;
				var next = Vertices[iNext];

				var cp = prev - center;
				var cpdist = cp.Normalize();
				var cn = next - center;
				var cndist = cn.Normalize();

				// if degenerate, skip this vert
				if (cpdist < MathUtil.ZERO_TOLERANCEF || cndist < MathUtil.ZERO_TOLERANCEF) {
					iCur = iNext;
					continue;
				}

				var angle = Vector2d.AngleD(cp, cn);
				// TODO document what this means sign-wise
				var sign = cp.Perp.Dot(cn);
				var bConcave = sign > 0;

				var thresh = bConcave ? minConcaveAngleDeg : minConvexAngleDeg;

				// ok not too sharp
				if (angle > thresh) {
					NewV.Add(center);
					iCur = iNext;
					continue;
				}


				var prev_cut_dist = Math.Min(chamfer_dist, cpdist * 0.5);
				var prev_cut = center + (prev_cut_dist * cp);
				var next_cut_dist = Math.Min(chamfer_dist, cndist * 0.5);
				var next_cut = center + (next_cut_dist * cn);

				NewV.Add(prev_cut);
				NewV.Add(next_cut);
				iCur = iNext;
			} while (iCur != 0);

			vertices = NewV;
			Timestamp++;
		}




		/// <summary>
		/// Return minimal bounding box of vertices, computed to epsilon tolerance
		/// </summary>
		public Box2d MinimalBoundingBox(in double epsilon) {
			var box2 = new ContMinBox2(vertices, epsilon, QueryNumberType.QT_DOUBLE, false);
			return box2.MinBox;
		}



		static public Polygon2d MakeRectangle(in Vector2d center, in double width, in double height) {
			var vertices = new VectorArray2d(4);
			vertices.Set(0, center.x - (width / 2), center.y - (height / 2));
			vertices.Set(1, center.x + (width / 2), center.y - (height / 2));
			vertices.Set(2, center.x + (width / 2), center.y + (height / 2));
			vertices.Set(3, center.x - (width / 2), center.y + (height / 2));
			return new Polygon2d(vertices);
		}


		static public Polygon2d MakeCircle(in double fRadius, in int nSteps, in double angleShiftRad = 0) {
			var vertices = new VectorArray2d(nSteps);

			for (var i = 0; i < nSteps; ++i) {
				var t = (double)i / (double)nSteps;
				var a = (MathUtil.TWO_PI * t) + angleShiftRad;
				vertices.Set(i, fRadius * Math.Cos(a), fRadius * Math.Sin(a));
			}

			return new Polygon2d(vertices);
		}

	}





	/// <summary>
	/// Wrapper for a Polygon2d that provides minimal IParametricCurve2D interface
	/// </summary>
	public sealed class Polygon2DCurve : IParametricCurve2d
	{
		public Polygon2d Polygon;

		public bool IsClosed => true;

		// can call SampleT in range [0,ParamLength]
		public double ParamLength => Polygon.VertexCount;
		public Vector2d SampleT(in double t) {
			var i = (int)t;
			if (i >= Polygon.VertexCount - 1) {
				return Polygon[Polygon.VertexCount - 1];
			}

			var a = Polygon[i];
			var b = Polygon[i + 1];
			var alpha = t - (double)i;
			return ((1.0 - alpha) * a) + (alpha * b);
		}
		public Vector2d TangentT(in double t) {
			throw new NotImplementedException("Polygon2dCurve.TangentT");
		}

		public bool HasArcLength => true;
		public double ArcLength => Polygon.ArcLength;
		public Vector2d SampleArcLength(in double a) {
			throw new NotImplementedException("Polygon2dCurve.SampleArcLength");
		}

		public void Reverse() {
			Polygon.Reverse();
		}

		public IParametricCurve2d Clone() {
			return new Polygon2DCurve() { Polygon = new Polygon2d(Polygon) };
		}

		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform) {
			Polygon.Transform(xform);
		}

	}




}
