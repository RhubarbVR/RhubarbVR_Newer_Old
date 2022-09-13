using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5's  DistPoint3Circle3  (didn't have point2circle2)
	// https://www.geometrictools.com/Downloads/Downloads.html

	public sealed class DistPoint2Circle2
	{
		Vector2d _point;
		public Vector2d Point
		{
			get => _point;
			set { _point = value; DistanceSquared = -1.0; }
		}

		Circle2d _circle;
		public Circle2d Circle
		{
			get => _circle;
			set { _circle = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector2d CircleClosest;
		public bool AllCirclePointsEquidistant;


		public DistPoint2Circle2(in Vector2d PointIn, in Circle2d circleIn) {
			_point = PointIn;
			_circle = circleIn;
		}

		public DistPoint2Circle2 Compute() {
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

			// Projection of P-C onto plane is Q-C = P-C - Dot(N,P-C)*N.
			var PmC = _point - _circle.Center;
			var lengthPmC = PmC.Length;
			if (lengthPmC > MathUtil.EPSILON) {
				CircleClosest = _circle.Center + (_circle.Radius * PmC / lengthPmC);
				AllCirclePointsEquidistant = false;
			}
			else {
				// All circle points are equidistant from P.  Return one of them.
				CircleClosest = _circle.Center + _circle.Radius;
				AllCirclePointsEquidistant = true;
			}

			var diff = _point - CircleClosest;
			var sqrDistance = diff.Dot(diff);

			// Account for numerical round-off error.
			if (sqrDistance < 0) {
				sqrDistance = 0;
			}
			DistanceSquared = sqrDistance;
			return sqrDistance;
		}
	}
}
