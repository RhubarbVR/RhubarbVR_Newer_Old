using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5 
	// https://www.geometrictools.com/Downloads/Downloads.html

	public class DistSegment3Triangle3
	{
		Segment3d _segment;
		public Segment3d Segment
		{
			get => _segment;
			set { _segment = value; DistanceSquared = -1.0; }
		}

		Triangle3d _triangle;
		public Triangle3d Triangle
		{
			get => _triangle;
			set { _triangle = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d SegmentClosest;
		public double SegmentParam;
		public Vector3d TriangleClosest;
		public Vector3d TriangleBaryCoords;

		public DistSegment3Triangle3(Segment3d SegmentIn, Triangle3d TriangleIn) {
			_triangle = TriangleIn;
			_segment = SegmentIn;
		}


		public DistSegment3Triangle3 Compute() {
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

			var line = new Line3d(_segment.Center, _segment.Direction);
			var queryLT = new DistLine3Triangle3(line, _triangle);
			var sqrDist = queryLT.GetSquared();
			SegmentParam = queryLT.LineParam;

			if (SegmentParam >= -_segment.Extent) {
				if (SegmentParam <= _segment.Extent) {
					SegmentClosest = queryLT.LineClosest;
					TriangleClosest = queryLT.TriangleClosest;
					TriangleBaryCoords = queryLT.TriangleBaryCoords;
				}
				else {
					SegmentClosest = _segment.P1;
					var queryPT = new DistPoint3Triangle3(SegmentClosest, _triangle);
					sqrDist = queryPT.GetSquared();
					TriangleClosest = queryPT.TriangleClosest;
					SegmentParam = _segment.Extent;
					TriangleBaryCoords = queryPT.TriangleBaryCoords;
				}
			}
			else {
				SegmentClosest = _segment.P0;
				var queryPT = new DistPoint3Triangle3(SegmentClosest, _triangle);
				sqrDist = queryPT.GetSquared();
				TriangleClosest = queryPT.TriangleClosest;
				SegmentParam = -_segment.Extent;
				TriangleBaryCoords = queryPT.TriangleBaryCoords;
			}

			DistanceSquared = sqrDist;
			return DistanceSquared;
		}
	}
}
