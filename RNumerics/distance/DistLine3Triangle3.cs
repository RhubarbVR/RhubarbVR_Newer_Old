using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public class DistLine3Triangle3
	{
		Line3d _line;
		public Line3d Line
		{
			get => _line;
			set { _line = value; DistanceSquared = -1.0; }
		}

		Triangle3d _triangle;
		public Triangle3d Triangle
		{
			get => _triangle;
			set { _triangle = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d LineClosest;
		public double LineParam;
		public Vector3d TriangleClosest;
		public Vector3d TriangleBaryCoords;


		public DistLine3Triangle3(Line3d LineIn, Triangle3d TriangleIn) {
			_triangle = TriangleIn;
			_line = LineIn;
		}

		public DistLine3Triangle3 Compute() {
			GetSquared();
			return this;
		}

		public double Get() {
			return Math.Sqrt(GetSquared());
		}


		public double GetSquared() {
			if (DistanceSquared >= 0) {
				return DistanceSquared;
			}

			// Test if line intersects triangle.  If so, the squared distance is zero.
			var edge0 = _triangle.V1 - _triangle.V0;
			var edge1 = _triangle.V2 - _triangle.V0;
			var normal = edge0.UnitCross(edge1);
			var NdD = normal.Dot(_line.Direction);
			if (Math.Abs(NdD) > MathUtil.ZERO_TOLERANCE) {
				// The line and triangle are not parallel, so the line intersects
				// the plane of the triangle.
				var diff = _line.Origin - _triangle.V0;
				Vector3d U = Vector3d.Zero, V = Vector3d.Zero;
				Vector3d.GenerateComplementBasis(ref U, ref V, _line.Direction);
				var UdE0 = U.Dot(edge0);
				var UdE1 = U.Dot(edge1);
				var UdDiff = U.Dot(diff);
				var VdE0 = V.Dot(edge0);
				var VdE1 = V.Dot(edge1);
				var VdDiff = V.Dot(diff);
				var invDet = 1 / ((UdE0 * VdE1) - (UdE1 * VdE0));

				// Barycentric coordinates for the point of intersection.
				var b1 = ((VdE1 * UdDiff) - (UdE1 * VdDiff)) * invDet;
				var b2 = ((UdE0 * VdDiff) - (VdE0 * UdDiff)) * invDet;
				var b0 = 1 - b1 - b2;

				if (b0 >= 0 && b1 >= 0 && b2 >= 0) {
					// Line parameter for the point of intersection.
					var DdE0 = _line.Direction.Dot(edge0);
					var DdE1 = _line.Direction.Dot(edge1);
					var DdDiff = _line.Direction.Dot(diff);
					LineParam = (b1 * DdE0) + (b2 * DdE1) - DdDiff;

					// Barycentric coordinates for the point of intersection.
					TriangleBaryCoords = new Vector3d(b0, b1, b2);

					// The intersection point is inside or on the triangle.
					LineClosest = _line.Origin + (LineParam * _line.Direction);
					TriangleClosest = _triangle.V0 + (b1 * edge0) + (b2 * edge1);
					DistanceSquared = 0;
					return 0;
				}
			}

			// Either (1) the line is not parallel to the triangle and the point of
			// intersection of the line and the plane of the triangle is outside the
			// triangle or (2) the line and triangle are parallel.  Regardless, the
			// closest point on the triangle is on an edge of the triangle.  Compare
			// the line to all three edges of the triangle.
			var sqrDist = double.MaxValue;
			for (int i0 = 2, i1 = 0; i1 < 3; i0 = i1++) {
				var segment = new Segment3d(_triangle[i0], _triangle[i1]);
				var queryLS = new DistLine3Segment3(_line, segment);
				var sqrDistTmp = queryLS.GetSquared();
				if (sqrDistTmp < sqrDist) {
					LineClosest = queryLS.LineClosest;
					TriangleClosest = queryLS.SegmentClosest;
					sqrDist = sqrDistTmp;
					LineParam = queryLS.LineParameter;
					var ratio = queryLS.SegmentParameter / segment.Extent;
					TriangleBaryCoords = Vector3d.Zero;
					TriangleBaryCoords[i0] = 0.5 * (1 - ratio);
					TriangleBaryCoords[i1] = 1 - TriangleBaryCoords[i0];
					TriangleBaryCoords[3 - i0 - i1] = 0;
				}
			}

			DistanceSquared = sqrDist;
			return DistanceSquared;
		}
	}
}
