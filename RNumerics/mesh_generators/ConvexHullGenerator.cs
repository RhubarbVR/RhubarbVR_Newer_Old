using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RNumerics
{
	public sealed class ConvexHullGenerator : MeshGenerator
	{
		// [ThreadStatic]
		public Vector3f[] points;
		public bool splitVerts = true;

		// https://github.com/OskarSigvardsson/unity-quickhull
		public override MeshGenerator Generate() {
			WantUVs = false;
			GenerateHull(points, splitVerts, ref base.vertices, ref base.triangles, ref base.normals);
			return this;
		}

		/// <summary>
		///   Constant representing a point that has yet to be assigned to a
		///   face. It's only used immediately after constructing the seed hull.
		/// </summary>
		const int UNASSIGNED = -2;

		/// <summary>
		///   Constant representing a point that is inside the convex hull, and
		///   thus is behind all faces. In the openSet array, all points with
		///   INSIDE are at the end of the array, with indexes larger
		///   openSetTail.
		/// </summary>
		const int INSIDE = -1;

		/// <summary>
		///   Epsilon value. If the coordinates of the point space are
		///   exceptionally close to each other, this value might need to be
		///   adjusted.
		/// </summary>
		const float EPSILON = 0.0001f;

		/// <summary>
		///   Struct representing a single face.
		///
		///   Vertex0, Vertex1 and Vertex2 are the vertices in CCW order. They
		///   acutal points are stored in the points array, these are just
		///   indexes into that array.
		///
		///   Opposite0, Opposite1 and Opposite2 are the keys to the faces which
		///   share an edge with this face. Opposite0 is the face opposite
		///   Vertex0 (so it has an edge with Vertex2 and Vertex1), etc.
		///
		///   Normal is (unsurprisingly) the normal of the triangle.
		/// </summary>
		struct Face
		{
			public int Vertex0;
			public int Vertex1;
			public int Vertex2;

			public int Opposite0;
			public int Opposite1;
			public int Opposite2;

			public Vector3f Normal;

			public Face(in int v0, in int v1, in int v2, in int o0, in int o1, in int o2, in Vector3f normal) {
				Vertex0 = v0;
				Vertex1 = v1;
				Vertex2 = v2;
				Opposite0 = o0;
				Opposite1 = o1;
				Opposite2 = o2;
				Normal = normal;
			}

			public bool Equals(in Face other) {
				return (Vertex0 == other.Vertex0)
					&& (Vertex1 == other.Vertex1)
					&& (Vertex2 == other.Vertex2)
					&& (Opposite0 == other.Opposite0)
					&& (Opposite1 == other.Opposite1)
					&& (Opposite2 == other.Opposite2)
					&& (Normal == other.Normal);
			}
		}

		/// <summary>
		///   Struct representing a mapping between a point and a face. These
		///   are used in the openSet array.
		///
		///   Point is the index of the point in the points array, Face is the
		///   key of the face in the Key dictionary, Distance is the distance
		///   from the face to the point.
		/// </summary>
		struct PointFace
		{
			public int Point;
			public int Face;
			public float Distance;

			public PointFace(in int p, in int f, in float d) {
				Point = p;
				Face = f;
				Distance = d;
			}
		}

		/// <summary>
		///   Struct representing a single edge in the horizon.
		///
		///   Edge0 and Edge1 are the vertexes of edge in CCW order, Face is the
		///   face on the other side of the horizon.
		///
		///   TODO Edge1 isn't actually needed, you can just index the next item
		///   in the horizon array.
		/// </summary>
		struct HorizonEdge
		{
			public int Face;
			public int Edge0;
			public int Edge1;
		}

		/// <summary>
		///   A dictionary storing the faces of the currently generated convex
		///   hull. The key is the id of the face, used in the Face, PointFace
		///   and HorizonEdge struct.
		///
		///   This is a Dictionary, because we need both random access to it,
		///   the ability to loop through it, and ability to quickly delete
		///   faces (in the ConstructCone method), and Dictionary is the obvious
		///   candidate that can do all of those things.
		///
		///   I'm wondering if using a Dictionary is best idea, though. It might
		///   be better to just have them in a List<Face> and mark a face as
		///   deleted by adding a field to the Face struct. The downside is that
		///   we would need an extra field in the Face struct, and when we're
		///   looping through the points in openSet, we would have to loop
		///   through all the Faces EVER created in the algorithm, and skip the
		///   ones that have been marked as deleted. However, looping through a
		///   list is fairly fast, and it might be worth it to avoid Dictionary
		///   overhead.
		///
		///   TODO test converting to a List<Face> instead.
		/// </summary>
		Dictionary<int, Face> _faces;

		/// <summary>
		///   The set of points to be processed. "openSet" is a misleading name,
		///   because it's both the open set (points which are still outside the
		///   convex hull) and the closed set (points that are inside the convex
		///   hull). The first part of the array (with indexes <= openSetTail)
		///   is the openSet, the last part of the array (with indexes >
		///   openSetTail) are the closed set, with Face set to INSIDE. The
		///   closed set is largely irrelevant to the algorithm, the open set is
		///   what matters.
		///
		///   Storing the entire open set in one big list has a downside: when
		///   we're reassigning points after ConstructCone, we only need to
		///   reassign points that belong to the faces that have been removed,
		///   but storing it in one array, we have to loop through the entire
		///   list, and checking litFaces to determine which we can skip and
		///   which need to be reassigned.
		///
		///   The alternative here is to give each face in Face array it's own
		///   openSet. I don't like that solution, because then you have to
		///   juggle so many more heap-allocated List<T>'s, we'd have to use
		///   object pools and such. It would do a lot more allocation, and it
		///   would have worse locality. I should maybe test that solution, but
		///   it probably wont be faster enough (if at all) to justify the extra
		///   allocations.
		/// </summary>
		List<PointFace> _openSet;

		/// <summary>
		///   Set of faces which are "lit" by the current point in the set. This
		///   is used in the FindHorizon() DFS search to keep track of which
		///   faces we've already visited, and in the ReassignPoints() method to
		///   know which points need to be reassigned.
		/// </summary>
		HashSet<int> _litFaces;

		/// <summary>
		///   The current horizon. Generated by the FindHorizon() DFS search,
		///   and used in ConstructCone to construct new faces. The list of
		///   edges are in CCW order.
		/// </summary>
		List<HorizonEdge> _horizon;

		/// <summary>
		///   If SplitVerts is false, this Dictionary is used to keep track of
		///   which points we've added to the final mesh.
		/// </summary>
		Dictionary<int, int> _hullVerts;

		/// <summary>
		///   The "tail" of the openSet, the last index of a vertex that has
		///   been assigned to a face.
		/// </summary>
		int _openSetTail = -1;

		/// <summary>
		///   When adding a new face to the faces Dictionary, use this for the
		///   key and then increment it.
		/// </summary>
		int _faceCount = 0;

		/// <summary>
		///   Generate a convex hull from points in points array, and store the
		///   mesh in Unity-friendly format in verts and tris. If splitVerts is
		///   true, the the verts will be split, if false, the same vert will be
		///   used for more than one triangle.
		/// </summary>
		public void GenerateHull(
			in Vector3f[] points,
			in bool splitVerts,
			ref VectorArray3d verts,
			ref IndexArray3i tris,
			ref VectorArray3f normals) {
			if (points.Length < 4) {
				throw new ArgumentException("Need at least 4 points to generate a convex hull");
			}

			Initialize(points, splitVerts);

			GenerateInitialHull(points);

			while (_openSetTail >= 0) {
				GrowHull(points);
			}

			ExportMesh(points, splitVerts, ref verts, ref tris, ref normals);
			// VerifyMesh(points, ref verts, ref tris);
		}

		/// <summary>
		///   Make sure all the buffers and variables needed for the algorithm
		///   are initialized.
		/// </summary>
		void Initialize(in Vector3f[] points, in bool splitVerts) {
			_faceCount = 0;
			_openSetTail = -1;

			if (_faces == null) {
				_faces = new Dictionary<int, Face>();
				_litFaces = new HashSet<int>();
				_horizon = new List<HorizonEdge>();
				_openSet = new List<PointFace>(points.Length);
			}
			else {
				_faces.Clear();
				_litFaces.Clear();
				_horizon.Clear();
				_openSet.Clear();

				if (_openSet.Capacity < points.Length) {
					// i wonder if this is a good idea... if you call
					// GenerateHull over and over with slightly increasing
					// points counts, it's going to reallocate every time. Maybe
					// i should just use .Add(), and let the List<T> manage the
					// capacity, increasing it geometrically every time we need
					// to reallocate.

					// maybe do
					//   openSet.Capacity = Mathf.NextPowerOfTwo(points.Count)
					// instead?

					_openSet.Capacity = points.Length;
				}
			}

			if (!splitVerts) {
				if (_hullVerts == null) {
					_hullVerts = new Dictionary<int, int>();
				}
				else {
					_hullVerts.Clear();
				}
			}
		}

		/// <summary>
		///   Create initial seed hull.
		/// </summary>
		void GenerateInitialHull(in Vector3f[] points) {
			// Find points suitable for use as the seed hull. Some varieties of
			// this algorithm pick extreme points here, but I'm not convinced
			// you gain all that much from that. Currently what it does is just
			// find the first four points that are not coplanar.
			FindInitialHullIndices(points, out var b0, out var b1, out var b2, out var b3);

			var v0 = points[b0];
			var v1 = points[b1];
			var v2 = points[b2];
			var v3 = points[b3];

			var above = Dot(v3 - v1, Cross(v1 - v0, v2 - v0)) > 0.0f;

			// Create the faces of the seed hull. You need to draw a diagram
			// here, otherwise it's impossible to know what's going on :)

			// Basically: there are two different possible start-tetrahedrons,
			// depending on whether the fourth point is above or below the base
			// triangle. If you draw a tetrahedron with these coordinates (in a
			// right-handed coordinate-system):

			//   b0 = (0,0,0)
			//   b1 = (1,0,0)
			//   b2 = (0,1,0)
			//   b3 = (0,0,1)

			// you can see the first case (set b3 = (0,0,-1) for the second
			// case). The faces are added with the proper references to the
			// faces opposite each vertex

			_faceCount = 0;
			if (above) {
				_faces[_faceCount++] = new Face(b0, b2, b1, 3, 1, 2, Normal(points[b0], points[b2], points[b1]));
				_faces[_faceCount++] = new Face(b0, b1, b3, 3, 2, 0, Normal(points[b0], points[b1], points[b3]));
				_faces[_faceCount++] = new Face(b0, b3, b2, 3, 0, 1, Normal(points[b0], points[b3], points[b2]));
				_faces[_faceCount++] = new Face(b1, b2, b3, 2, 1, 0, Normal(points[b1], points[b2], points[b3]));
			}
			else {
				_faces[_faceCount++] = new Face(b0, b1, b2, 3, 2, 1, Normal(points[b0], points[b1], points[b2]));
				_faces[_faceCount++] = new Face(b0, b3, b1, 3, 0, 2, Normal(points[b0], points[b3], points[b1]));
				_faces[_faceCount++] = new Face(b0, b2, b3, 3, 1, 0, Normal(points[b0], points[b2], points[b3]));
				_faces[_faceCount++] = new Face(b1, b3, b2, 2, 0, 1, Normal(points[b1], points[b3], points[b2]));
			}

			// VerifyFaces(points);

			// Create the openSet. Add all points except the points of the seed
			// hull.
			for (var i = 0; i < points.Length; i++) {
				if (i == b0 || i == b1 || i == b2 || i == b3) {
					continue;
				}

				_openSet.Add(new PointFace(i, UNASSIGNED, 0.0f));
			}

			// Add the seed hull verts to the tail of the list.
			_openSet.Add(new PointFace(b0, INSIDE, float.NaN));
			_openSet.Add(new PointFace(b1, INSIDE, float.NaN));
			_openSet.Add(new PointFace(b2, INSIDE, float.NaN));
			_openSet.Add(new PointFace(b3, INSIDE, float.NaN));

			// Set the openSetTail value. Last item in the array is
			// openSet.Count - 1, but four of the points (the verts of the seed
			// hull) are part of the closed set, so move openSetTail to just
			// before those.
			_openSetTail = _openSet.Count - 5;

			if (_openSet.Count != points.Length) {
				throw new ArgumentException("openSet.Count != points.Count");
			}

			// Assign all points of the open set. This does basically the same
			// thing as ReassignPoints()
			for (var i = 0; i <= _openSetTail; i++) {
				if (_openSet[i].Face != UNASSIGNED) {
					throw new ArgumentException("openSet[i].Face != UNASSIGNED");
				}
				if (_openSet[_openSetTail].Face != UNASSIGNED) {
					throw new ArgumentException("openSet[openSetTail].Face != UNASSIGNED");
				}
				if (_openSet[_openSetTail + 1].Face != INSIDE) {
					throw new ArgumentException("openSet[openSetTail + 1].Face != INSIDE");
				}

				var assigned = false;
				var fp = _openSet[i];

				if (_faces.Count != 4) {
					throw new ArgumentException("faces.Count != 4");
				}

				if (_faces.Count != _faceCount) {
					throw new ArgumentException("faces.Count != faceCount");
				}

				for (var j = 0; j < 4; j++) {
					if (!_faces.ContainsKey(j)) {
						throw new ArgumentException("Dummy");
					}

					var face = _faces[j];

					var dist = PointFaceDistance(points[fp.Point], points[face.Vertex0], face);

					if (dist > 0) {
						fp.Face = j;
						fp.Distance = dist;
						_openSet[i] = fp;

						assigned = true;
						break;
					}
				}

				if (!assigned) {
					// Point is inside
					fp.Face = INSIDE;
					fp.Distance = float.NaN;

					// Point is inside seed hull: swap point with tail, and move
					// openSetTail back. We also have to decrement i, because
					// there's a new item at openSet[i], and we need to process
					// it next iteration
					_openSet[i] = _openSet[_openSetTail];
					_openSet[_openSetTail] = fp;

					_openSetTail -= 1;
					i -= 1;
				}
			}

			// VerifyOpenSet(points);
		}

		/// <summary>
		///   Find four points in the point cloud that are not coplanar for the
		///   seed hull
		/// </summary>
		void FindInitialHullIndices(in Vector3f[] points, out int b0, out int b1, out int b2, out int b3) {
			var count = points.Length;

			for (var i0 = 0; i0 < count - 3; i0++) {
				for (var i1 = i0 + 1; i1 < count - 2; i1++) {
					var p0 = points[i0];
					var p1 = points[i1];

					if (AreCoincident(p0, p1)) {
						continue;
					}

					for (var i2 = i1 + 1; i2 < count - 1; i2++) {
						var p2 = points[i2];

						if (AreCollinear(p0, p1, p2)) {
							continue;
						}

						for (var i3 = i2 + 1; i3 < count - 0; i3++) {
							var p3 = points[i3];

							if (AreCoplanar(p0, p1, p2, p3)) {
								continue;
							}

							b0 = i0;
							b1 = i1;
							b2 = i2;
							b3 = i3;
							return;
						}
					}
				}
			}

			throw new ArgumentException("Can't generate hull, points are coplanar");
		}

		/// <summary>
		///   Grow the hull. This method takes the current hull, and expands it
		///   to encompass the point in openSet with the point furthest away
		///   from its face.
		/// </summary>
		void GrowHull(in Vector3f[] points) {
			if (!(_openSetTail >= 0)) {
				throw new ArgumentException("!(openSetTail >= 0)");
			}
			if (_openSet[0].Face == INSIDE) {
				throw new ArgumentException("openSet[0].Face == INSIDE");
			}

			// Find farthest point and first lit face.
			var farthestPoint = 0;
			var dist = _openSet[0].Distance;

			for (var i = 1; i <= _openSetTail; i++) {
				if (_openSet[i].Distance > dist) {
					farthestPoint = i;
					dist = _openSet[i].Distance;
				}
			}

			// Use lit face to find horizon and the rest of the lit
			// faces.
			FindHorizon(
				points,
				points[_openSet[farthestPoint].Point],
				_openSet[farthestPoint].Face,
				_faces[_openSet[farthestPoint].Face]);

			// VerifyHorizon();

			// Construct new cone from horizon
			ConstructCone(points, _openSet[farthestPoint].Point);

			// VerifyFaces(points);

			// Reassign points
			ReassignPoints(points);
		}

		/// <summary>
		///   Start the search for the horizon.
		///
		///   The search is a DFS search that searches neighboring triangles in
		///   a counter-clockwise fashion. When it find a neighbor which is not
		///   lit, that edge will be a line on the horizon. If the search always
		///   proceeds counter-clockwise, the edges of the horizon will be found
		///   in counter-clockwise order.
		///
		///   The heart of the search can be found in the recursive
		///   SearchHorizon() method, but the the first iteration of the search
		///   is special, because it has to visit three neighbors (all the
		///   neighbors of the initial triangle), while the rest of the search
		///   only has to visit two (because one of them has already been
		///   visited, the one you came from).
		/// </summary>
		void FindHorizon(in Vector3f[] points, in Vector3f point, in int fi, in Face face) {
			// TODO should I use epsilon in the PointFaceDistance comparisons?

			_litFaces.Clear();
			_horizon.Clear();

			_litFaces.Add(fi);

			if (!(PointFaceDistance(point, points[face.Vertex0], face) > 0.0f)) {
				throw new ArgumentException("point, points[face.Vertex0], face) > 0.0f");
			}

			// For the rest of the recursive search calls, we first check if the
			// triangle has already been visited and is part of litFaces.
			// However, in this first call we can skip that because we know it
			// can't possibly have been visited yet, since the only thing in
			// litFaces is the current triangle.
			{
				var oppositeFace = _faces[face.Opposite0];

				var dist = PointFaceDistance(
					point,
					points[oppositeFace.Vertex0],
					oppositeFace);

				if (dist <= 0.0f) {
					_horizon.Add(new HorizonEdge {
						Face = face.Opposite0,
						Edge0 = face.Vertex1,
						Edge1 = face.Vertex2,
					});
				}
				else {
					SearchHorizon(points, point, fi, face.Opposite0, oppositeFace);
				}
			}

			if (!_litFaces.Contains(face.Opposite1)) {
				var oppositeFace = _faces[face.Opposite1];

				var dist = PointFaceDistance(
					point,
					points[oppositeFace.Vertex0],
					oppositeFace);

				if (dist <= 0.0f) {
					_horizon.Add(new HorizonEdge {
						Face = face.Opposite1,
						Edge0 = face.Vertex2,
						Edge1 = face.Vertex0,
					});
				}
				else {
					SearchHorizon(points, point, fi, face.Opposite1, oppositeFace);
				}
			}

			if (!_litFaces.Contains(face.Opposite2)) {
				var oppositeFace = _faces[face.Opposite2];

				var dist = PointFaceDistance(
					point,
					points[oppositeFace.Vertex0],
					oppositeFace);

				if (dist <= 0.0f) {
					_horizon.Add(new HorizonEdge {
						Face = face.Opposite2,
						Edge0 = face.Vertex0,
						Edge1 = face.Vertex1,
					});
				}
				else {
					SearchHorizon(points, point, fi, face.Opposite2, oppositeFace);
				}
			}
		}

		/// <summary>
		///   Recursively search to find the horizon or lit set.
		/// </summary>
		void SearchHorizon(in Vector3f[] points, in Vector3f point, in int prevFaceIndex, in int faceCount, in Face face) {
			if (!(prevFaceIndex >= 0)) {
				throw new ArgumentException("prevFaceIndex >= 0");
			}
			if (!_litFaces.Contains(prevFaceIndex)) {
				throw new ArgumentException("itFaces.Contains(prevFaceIndex)");
			}
			if (_litFaces.Contains(faceCount)) {
				throw new ArgumentException("!litFaces.Contains(faceCount)");
			}
			if (!_faces[faceCount].Equals(face)) {
				throw new ArgumentException("faces[faceCount].Equals(face)");
			}

			_litFaces.Add(faceCount);

			// Use prevFaceIndex to determine what the next face to search will
			// be, and what edges we need to cross to get there. It's important
			// that the search proceeds in counter-clockwise order from the
			// previous face.
			int nextFaceIndex0;
			int nextFaceIndex1;
			int edge0;
			int edge1;
			int edge2;

			if (prevFaceIndex == face.Opposite0) {
				nextFaceIndex0 = face.Opposite1;
				nextFaceIndex1 = face.Opposite2;

				edge0 = face.Vertex2;
				edge1 = face.Vertex0;
				edge2 = face.Vertex1;
			}
			else if (prevFaceIndex == face.Opposite1) {
				nextFaceIndex0 = face.Opposite2;
				nextFaceIndex1 = face.Opposite0;

				edge0 = face.Vertex0;
				edge1 = face.Vertex1;
				edge2 = face.Vertex2;
			}
			else {
				if (prevFaceIndex != face.Opposite2) {
					throw new ArgumentException("prevFaceIndex != face.Opposite2");
				}

				nextFaceIndex0 = face.Opposite0;
				nextFaceIndex1 = face.Opposite1;

				edge0 = face.Vertex1;
				edge1 = face.Vertex2;
				edge2 = face.Vertex0;
			}

			if (!_litFaces.Contains(nextFaceIndex0)) {
				var oppositeFace = _faces[nextFaceIndex0];

				var dist = PointFaceDistance(
					point,
					points[oppositeFace.Vertex0],
					oppositeFace);

				if (dist <= 0.0f) {
					_horizon.Add(new HorizonEdge {
						Face = nextFaceIndex0,
						Edge0 = edge0,
						Edge1 = edge1,
					});
				}
				else {
					SearchHorizon(points, point, faceCount, nextFaceIndex0, oppositeFace);
				}
			}

			if (!_litFaces.Contains(nextFaceIndex1)) {
				var oppositeFace = _faces[nextFaceIndex1];

				var dist = PointFaceDistance(
					point,
					points[oppositeFace.Vertex0],
					oppositeFace);

				if (dist <= 0.0f) {
					_horizon.Add(new HorizonEdge {
						Face = nextFaceIndex1,
						Edge0 = edge1,
						Edge1 = edge2,
					});
				}
				else {
					SearchHorizon(points, point, faceCount, nextFaceIndex1, oppositeFace);
				}
			}
		}

		/// <summary>
		///   Remove all lit faces and construct new faces from the horizon in a
		///   "cone-like" fashion.
		///
		///   This is a relatively straight-forward procedure, given that the
		///   horizon is handed to it in already sorted counter-clockwise. The
		///   neighbors of the new faces are easy to find: they're the previous
		///   and next faces to be constructed in the cone, as well as the face
		///   on the other side of the horizon. We also have to update the face
		///   on the other side of the horizon to reflect it's new neighbor from
		///   the cone.
		/// </summary>
		void ConstructCone(in Vector3f[] points, in int farthestPoint) {
			foreach (var fi in _litFaces) {
				if (!_faces.ContainsKey(fi)) {
					throw new ArgumentException("!faces.ContainsKey(fi)");
				}
				_faces.Remove(fi);
			}

			var firstNewFace = _faceCount;

			for (var i = 0; i < _horizon.Count; i++) {
				// Vertices of the new face, the farthest point as well as the
				// edge on the horizon. Horizon edge is CCW, so the triangle
				// should be as well.
				var v0 = farthestPoint;
				var v1 = _horizon[i].Edge0;
				var v2 = _horizon[i].Edge1;

				// Opposite faces of the triangle. First, the edge on the other
				// side of the horizon, then the next/prev faces on the new cone
				var o0 = _horizon[i].Face;
				var o1 = (i == _horizon.Count - 1) ? firstNewFace : firstNewFace + i + 1;
				var o2 = (i == 0) ? (firstNewFace + _horizon.Count - 1) : firstNewFace + i - 1;

				var fi = _faceCount++;

				_faces[fi] = new Face(
					v0, v1, v2,
					o0, o1, o2,
					Normal(points[v0], points[v1], points[v2]));

				var horizonFace = _faces[_horizon[i].Face];

				if (horizonFace.Vertex0 == v1) {
					if (v2 != horizonFace.Vertex2) {
						throw new ArgumentException("v2 != horizonFace.Vertex2");
					}
					horizonFace.Opposite1 = fi;
				}
				else if (horizonFace.Vertex1 == v1) {
					if (v2 != horizonFace.Vertex0) {
						throw new ArgumentException("v2 != horizonFace.Vertex0");
					}
					horizonFace.Opposite2 = fi;
				}
				else {
					if (v1 != horizonFace.Vertex2) {
						throw new ArgumentException("v1 != horizonFace.Vertex2");
					}
					if (v2 != horizonFace.Vertex1) {
						throw new ArgumentException("v2 != horizonFace.Vertex1");
					}

					horizonFace.Opposite0 = fi;
				}

				_faces[_horizon[i].Face] = horizonFace;
			}
		}

		/// <summary>
		///   Reassign points based on the new faces added by ConstructCone().
		///
		///   Only points that were previous assigned to a removed face need to
		///   be updated, so check litFaces while looping through the open set.
		///
		///   There is a potential optimization here: there's no reason to loop
		///   through the entire openSet here. If each face had it's own
		///   openSet, we could just loop through the openSets in the removed
		///   faces. That would make the loop here shorter.
		///
		///   However, to do that, we would have to juggle A LOT more List<T>'s,
		///   and we would need an object pool to manage them all without
		///   generating a whole bunch of garbage. I don't think it's worth
		///   doing that to make this loop shorter, a straight for-loop through
		///   a list is pretty darn fast. Still, it might be worth trying
		/// </summary>
		void ReassignPoints(in Vector3f[] points) {
			for (var i = 0; i <= _openSetTail; i++) {
				var fp = _openSet[i];

				if (_litFaces.Contains(fp.Face)) {
					var assigned = false;
					var point = points[fp.Point];

					foreach (var kvp in _faces) {
						var fi = kvp.Key;
						var face = kvp.Value;

						var dist = PointFaceDistance(
							point,
							points[face.Vertex0],
							face);

						if (dist > EPSILON) {
							assigned = true;

							fp.Face = fi;
							fp.Distance = dist;

							_openSet[i] = fp;
							break;
						}
					}

					if (!assigned) {
						// If point hasn't been assigned, then it's inside the
						// convex hull. Swap it with openSetTail, and decrement
						// openSetTail. We also have to decrement i, because
						// there's now a new thing in openSet[i], so we need i
						// to remain the same the next iteration of the loop.
						fp.Face = INSIDE;
						fp.Distance = float.NaN;

						_openSet[i] = _openSet[_openSetTail];
						_openSet[_openSetTail] = fp;

						i--;
						_openSetTail--;
					}
				}
			}
		}

		/// <summary>
		///   Final step in algorithm, export the faces of the convex hull in a
		///   mesh-friendly format.
		///
		///   TODO normals calculation for non-split vertices. Right now it just
		///   leaves the normal array empty.
		/// </summary>
		void ExportMesh(
			in Vector3f[] points,
			in bool splitVerts,
			ref VectorArray3d re_verts,
			ref IndexArray3i re_tris,
			ref VectorArray3f re_normals) {

			var verts = new List<Vector3f>();
			var tris = new List<int>();
			var normals = new List<Vector3f>();

			foreach (var face in _faces.Values) {
				int vi0, vi1, vi2;

				if (splitVerts) {
					vi0 = verts.Count;
					verts.Add(points[face.Vertex0]);
					vi1 = verts.Count;
					verts.Add(points[face.Vertex1]);
					vi2 = verts.Count;
					verts.Add(points[face.Vertex2]);

					normals.Add(face.Normal);
					normals.Add(face.Normal);
					normals.Add(face.Normal);
				}
				else {
					if (!_hullVerts.TryGetValue(face.Vertex0, out vi0)) {
						vi0 = verts.Count;
						_hullVerts[face.Vertex0] = vi0;
						verts.Add(points[face.Vertex0]);
					}

					if (!_hullVerts.TryGetValue(face.Vertex1, out vi1)) {
						vi1 = verts.Count;
						_hullVerts[face.Vertex1] = vi1;
						verts.Add(points[face.Vertex1]);
					}

					if (!_hullVerts.TryGetValue(face.Vertex2, out vi2)) {
						vi2 = verts.Count;
						_hullVerts[face.Vertex2] = vi2;
						verts.Add(points[face.Vertex2]);
					}
				}

				tris.Add(vi0);
				tris.Add(vi1);
				tris.Add(vi2);
			}

			re_verts = new VectorArray3d(verts.Count);
			re_tris = new IndexArray3i(tris.Count / 3);
			re_normals = new VectorArray3f(verts.Count);

			for (var i = 0; i < verts.Count; i++) {
				re_verts[i] = verts[i];
			}

			for (var i = 0; i < tris.Count; i += 3) {
				re_tris[i / 3] = new Index3i(
					tris[i],
					tris[i + 1],
					tris[i + 2]
				);
			}

			for (var i = 0; i < verts.Count; i++) {
				re_normals[i] = new Vector3f(
					normals[i].x,
					normals[i].y,
					normals[i].z
					);
			}
		}

		/// <summary>
		///   Signed distance from face to point (a positive number means that
		///   the point is above the face)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		float PointFaceDistance(in Vector3f point, in Vector3f pointOnFace, in Face face) {
			return Dot(face.Normal, point - pointOnFace);
		}

		/// <summary>
		///   Calculate normal for triangle
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		Vector3f Normal(in Vector3f v0, in Vector3f v1, in Vector3f v2) {
			return Cross(v1 - v0, v2 - v0).Normalized;
		}

		/// <summary>
		///   Dot product, for convenience.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static float Dot(in Vector3f a, in Vector3f b) {
			return (a.x * b.x) + (a.y * b.y) + (a.z * b.z);
		}

		/// <summary>
		///   Vector3f.Cross i left-handed, the algorithm is right-handed. Also,
		///   i wanna test to see if using aggressive inlining makes any
		///   difference here.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Vector3f Cross(in Vector3f a, in Vector3f b) {
			return new Vector3f(
				(a.y * b.z) - (a.z * b.y),
				(a.z * b.x) - (a.x * b.z),
				(a.x * b.y) - (a.y * b.x));
		}

		/// <summary>
		///   Check if two points are coincident
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool AreCoincident(in Vector3f a, in Vector3f b) {
			return (a - b).Magnitude <= EPSILON;
		}

		/// <summary>
		///   Check if three points are collinear
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool AreCollinear(in Vector3f a, in Vector3f b, in Vector3f c) {
			return Cross(c - a, c - b).Magnitude <= EPSILON;
		}

		/// <summary>
		///   Check if four points are coplanar
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool AreCoplanar(in Vector3f a, in Vector3f b, in Vector3f c, in Vector3f d) {
			var n1 = Cross(c - a, c - b);
			var n2 = Cross(d - a, d - b);

			var m1 = n1.Magnitude;
			var m2 = n2.Magnitude;

			return m1 <= EPSILON
				|| m2 <= EPSILON
				|| AreCollinear(Vector3f.Zero,
					1.0f / m1 * n1,
					1.0f / m2 * n2);
		}
	}
}
