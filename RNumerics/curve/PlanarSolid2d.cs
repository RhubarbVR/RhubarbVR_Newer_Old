using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RNumerics
{
	// This class is analogous to GeneralPolygon2d, but for closed loops of curves, instead
	// of polygons. However, we cannot do some of the operations we would otherwise do in
	// GeneralPolygon2d (for example cw/ccw checking, intersctions, etc).
	//
	// So, it is strongly recommended that this be constructed alongside a GeneralPolygon2d,
	// which can be used for checking everything.
	public sealed class PlanarSolid2d
	{
		//bool bOuterIsCW;

		readonly List<IParametricCurve2d> _holes = new();


		public PlanarSolid2d()
		{
		}


		public IParametricCurve2d Outer { get; private set; }
		public void SetOuter(in IParametricCurve2d loop) {
			Debug.Assert(loop.IsClosed);
			Outer = loop;
		}


		public void AddHole(in IParametricCurve2d hole)
		{
			if (Outer == null) {
				throw new Exception("PlanarSolid2d.AddHole: outer polygon not set!");
			}

			//        if ( (bOuterIsCW && hole.IsClockwise) || (bOuterIsCW == false && hole.IsClockwise == false) )
			//throw new Exception("PlanarSolid2d.AddHole: new hole has same orientation as outer polygon!");

			_holes.Add(hole);
		}

		public ReadOnlyCollection<IParametricCurve2d> Holes => _holes.AsReadOnly();


		public bool HasArcLength
		{
			get
			{
				var bHas = Outer.HasArcLength;
				foreach (var h in Holes) {
					bHas = bHas && h.HasArcLength;
				}

				return bHas;
			}
		}


		public double Perimeter
		{
			get
			{
				if (!HasArcLength) {
					throw new Exception("PlanarSolid2d.Perimeter: some curves do not have arc length");
				}

				var dPerim = Outer.ArcLength;
				foreach (var h in Holes) {
					dPerim += h.ArcLength;
				}

				return dPerim;
			}
		}


		/// <summary>
		/// Resample parametric solid into polygonal solid
		/// </summary>
		public GeneralPolygon2d Convert(in double fSpacingLength, in double fSpacingT, in double fDeviationTolerance)
		{
			var poly = new GeneralPolygon2d {
				Outer = new Polygon2d(
				CurveSampler2.AutoSample(Outer, fSpacingLength, fSpacingT))
			};
			poly.Outer.Simplify(0, fDeviationTolerance);
			foreach (var hole in Holes)
			{
				var holePoly = new Polygon2d(
					CurveSampler2.AutoSample(hole, fSpacingLength, fSpacingT));
				holePoly.Simplify(0, fDeviationTolerance);
				poly.AddHole(holePoly, false);
			}
			return poly;
		}
	}
}
