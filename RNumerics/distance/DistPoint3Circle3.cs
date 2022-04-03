using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5
	// https://www.geometrictools.com/Downloads/Downloads.html

	public class DistPoint3Circle3
	{
		Vector3d _point;
		public Vector3d Point
		{
			get => _point;
			set { _point = value; DistanceSquared = -1.0; }
		}

		Circle3d _circle;
		public Circle3d Circle
		{
			get => _circle;
			set { _circle = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector3d CircleClosest;
		public bool AllCirclePointsEquidistant;


		public DistPoint3Circle3(Vector3d PointIn, Circle3d circleIn) {
			_point = PointIn;
			_circle = circleIn;
		}

		public DistPoint3Circle3 Compute() {
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
			var QmC = PmC - (_circle.Normal.Dot(PmC) * _circle.Normal);
			var lengthQmC = QmC.Length;
			if (lengthQmC > MathUtil.EPSILON) {
				CircleClosest = _circle.Center + (_circle.Radius * QmC / lengthQmC);
				AllCirclePointsEquidistant = false;
			}
			else {
				// All circle points are equidistant from P.  Return one of them.
				CircleClosest = _circle.Center + (_circle.Radius * _circle.PlaneX);
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
