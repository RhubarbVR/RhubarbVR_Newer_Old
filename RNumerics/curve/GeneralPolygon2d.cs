using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RNumerics
{
	public class GeneralPolygon2d : IDuplicatable<GeneralPolygon2d>
	{
		Polygon2d _outer;
		bool _bOuterIsCW;
		readonly List<Polygon2d> _holes = new();


		public GeneralPolygon2d() {
		}
		public GeneralPolygon2d(Polygon2d outer) {
			Outer = outer;
		}
		public GeneralPolygon2d(GeneralPolygon2d copy) {
			_outer = new Polygon2d(copy._outer);
			_bOuterIsCW = copy._bOuterIsCW;
			_holes = new List<Polygon2d>();
			foreach (var hole in copy._holes) {
				_holes.Add(new Polygon2d(hole));
			}
		}

		public virtual GeneralPolygon2d Duplicate() {
			return new GeneralPolygon2d(this);
		}


		public Polygon2d Outer
		{
			get => _outer;
			set {
				_outer = value;
				_bOuterIsCW = _outer.IsClockwise;
			}
		}


		public void AddHole(Polygon2d hole, bool bCheckContainment = true, bool bCheckOrientation = true) {
			if (_outer == null) {
				throw new Exception("GeneralPolygon2d.AddHole: outer polygon not set!");
			}

			if (bCheckContainment) {
				if (_outer.Contains(hole) == false) {
					throw new Exception("GeneralPolygon2d.AddHole: outer does not contain hole!");
				}

				// [RMS] segment/segment intersection broken?
				foreach (var hole2 in _holes) {
					if (hole.Intersects(hole2)) {
						throw new Exception("GeneralPolygon2D.AddHole: new hole intersects existing hole!");
					}
				}
			}
			if (bCheckOrientation) {
				if ((_bOuterIsCW && hole.IsClockwise) || (_bOuterIsCW == false && hole.IsClockwise == false)) {
					throw new Exception("GeneralPolygon2D.AddHole: new hole has same orientation as outer polygon!");
				}
			}

			_holes.Add(hole);
		}

		public void ClearHoles() {
			_holes.Clear();
		}


		public ReadOnlyCollection<Polygon2d> Holes => _holes.AsReadOnly();



		public double Area
		{
			get {
				var sign = _bOuterIsCW ? -1.0 : 1.0;
				var dArea = sign * Outer.SignedArea;
				foreach (var h in _holes) {
					dArea += sign * h.SignedArea;
				}

				return dArea;
			}
		}


		public double HoleArea
		{
			get {
				double dArea = 0;
				foreach (var h in Holes) {
					dArea += Math.Abs(h.SignedArea);
				}

				return dArea;
			}
		}


		public double Perimeter
		{
			get {
				var dPerim = _outer.Perimeter;
				foreach (var h in _holes) {
					dPerim += h.Perimeter;
				}

				return dPerim;
			}
		}


		public AxisAlignedBox2d Bounds
		{
			get {
				var box = _outer.GetBounds();
				foreach (var h in _holes) {
					box.Contain(h.GetBounds());
				}

				return box;
			}
		}

		public int VertexCount
		{
			get {
				var NV = _outer.VertexCount;
				foreach (var h in _holes) {
					NV += h.VertexCount;
				}

				return NV;
			}
		}


		public void Translate(Vector2d translate) {
			_outer.Translate(translate);
			foreach (var h in _holes) {
				h.Translate(translate);
			}
		}

		public void Rotate(Matrix2d rotation, Vector2d origin) {
			_outer.Rotate(rotation, origin);
			foreach (var h in _holes) {
				h.Rotate(rotation, origin);
			}
		}


		public void Scale(Vector2d scale, Vector2d origin) {
			_outer.Scale(scale, origin);
			foreach (var h in _holes) {
				h.Scale(scale, origin);
			}
		}

		public void Transform(Func<Vector2d, Vector2d> transformF) {
			_outer.Transform(transformF);
			foreach (var h in _holes) {
				h.Transform(transformF);
			}
		}

		public void Reverse() {
			Outer.Reverse();
			_bOuterIsCW = Outer.IsClockwise;
			foreach (var h in Holes) {
				h.Reverse();
			}
		}


		public bool Contains(Vector2d vTest) {
			if (_outer.Contains(vTest) == false) {
				return false;
			}

			foreach (var h in _holes) {
				if (h.Contains(vTest)) {
					return false;
				}
			}
			return true;
		}


		public bool Contains(Polygon2d poly) {
			if (_outer.Contains(poly) == false) {
				return false;
			}

			foreach (var h in _holes) {
				if (h.Contains(poly)) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks that all points on a segment are within the area defined by the GeneralPolygon2d;
		/// holes are included in the calculation.
		/// </summary>
		public bool Contains(Segment2d seg) {
			if (_outer.Contains(seg) == false) {
				return false;
			}

			foreach (var h in _holes) {
				if (h.Intersects(seg)) {
					return false;
				}
			}
			return true;
		}


		public bool Intersects(Polygon2d poly) {
			if (_outer.Intersects(poly)) {
				return true;
			}

			foreach (var h in _holes) {
				if (h.Intersects(poly)) {
					return true;
				}
			}
			return false;
		}


		public Vector2d PointAt(int iSegment, double fSegT, int iHoleIndex = -1) {
			return iHoleIndex == -1 ? _outer.PointAt(iSegment, fSegT) : _holes[iHoleIndex].PointAt(iSegment, fSegT);
		}

		public Segment2d Segment(int iSegment, int iHoleIndex = -1) {
			return iHoleIndex == -1 ? _outer.Segment(iSegment) : _holes[iHoleIndex].Segment(iSegment);
		}

		public Vector2d GetNormal(int iSegment, double segT, int iHoleIndex = -1) {
			return iHoleIndex == -1 ? _outer.GetNormal(iSegment, segT) : _holes[iHoleIndex].GetNormal(iSegment, segT);
		}

		// this should be more efficient when there are holes...
		public double DistanceSquared(Vector2d p, out int iHoleIndex, out int iNearSeg, out double fNearSegT) {
			iHoleIndex = -1;
			var dist = _outer.DistanceSquared(p, out iNearSeg, out fNearSegT);
			for (var i = 0; i < Holes.Count; ++i) {
				var holedist = Holes[i].DistanceSquared(p, out var seg, out var segt);
				if (holedist < dist) {
					dist = holedist;
					iHoleIndex = i;
					iNearSeg = seg;
					fNearSegT = segt;
				}
			}
			return dist;
		}


		public IEnumerable<Segment2d> AllSegmentsItr() {
			foreach (var seg in _outer.SegmentItr()) {
				yield return seg;
			}

			foreach (var hole in _holes) {
				foreach (var seg in hole.SegmentItr()) {
					yield return seg;
				}
			}
		}

		public IEnumerable<Vector2d> AllVerticesItr() {
			foreach (var v in _outer.Vertices) {
				yield return v;
			}

			foreach (var hole in _holes) {
				foreach (var v in hole.Vertices) {
					yield return v;
				}
			}
		}
	}
}
