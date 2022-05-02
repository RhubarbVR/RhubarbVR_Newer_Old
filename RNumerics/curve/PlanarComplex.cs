using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RNumerics
{

	public struct ComplexSegment2d
	{
		public Segment2d seg;
		public bool isClosed;
		public PlanarComplex.Element element;
	}
	public struct ComplexEndpoint2d
	{
		public Vector2d v;
		public bool isStart;
		public PlanarComplex.SmoothCurveElement element;
	}


	public class PlanarComplex
	{
		// these determine pointwise sampling rates
		public double DistanceAccuracy = 0.1;
		public double AngleAccuracyDeg = 5.0;
		public double SpacingT = 0.01;      // for curves where we don't know arc length
		public bool MinimizeSampling = false;   // if true, we don't subsample straight lines

		int _id_generator = 1;

		public abstract class Element
		{
			public IParametricCurve2d source;
			public int ID = 0;

			Colorf _color = Colorf.Black;

			public Colorf Color
			{
				get => _color;
				set { _color = value; HasSetColor = true; }
			}
			public bool HasSetColor { get; private set; } = false;

			protected void Copy_to(Element new_element) {
				new_element.ID = ID;
				new_element._color = _color;
				new_element.HasSetColor = HasSetColor;
				if (source != null) {
					new_element.source = source.Clone();
				}
			}

			public abstract IEnumerable<Segment2d> SegmentItr();
			public abstract AxisAlignedBox2d Bounds();
			public abstract Element Clone();
		}

		public class SmoothCurveElement : Element
		{
			public PolyLine2d polyLine;

			public override IEnumerable<Segment2d> SegmentItr() {
				return polyLine.SegmentItr();
			}
			public override AxisAlignedBox2d Bounds() {
				return polyLine.GetBounds();
			}

			public override Element Clone() {
				var curve = new SmoothCurveElement();
				Copy_to(curve);
				curve.polyLine = (polyLine == source) ? curve.source as PolyLine2d : new PolyLine2d(polyLine);
				return curve;
			}
		}

		public class SmoothLoopElement : Element
		{
			public Polygon2d polygon;

			public override IEnumerable<Segment2d> SegmentItr() {
				return polygon.SegmentItr();
			}
			public override AxisAlignedBox2d Bounds() {
				return polygon.GetBounds();
			}

			public override Element Clone() {
				var loop = new SmoothLoopElement();
				Copy_to(loop);
				loop.polygon = (polygon == source) ? loop.source as Polygon2d : new Polygon2d(polygon);
				return loop;
			}
		}




		List<Element> _vElements;


		public PlanarComplex() {
			_vElements = new List<Element>();
		}


		public int ElementCount => _vElements.Count;

		public Element Add(IParametricCurve2d curve) {
			if (curve.IsClosed) {
				var e = new SmoothLoopElement {
					ID = _id_generator++,
					source = curve
				};
				UpdateSampling(e);
				_vElements.Add(e);
				return e;
			}
			else {
				var e = new SmoothCurveElement {
					ID = _id_generator++,
					source = curve
				};
				UpdateSampling(e);
				_vElements.Add(e);
				return e;
			}
		}


		public Element Add(Polygon2d poly) {
			var e = new SmoothLoopElement {
				ID = _id_generator++,
				source = new Polygon2DCurve() { Polygon = poly },
				polygon = new Polygon2d(poly)
			};
			_vElements.Add(e);
			return e;
		}


		public Element Add(PolyLine2d pline) {
			var e = new SmoothCurveElement {
				ID = _id_generator++,
				source = new PolyLine2DCurve() { Polyline = pline },
				polyLine = new PolyLine2d(pline)
			};
			_vElements.Add(e);
			return e;
		}


		public void Remove(Element e) {
			_vElements.Remove(e);
		}


		void UpdateSampling(SmoothCurveElement c) {
			if (MinimizeSampling && c.source is Segment2d d) {
				c.polyLine = new PolyLine2d();
				c.polyLine.AppendVertex(d.P0);
				c.polyLine.AppendVertex(d.P1);
			}
			else {
				c.polyLine = new PolyLine2d(
					CurveSampler2.AutoSample(c.source, DistanceAccuracy, SpacingT));
			}
		}
		void UpdateSampling(SmoothLoopElement l) {
			l.polygon = new Polygon2d(
				CurveSampler2.AutoSample(l.source, DistanceAccuracy, SpacingT));
		}


		public void Reverse(SmoothCurveElement c) {
			c.source.Reverse();
			UpdateSampling(c);
		}



		public IEnumerable<ComplexSegment2d> AllSegmentsItr() {
			foreach (var e in _vElements) {
				var s = new ComplexSegment2d();
				if (e is SmoothLoopElement) {
					s.isClosed = true;
				}
				else if (e is SmoothCurveElement) {
					s.isClosed = false;
				}

				foreach (var seg in e.SegmentItr()) {
					s.seg = seg;
					s.element = e;
					yield return s;
				}
			}
		}


		public IEnumerable<Element> ElementsItr() {
			foreach (var e in _vElements) {
				yield return e;
			}
		}
		public IEnumerable<SmoothLoopElement> LoopsItr() {
			foreach (var e in _vElements) {
				if (e is SmoothLoopElement) {
					yield return e as SmoothLoopElement;
				}
			}
		}
		public IEnumerable<SmoothCurveElement> CurvesItr() {
			foreach (var e in _vElements) {
				if (e is SmoothCurveElement) {
					yield return e as SmoothCurveElement;
				}
			}
		}

		public bool HasOpenCurves() {
			foreach (var e in _vElements) {
				if (e is SmoothCurveElement) {
					return true;
				}
			}
			return false;
		}



		/// <summary>
		/// iterate through "leaf" curves, ie all the IParametricCurve2D's 
		/// embedded in loops that do not contain any child curves
		/// </summary>
		public IEnumerable<IParametricCurve2d> LoopLeafComponentsItr() {
			foreach (var e in _vElements) {
				if (e is SmoothLoopElement) {
					var source = e.source;
					if (source is IMultiCurve2d) {
						foreach (var c in CurveUtils2.LeafCurvesIteration(source)) {
							yield return c;
						}
					}
					else {
						yield return source;
					}
				}
			}
		}

		// iterate through endpoints of open curves
		public IEnumerable<ComplexEndpoint2d> EndpointsItr() {
			foreach (var e in _vElements) {
				if (e is SmoothCurveElement) {
					var s = e as SmoothCurveElement;
					yield return new ComplexEndpoint2d() {
						v = s.polyLine.Start,
						isStart = true,
						element = s
					};
					yield return new ComplexEndpoint2d() {
						v = s.polyLine.End,
						isStart = false,
						element = s
					};
				}
			}
		}



		public AxisAlignedBox2d Bounds() {
			var box = AxisAlignedBox2d.Empty;
			foreach (var e in _vElements) {
				box.Contain(e.Bounds());
			}
			return box;
		}




		public void SplitAllLoops() {
			var vRemove = new List<Element>();
			var vAdd = new List<IParametricCurve2d>();

			foreach (var loop in LoopsItr()) {
				if (loop.source is IMultiCurve2d) {
					vRemove.Add(loop);
					Find_sub_elements(loop.source as IMultiCurve2d, vAdd);
				}
			}

			foreach (var e in vRemove) {
				Remove(e);
			}

			foreach (var c in vAdd) {
				Add(c);
			}
		}
		private void Find_sub_elements(IMultiCurve2d multicurve, List<IParametricCurve2d> vAdd) {
			foreach (var curve in multicurve.Curves) {
				if (curve is IMultiCurve2d) {
					Find_sub_elements(curve as IMultiCurve2d, vAdd);
				}
				else {
					vAdd.Add(curve);
				}
			}
		}




		public bool JoinElements(ComplexEndpoint2d a, ComplexEndpoint2d b, double loop_tolerance = MathUtil.ZERO_TOLERANCE) {
			if (a.element == b.element) {
				throw new Exception("PlanarComplex.ChainElements: same curve!!");
			}

			var c1 = a.element;
			var c2 = b.element;

			SmoothCurveElement joined = null;
			if (a.isStart == false && b.isStart == true) {
				_vElements.Remove(c2);
				Append(c1, c2);
				joined = c1;
			}
			else if (a.isStart == true && b.isStart == false) {
				_vElements.Remove(c1);
				Append(c2, c1);
				joined = c2;
			}
			else if (a.isStart == false) {       // end-to-end join
				c2.source.Reverse();
				_vElements.Remove(c2);
				Append(c1, c2);
				joined = c1;
			}
			else if (a.isStart == true) {       // start-to-start join
				c1.source.Reverse();
				_vElements.Remove(c2);
				Append(c1, c2);
				joined = c1;
			}

			if (joined != null) {
				// check if we have closed a loop
				var dDelta = (joined.polyLine.Start - joined.polyLine.End).Length;
				if (dDelta < loop_tolerance) {

					// should always be one of these since we constructed it in append()
					(joined.source as ParametricCurveSequence2).IsClosed = joined.source is ParametricCurveSequence2
						? true
						: throw new Exception("PlanarComplex.JoinElements: we have closed a loop but it is not a parametric seq??");

					var loop = new SmoothLoopElement() {
						ID = _id_generator++,
						source = joined.source
					};
					_vElements.Remove(joined);
					_vElements.Add(loop);
					UpdateSampling(loop);
				}
				return true;
			}

			return false;
		}




		public void ConvertToLoop(SmoothCurveElement curve, double tolerance = MathUtil.ZERO_TOLERANCE) {
			var dDelta = (curve.polyLine.Start - curve.polyLine.End).Length;
			if (dDelta < tolerance) {

				// handle degenerate element case
				if (curve.polyLine.VertexCount == 2) {
					_vElements.Remove(curve);
					return;
				}

				// should always be one of these since we constructed it in append()
				(curve.source as ParametricCurveSequence2).IsClosed = curve.source is ParametricCurveSequence2
					? true
					: throw new Exception("PlanarComplex.ConvertToLoop: we have closed a loop but it is not a parametric seq??");

				var loop = new SmoothLoopElement() {
					ID = _id_generator++,
					source = curve.source
				};
				_vElements.Remove(curve);
				_vElements.Add(loop);
				UpdateSampling(loop);
			}
		}



		void Append(SmoothCurveElement cTo, SmoothCurveElement cAppend) {
			ParametricCurveSequence2 use;
			if (cTo.source is ParametricCurveSequence2) {
				use = cTo.source as ParametricCurveSequence2;
			}
			else {
				use = new ParametricCurveSequence2();
				use.Append(cTo.source);
			}

			if (cAppend.source is ParametricCurveSequence2) {
				var cseq = cAppend.source as ParametricCurveSequence2;
				foreach (var c in cseq.Curves) {
					use.Append(c);
				}
			}
			else {
				use.Append(cAppend.source);
			}

			cTo.source = use;
			UpdateSampling(cTo);
		}



		public class GeneralSolid
		{
			public Element Outer;
			public List<Element> Holes = new();
		}

		public class SolidRegionInfo
		{
			public List<GeneralPolygon2d> Polygons;
			public List<PlanarSolid2d> Solids;

			// map from polygon solids back to element(s) they came from
			public List<GeneralSolid> PolygonsSources;

			public AxisAlignedBox2d Bounds
			{
				get {
					var bounds = AxisAlignedBox2d.Empty;
					foreach (var p in Polygons) {
						bounds.Contain(p.Bounds);
					}

					return bounds;
				}
			}


			public double Area
			{
				get {
					double area = 0;
					foreach (var p in Polygons) {
						area += p.Area;
					}

					return area;
				}
			}


			public double HolesArea
			{
				get {
					double area = 0;
					foreach (var p in Polygons) {
						foreach (var h in p.Holes) {
							area += Math.Abs(h.SignedArea);
						}
					}
					return area;
				}
			}
		}



		public struct FindSolidsOptions
		{
			public double SimplifyDeviationTolerance;
			public bool WantCurveSolids;
			public bool TrustOrientations;
			public bool AllowOverlappingHoles;

			public static readonly FindSolidsOptions Default = new() {
				SimplifyDeviationTolerance = 0.1,
				WantCurveSolids = true,
				TrustOrientations = false,
				AllowOverlappingHoles = false
			};

			public static readonly FindSolidsOptions SortPolygons = new() {
				SimplifyDeviationTolerance = 0.0,
				WantCurveSolids = false,
				TrustOrientations = true,
				AllowOverlappingHoles = false
			};
		}


		public SolidRegionInfo FindSolidRegions(double fSimplifyDeviationTol = 0.1, bool bWantCurveSolids = true) {
			var opt = FindSolidsOptions.Default;
			opt.SimplifyDeviationTolerance = fSimplifyDeviationTol;
			opt.WantCurveSolids = bWantCurveSolids;
			return FindSolidRegions(opt);
		}

		// Finds set of "solid" regions - eg boundary loops with interior holes.
		// Result has outer loops being clockwise, and holes counter-clockwise
		public SolidRegionInfo FindSolidRegions(FindSolidsOptions options) {
			var validLoops = new List<SmoothLoopElement>(LoopsItr());
			var N = validLoops.Count;

			// precompute bounding boxes
			var maxid = 0;
			foreach (var v in validLoops) {
				maxid = Math.Max(maxid, v.ID + 1);
			}

			var bounds = new AxisAlignedBox2d[maxid];
			foreach (var v in validLoops) {
				bounds[v.ID] = v.Bounds();
			}

			// copy polygons, simplify if desired
			var fClusterTol = 0.0;       // don't do simple clustering, can lose corners
			var fDeviationTol = options.SimplifyDeviationTolerance;
			var polygons = new Polygon2d[maxid];
			foreach (var v in validLoops) {
				var p = new Polygon2d(v.polygon);
				if (fClusterTol > 0 || fDeviationTol > 0) {
					p.Simplify(fClusterTol, fDeviationTol);
				}

				polygons[v.ID] = p;
			}

			// sort by bbox containment to speed up testing (does it??)
			validLoops.Sort((x, y) => bounds[x.ID].Contains(bounds[y.ID]) ? -1 : 1);

			// containment sets
			var bIsContained = new bool[N];
			var ContainSets = new Dictionary<int, List<int>>();
			var ContainedParents = new Dictionary<int, List<int>>();

			var bUseOrient = options.TrustOrientations;
			var bWantCurveSolids = options.WantCurveSolids;
			var bCheckHoles = !options.AllowOverlappingHoles;

			// construct containment sets
			for (var i = 0; i < N; ++i) {
				var loopi = validLoops[i];
				var polyi = polygons[loopi.ID];

				for (var j = 0; j < N; ++j) {
					if (i == j) {
						continue;
					}

					var loopj = validLoops[j];
					var polyj = polygons[loopj.ID];

					// if we are preserving orientations, holes cannot contain holes and
					// outers cannot contain outers!
					if (bUseOrient && loopj.polygon.IsClockwise == loopi.polygon.IsClockwise) {
						continue;
					}

					// cannot be contained if bounds are not contained
					if (bounds[loopi.ID].Contains(bounds[loopj.ID]) == false) {
						continue;
					}

					// any other early-outs??

					if (polyi.Contains(polyj)) {
						if (ContainSets.ContainsKey(i) == false) {
							ContainSets.Add(i, new List<int>());
						}

						ContainSets[i].Add(j);
						bIsContained[j] = true;

						if (ContainedParents.ContainsKey(j) == false) {
							ContainedParents.Add(j, new List<int>());
						}

						ContainedParents[j].Add(i);
					}

				}
			}

			var polysolids = new List<GeneralPolygon2d>();
			var polySolidsInfo = new List<GeneralSolid>();

			var solids = new List<PlanarSolid2d>();
			var used = new HashSet<SmoothLoopElement>();

			var LoopToOuterIndex = new Dictionary<SmoothLoopElement, int>();

			var ParentsToProcess = new List<int>();


			// The following is a lot of code but it is very similar, just not clear how
			// to refactor out the common functionality
			//   1) we find all the top-level uncontained polys and add them to the final polys list
			//   2a) for any poly contained in those parent-polys, that is not also contained in anything else,
			//       add as hole to that poly
			//   2b) remove all those used parents & holes from consideration
			//   2c) now find all the "new" top-level polys
			//   3) repeat 2a-c until done all polys
			//   4) any remaining polys must be interior solids w/ no holes
			//          **or** weird leftovers like intersecting polys...

			// add all top-level uncontained polys
			for (var i = 0; i < N; ++i) {
				var loopi = validLoops[i];
				if (bIsContained[i]) {
					continue;
				}

				var outer_poly = polygons[loopi.ID];
				var outer_loop = bWantCurveSolids ? loopi.source.Clone() : null;
				if (outer_poly.IsClockwise == false) {
					outer_poly.Reverse();
					if (bWantCurveSolids) {
						outer_loop.Reverse();
					}
				}

				var g = new GeneralPolygon2d {
					Outer = outer_poly
				};
				var s = new PlanarSolid2d();
				if (bWantCurveSolids) {
					s.SetOuter(outer_loop);
				}

				var idx = polysolids.Count;
				LoopToOuterIndex[loopi] = idx;
				used.Add(loopi);

				if (ContainSets.ContainsKey(i)) {
					ParentsToProcess.Add(i);
				}

				polysolids.Add(g);
				polySolidsInfo.Add(new GeneralSolid() { Outer = loopi });
				if (bWantCurveSolids) {
					solids.Add(s);
				}
			}


			// keep iterating until we processed all parent loops
			while (ParentsToProcess.Count > 0) {

				var ContainersToRemove = new List<int>();

				// now for all top-level polys that contain children, add those children
				// as long as they do not have multiple contain-parents
				foreach (var i in ParentsToProcess) {
					var parentloop = validLoops[i];
					var outer_idx = LoopToOuterIndex[parentloop];

					var children = ContainSets[i];
					foreach (var childj in children) {
						var childLoop = validLoops[childj];
						Debug.Assert(used.Contains(childLoop) == false);

						// skip multiply-contained children
						var parents = ContainedParents[childj];
						if (parents.Count > 1) {
							continue;
						}

						var hole_poly = polygons[childLoop.ID];
						var hole_loop = bWantCurveSolids ? childLoop.source.Clone() : null;
						if (hole_poly.IsClockwise) {
							hole_poly.Reverse();
							if (bWantCurveSolids) {
								hole_loop.Reverse();
							}
						}

						try {
							polysolids[outer_idx].AddHole(hole_poly, bCheckHoles);
							polySolidsInfo[outer_idx].Holes.Add(childLoop);
							if (hole_loop != null) {
								solids[outer_idx].AddHole(hole_loop);
							}
						}
						catch {
							// don't add this hole - must intersect or something?
							// We should have caught this earlier!
						}

						used.Add(childLoop);
						if (ContainSets.ContainsKey(childj)) {
							ContainersToRemove.Add(childj);
						}
					}
					ContainersToRemove.Add(i);
				}

				// remove all containers that are no longer valid
				foreach (var ci in ContainersToRemove) {
					ContainSets.Remove(ci);

					// have to remove from each ContainedParents list
					var keys = new List<int>(ContainedParents.Keys);
					foreach (var j in keys) {
						if (ContainedParents[j].Contains(ci)) {
							ContainedParents[j].Remove(ci);
						}
					}
				}

				ParentsToProcess.Clear();

				// ok now find next-level uncontained parents...
				for (var i = 0; i < N; ++i) {
					var loopi = validLoops[i];
					if (used.Contains(loopi)) {
						continue;
					}

					if (ContainSets.ContainsKey(i) == false) {
						continue;
					}

					var parents = ContainedParents[i];
					if (parents.Count > 0) {
						continue;
					}

					var outer_poly = polygons[loopi.ID];
					var outer_loop = bWantCurveSolids ? loopi.source.Clone() : null;
					if (outer_poly.IsClockwise == false) {
						outer_poly.Reverse();
						if (bWantCurveSolids) {
							outer_loop.Reverse();
						}
					}

					var g = new GeneralPolygon2d {
						Outer = outer_poly
					};
					var s = new PlanarSolid2d();
					if (bWantCurveSolids) {
						s.SetOuter(outer_loop);
					}

					var idx = polysolids.Count;
					LoopToOuterIndex[loopi] = idx;
					used.Add(loopi);

					if (ContainSets.ContainsKey(i)) {
						ParentsToProcess.Add(i);
					}

					polysolids.Add(g);
					polySolidsInfo.Add(new GeneralSolid() { Outer = loopi });
					if (bWantCurveSolids) {
						solids.Add(s);
					}
				}
			}


			// any remaining loops must be top-level
			for (var i = 0; i < N; ++i) {
				var loopi = validLoops[i];
				if (used.Contains(loopi)) {
					continue;
				}

				var outer_poly = polygons[loopi.ID];
				var outer_loop = bWantCurveSolids ? loopi.source.Clone() : null;
				if (outer_poly.IsClockwise == false) {
					outer_poly.Reverse();
					if (bWantCurveSolids) {
						outer_loop.Reverse();
					}
				}

				var g = new GeneralPolygon2d {
					Outer = outer_poly
				};
				var s = new PlanarSolid2d();
				if (bWantCurveSolids) {
					s.SetOuter(outer_loop);
				}

				polysolids.Add(g);
				polySolidsInfo.Add(new GeneralSolid() { Outer = loopi });
				if (bWantCurveSolids) {
					solids.Add(s);
				}
			}



			return new SolidRegionInfo() {
				Polygons = polysolids,
				PolygonsSources = polySolidsInfo,
				Solids = bWantCurveSolids ? solids : null
			};
		}





		public class ClosedLoopsInfo
		{
			public List<Polygon2d> Polygons;
			public List<IParametricCurve2d> Loops;


			public AxisAlignedBox2d Bounds
			{
				get {
					var bounds = AxisAlignedBox2d.Empty;
					foreach (var p in Polygons) {
						bounds.Contain(p.GetBounds());
					}

					return bounds;
				}
			}
		}
		// returns set of closed loops (not necessarily solids)
		public ClosedLoopsInfo FindClosedLoops(double fSimplifyDeviationTol = 0.1) {
			var loopElems = new List<SmoothLoopElement>(LoopsItr());
			var maxid = 0;
			foreach (var v in loopElems) {
				maxid = Math.Max(maxid, v.ID + 1);
			}

			// copy polygons, simplify if desired
			var fClusterTol = 0.0;       // don't do simple clustering, can lose corners
			var fDeviationTol = fSimplifyDeviationTol;
			var polygons = new Polygon2d[maxid];
			var curves = new IParametricCurve2d[maxid];
			foreach (var v in loopElems) {
				var p = new Polygon2d(v.polygon);
				if (fClusterTol > 0 || fDeviationTol > 0) {
					p.Simplify(fClusterTol, fDeviationTol);
				}

				polygons[v.ID] = p;
				curves[v.ID] = v.source;
			}

			var ci = new ClosedLoopsInfo() {
				Polygons = new List<Polygon2d>(),
				Loops = new List<IParametricCurve2d>()
			};

			for (var i = 0; i < polygons.Length; ++i) {
				if (polygons[i] != null && polygons[i].VertexCount > 0) {
					ci.Polygons.Add(polygons[i]);
					ci.Loops.Add(curves[i]);
				}
			}

			return ci;
		}







		public class OpenCurvesInfo
		{
			public List<PolyLine2d> Polylines;
			public List<IParametricCurve2d> Curves;


			public AxisAlignedBox2d Bounds
			{
				get {
					var bounds = AxisAlignedBox2d.Empty;
					foreach (var p in Polylines) {
						bounds.Contain(p.GetBounds());
					}

					return bounds;
				}
			}
		}
		// returns set of open curves (ie non-solids)
		public OpenCurvesInfo FindOpenCurves(double fSimplifyDeviationTol = 0.1) {
			var curveElems = new List<SmoothCurveElement>(CurvesItr());

			var maxid = 0;
			foreach (var v in curveElems) {
				maxid = Math.Max(maxid, v.ID + 1);
			}

			// copy polygons, simplify if desired
			var fClusterTol = 0.0;       // don't do simple clustering, can lose corners
			var fDeviationTol = fSimplifyDeviationTol;
			var polylines = new PolyLine2d[maxid];
			var curves = new IParametricCurve2d[maxid];
			foreach (var v in curveElems) {
				var p = new PolyLine2d(v.polyLine);
				if (fClusterTol > 0 || fDeviationTol > 0) {
					p.Simplify(fClusterTol, fDeviationTol);
				}

				polylines[v.ID] = p;
				curves[v.ID] = v.source;
			}

			var ci = new OpenCurvesInfo() {
				Polylines = new List<PolyLine2d>(),
				Curves = new List<IParametricCurve2d>()
			};

			for (var i = 0; i < polylines.Length; ++i) {
				if (polylines[i] != null && polylines[i].VertexCount > 0) {
					ci.Polylines.Add(polylines[i]);
					ci.Curves.Add(curves[i]);
				}
			}

			return ci;
		}






		public PlanarComplex Clone() {
			var clone = new PlanarComplex {
				DistanceAccuracy = DistanceAccuracy,
				AngleAccuracyDeg = AngleAccuracyDeg,
				SpacingT = SpacingT,
				MinimizeSampling = MinimizeSampling,
				_id_generator = _id_generator,

				_vElements = new List<Element>(_vElements.Count)
			};
			foreach (var element in _vElements) {
				clone._vElements.Add(element.Clone());
			}

			return clone;
		}




		public void Append(PlanarComplex append) {
			foreach (var element in append._vElements) {
				element.ID = _id_generator++;
				_vElements.Add(element);
			}

			// clear elements in other so we don't make any mistakes...
			append._vElements.Clear();
		}



		public void Transform(ITransform2 xform, bool bApplyToSources, bool bRecomputePolygons = false) {
			foreach (var element in _vElements) {
				if (element is SmoothLoopElement) {
					var loop = element as SmoothLoopElement;
					if (bApplyToSources && loop.source != loop.polygon) {
						loop.source.Transform(xform);
					}

					if (bRecomputePolygons) {
						UpdateSampling(loop);
					}
					else {
						loop.polygon.Transform(xform);
					}
				}
				else if (element is SmoothCurveElement) {
					var curve = element as SmoothCurveElement;
					if (bApplyToSources && curve.source != curve.polyLine) {
						curve.source.Transform(xform);
					}

					if (bRecomputePolygons) {
						UpdateSampling(curve);
					}
					else {
						curve.polyLine.Transform(xform);
					}
				}
			}
		}









		public void PrintStats(string label = "") {
			System.Console.WriteLine("PlanarComplex Stats {0}", label);
			var Loops = new List<SmoothLoopElement>(LoopsItr());
			var Curves = new List<SmoothCurveElement>(CurvesItr());

			var bounds = Bounds();
			System.Console.WriteLine("  Bounding Box  w: {0} h: {1}  range {2} ", bounds.Width, bounds.Height, bounds);

			var vEndpoints = new List<ComplexEndpoint2d>(EndpointsItr());
			System.Console.WriteLine("  Closed Loops {0}  Open Curves {1}   Open Endpoints {2}",
				Loops.Count, Curves.Count, vEndpoints.Count);

			var nSegments = CountType(typeof(Segment2d));
			var nArcs = CountType(typeof(Arc2d));
			var nCircles = CountType(typeof(Circle2d));
			var nNURBS = CountType(typeof(NURBSCurve2));
			var nEllipses = CountType(typeof(Ellipse2d));
			var nEllipseArcs = CountType(typeof(EllipseArc2d));
			var nSeqs = CountType(typeof(ParametricCurveSequence2));
			System.Console.WriteLine("  [Type Counts]   // {0} multi-curves", nSeqs);
			System.Console.WriteLine("    segments {0,4}  arcs     {1,4}  circles      {2,4}", nSegments, nArcs, nCircles);
			System.Console.WriteLine("    nurbs    {0,4}  ellipses {1,4}  ellipse-arcs {2,4}", nNURBS, nEllipses, nEllipseArcs);
		}
		public int CountType(Type t) {
			var count = 0;
			foreach (var loop in _vElements) {
				if (loop.source.GetType() == t) {
					count++;
				}

				if (loop.source is IMultiCurve2d) {
					count += CountType(loop.source as IMultiCurve2d, t);
				}
			}
			return count;
		}
		public int CountType(IMultiCurve2d curve, Type t) {
			var count = 0;
			foreach (var c in curve.Curves) {
				if (c.GetType() == t) {
					count++;
				}

				if (c is IMultiCurve2d) {
					count += CountType(c as IMultiCurve2d, t);
				}
			}
			return count;
		}

	}
}
