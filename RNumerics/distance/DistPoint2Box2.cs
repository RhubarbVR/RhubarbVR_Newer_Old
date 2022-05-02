using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from WildMagic 5's DistPoint2Box2
	// https://www.geometrictools.com/Downloads/Downloads.html

	public class DistPoint2Box2
	{
		Vector2d _point;
		public Vector2d Point
		{
			get => _point;
			set { _point = value; DistanceSquared = -1.0; }
		}

		Box2d _box;
		public Box2d Box
		{
			get => _box;
			set { _box = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		public Vector2d BoxClosest;


		public DistPoint2Box2(Vector2d PointIn, Box2d boxIn) {
			_point = PointIn;
			_box = boxIn;
		}

		public DistPoint2Box2 Compute() {
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

			// Work in the box's coordinate system.
			var diff = _point - _box.Center;

			// Compute squared distance and closest point on box.
			var sqrDistance = (double)0;
			double delta;
			var closest = Vector2d.Zero;
			int i;
			for (i = 0; i < 2; ++i) {
				closest[i] = diff.Dot(_box.Axis(i));
				if (closest[i] < -_box.Extent[i]) {
					delta = closest[i] + _box.Extent[i];
					sqrDistance += delta * delta;
					closest[i] = -_box.Extent[i];
				}
				else if (closest[i] > _box.Extent[i]) {
					delta = closest[i] - _box.Extent[i];
					sqrDistance += delta * delta;
					closest[i] = _box.Extent[i];
				}
			}

			BoxClosest = _box.Center;
			for (i = 0; i < 2; ++i) {
				BoxClosest += closest[i] * _box.Axis(i);
			}

			DistanceSquared = sqrDistance;
			return sqrDistance;
		}
	}
}
