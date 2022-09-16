using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	/// <summary>
	/// Construct convex hull of a set of 2D points, with various accuracy levels.
	/// 
	/// HullIndices provides ordered indices of vertices of input points that form hull.
	/// </summary>
	public sealed class ConvexHull2
	{
		//QueryNumberType mQueryType = QueryNumberType.QT_DOUBLE;
		readonly IList<Vector2d> _mVertices;
		readonly int _mNumVertices;
		readonly int _mNumSimplices;
		readonly double _mEpsilon;
		readonly Vector2d[] _mSVertices;
		readonly int[] _mIndices;
		readonly IQuery2 _mQuery;
		Vector2d _mLineOrigin;
		Vector2d _mLineDirection;


		/*
         * Outputs
         */

		public int Dimension { get; }

		/// <summary>
		/// Number of convex polygon edges
		/// </summary>
		public int NumSimplices => _mNumSimplices;


		/// <summary>
		///   array of indices into V that represent the convex polygon edges (NumSimplices total elements)
		/// The i-th edge has vertices
		///   vertex[0] = V[I[i]]
		///   vertex[1] = V[I[(i+1) % SQ]]
		/// </summary>
		public int[] HullIndices => _mIndices;


		/// <summary>
		/// Compute convex hull of input points. 
		/// epsilon is only used for check if points lie on a line (1d hull), not for rest of compute.
		/// </summary>
		public ConvexHull2(in IList<Vector2d> vertices, in double epsilon, in QueryNumberType queryType) {
			//mQueryType = queryType;
			_mVertices = vertices;
			_mNumVertices = vertices.Count;
			Dimension = 0;
			_mNumSimplices = 0;
			_mIndices = null;
			_mSVertices = null;

			_mEpsilon = epsilon;

			_mQuery = null;

			_mLineOrigin = Vector2d.Zero;
			_mLineDirection = Vector2d.Zero;

			Vector2d.GetInformation(_mVertices, _mEpsilon, out var info);
			if (info.mDimension == 0) {
				Dimension = 0;
				_mIndices = null;
				return;
			}

			if (info.mDimension == 1) {
				// The set is (nearly) collinear.  The caller is responsible for
				// creating a ConvexHull1 object.
				Dimension = 1;
				_mLineOrigin = info.mOrigin;
				_mLineDirection = info.mDirection0;
				return;
			}

			Dimension = 2;

			var i0 = info.mExtreme[0];
			var i1 = info.mExtreme[1];
			var	i2 = info.mExtreme[2];

			_mSVertices = new Vector2d[_mNumVertices];

			if (queryType is not QueryNumberType.QT_RATIONAL and not QueryNumberType.QT_FILTERED) {

				// Transform the vertices to the square [0,1]^2.
				var minValue = new Vector2d(info.mMin[0], info.mMin[1]);
				var scale = 1 / info.mMaxRange;
				for (var i = 0; i < _mNumVertices; ++i) {
					_mSVertices[i] = (_mVertices[i] - minValue) * scale;
				}

				double expand;
				if (queryType == QueryNumberType.QT_INT64) {
					// Scale the vertices to the square [0,2^{20}]^2 to allow use of
					// 64-bit integers.
					expand = (double)(1 << 20);
					_mQuery = new Query2Int64(_mSVertices);

				}
				else if (queryType == QueryNumberType.QT_INTEGER) {
					throw new NotImplementedException("ConvexHull2: Query type QT_INTEGER not currently supported");
					// Scale the vertices to the square [0,2^{24}]^2 to allow use of
					// Integer.
					//expand = (double)(1 << 24);
					//mQuery = new Query2Integer(mNumVertices, mSVertices);
				}
				else {  // queryType == Query::QT_double
						// No scaling for floating point.
					expand = (double)1;
					_mQuery = new Query2d(_mSVertices);
				}

				for (var i = 0; i < _mNumVertices; ++i) {
					_mSVertices[i] *= expand;
				}
			}
			else {
				throw new NotImplementedException("ConvexHull2: Query type QT_RATIONAL/QT_FILTERED not currently supported");

				// No transformation needed for exact rational arithmetic or filtered
				// predicates.
				//for (int i = 0; i < mSVertices.Length; ++i)
				//    mSVertices[i] = mVertices[i];

				//if (queryType == Query::QT_RATIONAL) {
				//    mQuery = new Query2Rational(mNumVertices, mSVertices);
				//} else { // queryType == Query::QT_FILTERED
				//    mQuery = new Query2Filtered(mNumVertices, mSVertices,
				//        mEpsilon);
				//}
			}

			Edge edge2;
			Edge edge1;
			Edge edge0;
			if (info.mExtremeCCW) {
				edge0 = new Edge(i0, i1);
				edge1 = new Edge(i1, i2);
				edge2 = new Edge(i2, i0);
			}
			else {
				edge0 = new Edge(i0, i2);
				edge1 = new Edge(i2, i1);
				edge2 = new Edge(i1, i0);
			}

			edge0.Insert(edge2, edge1);
			edge1.Insert(edge0, edge2);
			edge2.Insert(edge1, edge0);

			var hull = edge0;

			// ideally we insert points in random order. but instead of
			// generating a permutation, just insert them using modulo-indexing, 
			// which is in the ballpark...
			var ii = 0;
			do {
				if (!Update(ref hull, ii)) {
					return;
				}

				ii = (ii + 31337) % _mNumVertices;
			} while (ii != 0);

			// original code, vastly slower in pathological cases
			//for (int i = 0; i < mNumVertices; ++i) {
			//    if ( ! Update(ref hull, i) )
			//        return;
			//}

			hull.GetIndices(ref _mNumSimplices, ref _mIndices);
		}



		/// <summary>
		/// If the resulting Dimension == 1, then you can use this to get some info...
		/// </summary>
		public void Get1DHullInfo(out Vector2d origin, out Vector2d direction) {
			origin = _mLineOrigin;
			direction = _mLineDirection;
		}


		/// <summary>
		/// Extract convex hull polygon from input points
		/// </summary>
		public Polygon2d GetHullPolygon() {
			if (_mIndices == null) {
				return null;
			}

			var poly = new Polygon2d();
			for (var i = 0; i < _mIndices.Length; ++i) {
				poly.AppendVertex(_mVertices[_mIndices[i]]);
			}

			return poly;
		}



		//ConvexHull1<double>* GetConvexHull1()
		//{
		//    assertion(mDimension == 1, "The dimension must be 1\n");
		//    if (mDimension != 1) {
		//        return 0;
		//    }

		//    double* projection = new1<double>(mNumVertices);
		//    for (int i = 0; i < mNumVertices; ++i) {
		//        Vector2d diff = mVertices[i] - mLineOrigin;
		//        projection[i] = mLineDirection.Dot(diff);
		//    }

		//    return new ConvexHull1<double>(mNumVertices, projection, mEpsilon, true,
		//        mQueryType);
		//}

		bool Update(ref Edge hull, in int i) {
			// Locate an edge visible to the input point (if possible).
			Edge visible = null;
			var current = hull;
			do {
				if (current.GetSign(i, _mQuery) > 0) {
					visible = current;
					break;
				}

				current = current.E1;
			}
			while (current != hull);

			if (visible == null) {
				// The point is inside the current hull; nothing to do.
				return true;
			}

			// Remove the visible edges.
			var adj0 = visible.E0;
			if (adj0 == null) {
				return false;
			}

			var adj1 = visible.E1;
			if (adj1 == null) {
				return false;
			}

			visible.DeleteSelf();

			while (adj0.GetSign(i, _mQuery) > 0) {
				hull = adj0;
				adj0 = adj0.E0;
				if (adj0 == null) {
					return false;
				}

				adj0.E1.DeleteSelf();
			}

			while (adj1.GetSign(i, _mQuery) > 0) {
				hull = adj1;
				adj1 = adj1.E1;
				if (adj1 == null) {
					return false;
				}

				adj1.E0.DeleteSelf();
			}

			// Insert the new edges formed by the input point and the end points of
			// the polyline of invisible edges.
			var edge0 = new Edge(adj0.V[1], i);
			var edge1 = new Edge(i, adj1.V[0]);
			edge0.Insert(adj0, edge1);
			edge1.Insert(edge0, adj1);
			hull = edge0;

			return true;
		}




		/// <summary>
		/// Internal class that represents edge of hull, and neighbours
		/// </summary>
		private sealed class Edge
		{
			public Vector2i V;
			public Edge E0;
			public Edge E1;
			public int Sign;
			public int Time;

			public Edge(in int v0, in int v1) {
				Sign = 0;
				Time = -1;
				V[0] = v0;
				V[1] = v1;
				E0 = null;
				E1 = null;
			}

			public int GetSign(in int i, in IQuery2 query) {
				if (i != Time) {
					Time = i;
					Sign = query.ToLine(i, V[0], V[1]);
				}
				return Sign;
			}

			public void Insert(in Edge adj0, in Edge adj1) {
				adj0.E1 = this;
				adj1.E0 = this;
				E0 = adj0;
				E1 = adj1;
			}

			public void DeleteSelf() {
				if (E0 != null) {
					E0.E1 = null;
				}

				if (E1 != null) {
					E1.E0 = null;
				}
			}

			public void GetIndices(ref int numIndices, ref int[] indices) {
				// Count the number of edge vertices and allocate the index array.
				numIndices = 0;
				var current = this;
				do {
					++numIndices;
					current = current.E1;
				} while (current != this);

				indices = new int[numIndices];

				// Fill the index array.
				numIndices = 0;
				current = this;
				do {
					indices[numIndices] = current.V[0];
					++numIndices;
					current = current.E1;
				} while (current != this);
			}


		};
	}

	// ported from WildMagic5 ContMinBox2. 
	// Compute a minimum-area oriented box containing the specified points.  The
	// algorithm uses the rotating calipers method.  If the input points represent
	// a counterclockwise-ordered polygon, set 'isConvexPolygon' to 'true';
	// otherwise, set 'isConvexPolygon' to 'false'.


	/// <summary>
	/// Fit minimal bounding-box to a set of 2D points. Result is in MinBox.
	/// </summary>
	public sealed class ContMinBox2
	{
		public Box2d _mMinBox;

		// Flags for the rotating calipers algorithm.
		private enum RCFlags
		{
			F_NONE, F_LEFT, F_RIGHT, F_BOTTOM, F_TOP
		};


		public Box2d MinBox => _mMinBox;

		public ContMinBox2(in IList<Vector2d> points, in double epsilon, in QueryNumberType queryType, in bool isConvexPolygon) {
			// Get the convex hull of the points.
			IList<Vector2d> hullPoints;
			int numPoints;
			if (isConvexPolygon) {
				hullPoints = points;
				numPoints = hullPoints.Count;
			}
			else {
				var hull = new ConvexHull2(points, epsilon, queryType);
				var hullDim = hull.Dimension;
				var hullNumSimplices = hull.NumSimplices;
				var hullIndices = hull.HullIndices;

				if (hullDim == 0) {
					_mMinBox.Center = points[0];
					_mMinBox.AxisX = Vector2d.AxisX;
					_mMinBox.AxisY = Vector2d.AxisY;
					_mMinBox.Extent[0] = (double)0;
					_mMinBox.Extent[1] = (double)0;
					return;
				}

				if (hullDim == 1) {
					throw new NotImplementedException("ContMinBox2: Have not implemented 1d case");
					//ConvexHull1 hull1 = hull.GetConvexHull1();
					//hullIndices = hull1->GetIndices();

					//mMinBox.Center = ((double)0.5) * (points[hullIndices[0]] +
					//    points[hullIndices[1]]);
					//Vector2d diff =
					//    points[hullIndices[1]] - points[hullIndices[0]];
					//mMinBox.Extent[0] = ((double)0.5) * diff.Normalize();
					//mMinBox.Extent[1] = (double)0.0;
					//mMinBox.Axis[0] = diff;
					//mMinBox.Axis[1] = -mMinBox.Axis[0].Perp();
					//return;
				}

				numPoints = hullNumSimplices;
				var pointsArray = new Vector2d[numPoints];
				for (var i = 0; i < numPoints; ++i) {
					pointsArray[i] = points[hullIndices[i]];
				}
				hullPoints = pointsArray;
			}

			// The input points are V[0] through V[N-1] and are assumed to be the
			// vertices of a convex polygon that are counterclockwise ordered.  The
			// input points must not contain three consecutive collinear points.

			// Unit-length edge directions of convex polygon.  These could be
			// precomputed and passed to this routine if the application requires it.
			var numPointsM1 = numPoints - 1;
			var edges = new Vector2d[numPoints];
			var visited = new bool[numPoints];
			for (var i = 0; i < numPointsM1; ++i) {
				edges[i] = hullPoints[i + 1] - hullPoints[i];
				edges[i].Normalize();
				visited[i] = false;
			}
			edges[numPointsM1] = hullPoints[0] - hullPoints[numPointsM1];
			edges[numPointsM1].Normalize();
			visited[numPointsM1] = false;

			// Find the smallest axis-aligned box containing the points.  Keep track
			// of the extremum indices, L (left), R (right), B (bottom), and T (top)
			// so that the following constraints are met:
			//   V[L].x <= V[i].x for all i and V[(L+1)%N].x > V[L].x
			//   V[R].x >= V[i].x for all i and V[(R+1)%N].x < V[R].x
			//   V[B].y <= V[i].y for all i and V[(B+1)%N].y > V[B].y
			//   V[T].y >= V[i].y for all i and V[(T+1)%N].y < V[T].y
			double xmin = hullPoints[0].x, xmax = xmin;
			double ymin = hullPoints[0].y, ymax = ymin;
			int LIndex = 0, RIndex = 0, BIndex = 0, TIndex = 0;
			for (var i = 1; i < numPoints; ++i) {
				if (hullPoints[i].x <= xmin) {
					xmin = hullPoints[i].x;
					LIndex = i;
				}
				if (hullPoints[i].x >= xmax) {
					xmax = hullPoints[i].x;
					RIndex = i;
				}

				if (hullPoints[i].y <= ymin) {
					ymin = hullPoints[i].y;
					BIndex = i;
				}
				if (hullPoints[i].y >= ymax) {
					ymax = hullPoints[i].y;
					TIndex = i;
				}
			}

			// Apply wrap-around tests to ensure the constraints mentioned above are
			// satisfied.
			if (LIndex == numPointsM1) {
				if (hullPoints[0].x <= xmin) {
					xmin = hullPoints[0].x;
					LIndex = 0;
				}
			}

			if (RIndex == numPointsM1) {
				if (hullPoints[0].x >= xmax) {
					xmax = hullPoints[0].x;
					RIndex = 0;
				}
			}

			if (BIndex == numPointsM1) {
				if (hullPoints[0].y <= ymin) {
					ymin = hullPoints[0].y;
					BIndex = 0;
				}
			}

			if (TIndex == numPointsM1) {
				if (hullPoints[0].y >= ymax) {
					ymax = hullPoints[0].y;
					TIndex = 0;
				}
			}

			// The dimensions of the axis-aligned box.  The extents store width and
			// height for now.
			_mMinBox.Center.x = ((double)0.5) * (xmin + xmax);
			_mMinBox.Center.y = ((double)0.5) * (ymin + ymax);
			_mMinBox.AxisX = Vector2d.AxisX;
			_mMinBox.AxisY = Vector2d.AxisY;
			_mMinBox.Extent[0] = ((double)0.5) * (xmax - xmin);
			_mMinBox.Extent[1] = ((double)0.5) * (ymax - ymin);
			var minAreaDiv4 = _mMinBox.Extent[0] * _mMinBox.Extent[1];

			// The rotating calipers algorithm.
			var U = Vector2d.AxisX;
			var V = Vector2d.AxisY;

			var done = false;
			while (!done) {
				// Determine the edge that forms the smallest angle with the current
				// box edges.
				var flag = RCFlags.F_NONE;
				var maxDot = (double)0;

				var dot = U.Dot(edges[BIndex]);
				if (dot > maxDot) {
					maxDot = dot;
					flag = RCFlags.F_BOTTOM;
				}

				dot = V.Dot(edges[RIndex]);
				if (dot > maxDot) {
					maxDot = dot;
					flag = RCFlags.F_RIGHT;
				}

				dot = -U.Dot(edges[TIndex]);
				if (dot > maxDot) {
					maxDot = dot;
					flag = RCFlags.F_TOP;
				}

				dot = -V.Dot(edges[LIndex]);
				if (dot > maxDot) {
					flag = RCFlags.F_LEFT;
				}

				switch (flag) {
					case RCFlags.F_BOTTOM:
						if (visited[BIndex]) {
							done = true;
						}
						else {
							// Compute box axes with E[B] as an edge.
							U = edges[BIndex];
							V = -U.Perp;
							UpdateBox(hullPoints[LIndex], hullPoints[RIndex],
								hullPoints[BIndex], hullPoints[TIndex], ref U, ref V,
								ref minAreaDiv4);

							// Mark edge visited and rotate the calipers.
							visited[BIndex] = true;
							if (++BIndex == numPoints) {
								BIndex = 0;
							}
						}
						break;
					case RCFlags.F_RIGHT:
						if (visited[RIndex]) {
							done = true;
						}
						else {
							// Compute box axes with E[R] as an edge.
							V = edges[RIndex];
							U = V.Perp;
							UpdateBox(hullPoints[LIndex], hullPoints[RIndex],
								hullPoints[BIndex], hullPoints[TIndex], ref U, ref V,
								ref minAreaDiv4);

							// Mark edge visited and rotate the calipers.
							visited[RIndex] = true;
							if (++RIndex == numPoints) {
								RIndex = 0;
							}
						}
						break;
					case RCFlags.F_TOP:
						if (visited[TIndex]) {
							done = true;
						}
						else {
							// Compute box axes with E[T] as an edge.
							U = -edges[TIndex];
							V = -U.Perp;
							UpdateBox(hullPoints[LIndex], hullPoints[RIndex],
								hullPoints[BIndex], hullPoints[TIndex], ref U, ref V,
								ref minAreaDiv4);

							// Mark edge visited and rotate the calipers.
							visited[TIndex] = true;
							if (++TIndex == numPoints) {
								TIndex = 0;
							}
						}
						break;
					case RCFlags.F_LEFT:
						if (visited[LIndex]) {
							done = true;
						}
						else {
							// Compute box axes with E[L] as an edge.
							V = -edges[LIndex];
							U = V.Perp;
							UpdateBox(hullPoints[LIndex], hullPoints[RIndex],
								hullPoints[BIndex], hullPoints[TIndex], ref U, ref V,
								ref minAreaDiv4);

							// Mark edge visited and rotate the calipers.
							visited[LIndex] = true;
							if (++LIndex == numPoints) {
								LIndex = 0;
							}
						}
						break;
					case RCFlags.F_NONE:
						// The polygon is a rectangle.
						done = true;
						break;
				}
			}

		}




		private void UpdateBox(in Vector2d LPoint, in Vector2d RPoint,
								 in Vector2d BPoint, in Vector2d TPoint,
								 ref Vector2d U, ref Vector2d V, ref double minAreaDiv4) {
			var RLDiff = RPoint - LPoint;
			var TBDiff = TPoint - BPoint;
			var extent0 = ((double)0.5) * U.Dot(RLDiff);
			var extent1 = ((double)0.5) * V.Dot(TBDiff);
			var areaDiv4 = extent0 * extent1;
			if (areaDiv4 < minAreaDiv4) {
				minAreaDiv4 = areaDiv4;
				_mMinBox.AxisX = U;
				_mMinBox.AxisY = V;
				_mMinBox.Extent[0] = extent0;
				_mMinBox.Extent[1] = extent1;
				var LBDiff = LPoint - BPoint;
				_mMinBox.Center = LPoint + (U * extent0) + (V * (extent1 - V.Dot(LBDiff)));
			}
		}

	}
}
