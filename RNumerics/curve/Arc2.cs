﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RNumerics
{

	public sealed class Arc2d : IParametricCurve2d
	{
		public Vector2d Center;
		public double Radius;
		public double AngleStartDeg;
		public double AngleEndDeg;
		public bool IsReversed;     // use ccw orientation instead of cw


		public Arc2d(in Vector2d center, in double radius, in double startDeg, in double endDeg) {
			IsReversed = false;
			Center = center;
			Radius = radius;
			AngleStartDeg = startDeg;
			AngleEndDeg = endDeg;
			if (AngleEndDeg < AngleStartDeg) {
				AngleEndDeg += 360;
			}

			// [TODO] handle full arcs, which should be circles?
		}


		/// <summary>
		/// Create Arc around center, **clockwise** from start to end points.
		/// Points must both be the same distance from center (ie on circle)
		/// </summary>
		public Arc2d(in Vector2d vCenter, in Vector2d vStart, in Vector2d vEnd) {
			IsReversed = false;
			SetFromCenterAndPoints(vCenter, vStart, vEnd);
		}


		/// <summary>
		/// Initialize Arc around center, **clockwise** from start to end points.
		/// Points must both be the same distance from center (ie on circle)
		/// </summary>
		public void SetFromCenterAndPoints(in Vector2d vCenter, in Vector2d vStart, in Vector2d vEnd) {
			var ds = vStart - vCenter;
			var de = vEnd - vCenter;
			Debug.Assert(Math.Abs(ds.LengthSquared - de.LengthSquared) < MathUtil.ZERO_TOLERANCEF);
			AngleStartDeg = Math.Atan2(ds.y, ds.x) * MathUtil.RAD_2_DEG;
			AngleEndDeg = Math.Atan2(de.y, de.x) * MathUtil.RAD_2_DEG;
			if (AngleEndDeg < AngleStartDeg) {
				AngleEndDeg += 360;
			}

			Center = vCenter;
			Radius = ds.Length;
		}



		public Vector2d P0 => SampleT(0.0);
		public Vector2d P1 => SampleT(1.0);

		public double Curvature => 1.0 / Radius;
		public double SignedCurvature => IsReversed ? (-1.0 / Radius) : (1.0 / Radius);

		public bool IsClosed => false;


		public double ParamLength => 1.0f;


		// t in range[0,1] spans arc
		public Vector2d SampleT(in double t) {
			var theta = IsReversed ?
				((1 - t) * AngleEndDeg) + (t * AngleStartDeg) :
				((1 - t) * AngleStartDeg) + (t * AngleEndDeg);
			theta *= MathUtil.DEG_2_RAD;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
		}


		public Vector2d TangentT(in double t) {
			var theta = IsReversed ?
				((1 - t) * AngleEndDeg) + (t * AngleStartDeg) :
				((1 - t) * AngleStartDeg) + (t * AngleEndDeg);
			theta *= MathUtil.DEG_2_RAD;
			var tangent = new Vector2d(-Math.Sin(theta), Math.Cos(theta));
			if (IsReversed) {
				tangent = -tangent;
			}

			tangent.Normalize();
			return tangent;
		}


		public bool HasArcLength => true;

		public double ArcLength => (AngleEndDeg - AngleStartDeg) * MathUtil.DEG_2_RAD * Radius;

		public Vector2d SampleArcLength(in double a) {
			if (ArcLength < MathUtil.EPSILON) {
				return (a < 0.5) ? SampleT(0) : SampleT(1);
			}

			var t = a / ArcLength;
			var theta = IsReversed ?
				((1 - t) * AngleEndDeg) + (t * AngleStartDeg) :
				((1 - t) * AngleStartDeg) + (t * AngleEndDeg);
			theta *= MathUtil.DEG_2_RAD;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
		}

		public void Reverse() {
			IsReversed = !IsReversed;
		}

		public IParametricCurve2d Clone() {
			return new Arc2d(Center, Radius, AngleStartDeg, AngleEndDeg) { IsReversed = IsReversed };
		}


		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform) {
			var vCenter = xform.TransformP(Center);
			var vStart = xform.TransformP(IsReversed ? P1 : P0);
			var vEnd = xform.TransformP(IsReversed ? P0 : P1);

			SetFromCenterAndPoints(vCenter, vStart, vEnd);
		}



		public AxisAlignedBox2d Bounds
		{
			get {
				// extrema of arc are P0, P1, and any axis-crossings that lie in arc span.
				// We can compute bounds of axis-crossings in normalized space and then scale/translate.
				var k = (int)(AngleStartDeg / 90.0);
				if (k * 90 < AngleStartDeg) {
					k++;
				}

				var stop_k = (int)(AngleEndDeg / 90);
				if (stop_k * 90 > AngleEndDeg) {
					stop_k--;
				}
				// [TODO] we should only ever need to check at most 4 here, right? then we have gone a circle...
				var bounds = AxisAlignedBox2d.Empty;
				while (k <= stop_k) {
					var i = k++ % 4;
					bounds.Contain(_bounds_dirs[i]);
				}
				bounds.Scale(Radius);
				bounds.Translate(Center);
				bounds.Contain(P0);
				bounds.Contain(P1);
				return bounds;
			}
		}
		private static readonly Vector2d[] _bounds_dirs = new Vector2d[4] {
			Vector2d.AxisX, Vector2d.AxisY, -Vector2d.AxisX, -Vector2d.AxisY };




		public double Distance(in Vector2d point) {
			var PmC = point - Center;
			var lengthPmC = PmC.Length;
			if (lengthPmC > MathUtil.EPSILON) {
				var dv = PmC / lengthPmC;
				var theta = Math.Atan2(dv.y, dv.x) * MathUtil.RAD_2_DEG;
				if (!(theta >= AngleStartDeg && theta <= AngleEndDeg)) {
					var ctheta = MathUtil.ClampAngleDeg(theta, AngleStartDeg, AngleEndDeg);
					var radians = ctheta * MathUtil.DEG_2_RAD;
					double c = Math.Cos(radians), s = Math.Sin(radians);
					var pos = new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
					return pos.Distance(point);
				}
				else {
					return Math.Abs(lengthPmC - Radius);
				}
			}
			else {
				return Radius;
			}
		}


		public Vector2d NearestPoint(in Vector2d point) {
			var PmC = point - Center;
			var lengthPmC = PmC.Length;
			if (lengthPmC > MathUtil.EPSILON) {
				var dv = PmC / lengthPmC;
				var theta = Math.Atan2(dv.y, dv.x);
				theta *= MathUtil.RAD_2_DEG;
				theta = MathUtil.ClampAngleDeg(theta, AngleStartDeg, AngleEndDeg);
				theta = MathUtil.DEG_2_RAD * theta;
				double c = Math.Cos(theta), s = Math.Sin(theta);
				return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
			}
			else {
				return SampleT(0.5);        // all points equidistant
			}
		}


	}
}
