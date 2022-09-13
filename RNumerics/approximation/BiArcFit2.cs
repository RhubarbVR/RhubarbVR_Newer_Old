using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	//
	// 2D Biarc fitting ported from http://www.ryanjuckett.com/programming/biarc-interpolation/
	//
	//
	public sealed class BiArcFit2
	{
		public Vector2d Point1;
		public Vector2d Point2;
		public Vector2d Tangent1;
		public Vector2d Tangent2;
		// original code used 0.0001 here...
		public double Epsilon = MathUtil.ZERO_TOLERANCE;

		public Arc2d Arc1;
		public Arc2d Arc2;
		public bool Arc1IsSegment;
		public bool Arc2IsSegment;
		public Segment2d Segment1;
		public Segment2d Segment2;

		// these are the computed d1 and d2 parameters. By default you will,
		// get d1==d2, unless you specify d1 in the second constructor
		public double FitD1;
		public double FitD2;


		// compute standard biarc fit with d1==d2
		public BiArcFit2(in Vector2d point1, in Vector2d tangent1, in Vector2d point2, in Vector2d tangent2) {
			Point1 = point1;
			Tangent1 = tangent1;
			Point2 = point2;
			Tangent2 = tangent2;
			Fit();
			Set_output();
		}

		// advanced biarc fit with specified d1. Note that d1 can technically be any value, but
		// outside of some range you will get nonsense (like 2 full circles, etc). A reasonable
		// strategy is to compute the default fit first (d1==d2), then it seems like d1 can safely be in
		// the range [0,2*first_d1]. This will vary the length of the two arcs, and for some d1 you
		// will almost always get a better fit.
		public BiArcFit2(in Vector2d point1, in Vector2d tangent1, in Vector2d point2, in Vector2d tangent2, in double d1) {
			Point1 = point1;
			Tangent1 = tangent1;
			Point2 = point2;
			Tangent2 = tangent2;
			Fit(d1);
			Set_output();
		}


		void Set_output() {
			if (_arc1.IsSegment) {
				Arc1IsSegment = true;
				Segment1 = new Segment2d(_arc1.P0, _arc1.P1);
			}
			else {
				Arc1IsSegment = false;
				Arc1 = Get_arc(0);
			}

			if (_arc2.IsSegment) {
				Arc2IsSegment = true;
				Segment2 = new Segment2d(_arc2.P1, _arc2.P0);
			}
			else {
				Arc2IsSegment = false;
				Arc2 = Get_arc(1);
			}
		}



		public double Distance(in Vector2d point) {
			var d0 = Arc1IsSegment ?
				Math.Sqrt(Segment1.DistanceSquared(point)) : Arc1.Distance(point);
			var d1 = Arc2IsSegment ?
				Math.Sqrt(Segment2.DistanceSquared(point)) : Arc2.Distance(point);
			return Math.Min(d0, d1);
		}
		public Vector2d NearestPoint(in Vector2d point) {
			var n1 = Arc1IsSegment ?
				Segment1.NearestPoint(point) : Arc1.NearestPoint(point);
			var n2 = Arc2IsSegment ?
				Segment2.NearestPoint(point) : Arc2.NearestPoint(point);
			return (n1.DistanceSquared(point) < n2.DistanceSquared(point)) ? n1 : n2;
		}

		public List<IParametricCurve2d> Curves
		{
			get {
				var c1 = Arc1IsSegment ? Segment1 : (IParametricCurve2d)Arc1;
				var c2 = Arc2IsSegment ? Segment2 : (IParametricCurve2d)Arc2;
				return new List<IParametricCurve2d>() { c1, c2 };
			}
		}
		public IParametricCurve2d Curve1 => Arc1IsSegment ? Segment1 : (IParametricCurve2d)Arc1;
		public IParametricCurve2d Curve2 => Arc2IsSegment ? Segment2 : (IParametricCurve2d)Arc2;


		public struct Arc
		{
			public Vector2d Center;
			public double Radius;
			public double AngleStartR;
			public double AngleEndR;
			public bool PositiveRotation;
			public bool IsSegment;
			public Vector2d P0;
			public Vector2d P1;

			public Arc(in Vector2d c, in double r, in double startR, in double endR, in bool posRotation) {
				Center = c;
				Radius = r;
				AngleStartR = startR;
				AngleEndR = endR;
				PositiveRotation = posRotation;
				IsSegment = false;
				P0 = P1 = Vector2d.Zero;
			}

			public Arc(in Vector2d p0, in Vector2d p1) {
				Center = Vector2d.Zero;
				Radius = AngleStartR = AngleEndR = 0;
				PositiveRotation = false;
				IsSegment = true;
				P0 = p0;
				P1 = p1;
			}
		}

		Arc _arc1;
		Arc _arc2;

		void Set_arc(in int i, in Arc a) {
			if (i == 0) {
				_arc1 = a;
			}
			else {
				_arc2 = a;
			}
		}


		Arc2d Get_arc(in int i) {
			var a = (i == 0) ? _arc1 : _arc2;
			var start_deg = a.AngleStartR * MathUtil.RAD_2_DEG;
			var end_deg = a.AngleEndR * MathUtil.RAD_2_DEG;
			if (a.PositiveRotation == true) {
				(end_deg, start_deg) = (start_deg, end_deg);
			}
			var arc = new Arc2d(a.Center, a.Radius, start_deg, end_deg);

			// [RMS] code above does not preserve CW/CCW of arcs. 
			//  It would be better to fix that. But for now, just check if
			//  we preserved start and end points, and if not reverse curves.
			if (i == 0 && arc.SampleT(0.0).DistanceSquared(Point1) > MathUtil.ZERO_TOLERANCE) {
				arc.Reverse();
			}

			if (i == 1 && arc.SampleT(1.0).DistanceSquared(Point2) > MathUtil.ZERO_TOLERANCE) {
				arc.Reverse();
			}

			return arc;
		}




		// [TODO]
		//    - we could get a better fit to the original curve if we use the ability to have separate
		//      d1 and d2 values. There is a point where d1 > 0 and d2 > 0 where we will get a best-fit.
		//
		//      if d1==0, the first arc degenerates. The second arc degenerates when d2 == 0.
		//      If either d1 or d2 go negative, then we shouldn't use that result. 
		//      But it's not clear if we can directly compute the positive-d-range...
		//
		//      It does seem like if we solve the d1=d2 case, then we use [0,2*d1] as the d1 range,
		//      then maybe we are safe. And we can always discard solutions where it is negative...


		// solve biarc fit where the free parameter is automatically set so that
		// d1=d2, which is basically the 'middle' case
		void Fit() {
			// get inputs
			var p1 = Point1;
			var p2 = Point2;

			var t1 = Tangent1;
			var t2 = Tangent2;

			// fit biarc
			var v = p2 - p1;
			var vMagSqr = v.LengthSquared;

			// set d1 equal to d2
			var t = t1 + t2;
			var tMagSqr = t.LengthSquared;

			// original code used 0.0001 here...
			var equalTangents = MathUtil.EpsilonEqual(tMagSqr, 4.0, Epsilon);
			//var equalTangents = IsEqualEps(tMagSqr, 4.0);

			var vDotT1 = v.Dot(t1);
			var perpT1 = MathUtil.EpsilonEqual(vDotT1, 0.0, Epsilon);
			if (equalTangents && perpT1) {
				// we have two semicircles
				//Vector2d joint = p1 + 0.5 * v;

				// d1 = d2 = infinity here...
				FitD1 = FitD2 = double.PositiveInfinity;

				// draw arcs
				var angle = Math.Atan2(v.y, v.x);
				var center1 = p1 + (0.25 * v);
				var center2 = p1 + (0.75 * v);
				var radius = Math.Sqrt(vMagSqr) * 0.25;
				var cross = (v.x * t1.y) - (v.y * t1.x);

				_arc1 = new Arc(center1, radius, angle, angle + Math.PI, cross < 0);
				_arc1 = new Arc(center2, radius, angle, angle + Math.PI, cross > 0);

			}
			else {
				var vDotT = v.Dot(t);

				// [RMS] this was unused in original code...
				//bool perpT1 = MathUtil.EpsilonEqual(vDotT1, 0, epsilon);

				double d1;
				if (equalTangents) {
					d1 = vMagSqr / (4 * vDotT1);
				}
				else {
					var denominator = 2.0 - (2.0 * t1.Dot(t2));
					var discriminant = (vDotT * vDotT) + (denominator * vMagSqr);
					d1 = (Math.Sqrt(discriminant) - vDotT) / denominator;
				}
				FitD1 = FitD2 = d1;

				var joint = p1 + p2 + (d1 * (t1 - t2));
				joint *= 0.5;

				// construct arcs
				SetArcFromEdge(0, p1, t1, joint, true);
				SetArcFromEdge(1, p2, t2, joint, false);
			}

		}



		// This is a variant of Fit() where the d1 value is specified.
		// Note: has not been tested extensively, particularly the special case
		// where one of the arcs beomes a semi-circle...
		void Fit(in double d1) {

			var p1 = Point1;
			var p2 = Point2;

			var t1 = Tangent1;
			var t2 = Tangent2;

			// fit biarc
			var v = p2 - p1;
			var vMagSqr = v.LengthSquared;


			var vDotT1 = v.Dot(t1);

			var vDotT2 = v.Dot(t2);
			var t1DotT2 = t1.Dot(t2);
			var denominator = vDotT2 - (d1 * (t1DotT2 - 1.0));

			if (MathUtil.EpsilonEqual(denominator, 0.0, MathUtil.ZERO_TOLERANCEF)) {
				// the second arc is a semicircle

				FitD1 = d1;
				FitD2 = double.PositiveInfinity;

				var joint = p1 + (d1 * t1);
				joint += (vDotT2 - (d1 * t1DotT2)) * t2;

				// construct arcs
				// [TODO] this might not be right for semi-circle...
				SetArcFromEdge(0, p1, t1, joint, true);
				SetArcFromEdge(1, p2, t2, joint, false);

			}
			else {
				var d2 = ((0.5 * vMagSqr) - (d1 * vDotT1)) / denominator;
				var invLen = 1.0 / (d1 + d2);

				var joint = d1 * d2 * (t1 - t2);
				joint += d1 * p2;
				joint += d2 * p1;
				joint *= invLen;

				FitD1 = d1;
				FitD2 = d2;

				// draw arcs
				SetArcFromEdge(0, p1, t1, joint, true);
				SetArcFromEdge(1, p2, t2, joint, false);
			}


		}






		void SetArcFromEdge(in int i, in Vector2d p1, in Vector2d t1, in Vector2d p2, in bool fromP1) {
			var chord = p2 - p1;
			var n1 = new Vector2d(-t1.y, t1.x);
			var chordDotN1 = chord.Dot(n1);

			if (MathUtil.EpsilonEqual(chordDotN1, 0, Epsilon)) {
				// straight line case
				Set_arc(i, new Arc(p1, p2));

			}
			else {
				var radius = chord.LengthSquared / (2.0 * chordDotN1);
				var center = p1 + (radius * n1);

				var p1Offset = p1 - center;
				var p2Offset = p2 - center;

				var p1Ang1 = Math.Atan2(p1Offset.y, p1Offset.x);
				var p2Ang1 = Math.Atan2(p2Offset.y, p2Offset.x);
				if ((p1Offset.x * t1.y) - (p1Offset.y * t1.x) > 0) {
					Set_arc(i, new Arc(center, Math.Abs(radius), p1Ang1, p2Ang1, !fromP1));
				}
				else {
					Set_arc(i, new Arc(center, Math.Abs(radius), p1Ang1, p2Ang1, fromP1));
				}
			}
		}
	}
}
