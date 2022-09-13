using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	// ported from GTEngine
	// https://www.geometrictools.com/Downloads/Downloads.html
	// However, code is modified to compute signed distance, instead of distance
	// to cylinder solid (which is 0 inside cylinder). If you want solid distance,
	// check IsInside, and if true, distance is 0 and point is input point.
	// SolidDistance will return this distance for you, but you have to do
	// the Point classification yourself.
	//
	// DistanceSquared is always positive!!
	//
	public sealed class DistPoint3Cylinder3
	{
		Vector3d _point;
		public Vector3d Point
		{
			get => _point;
			set { _point = value; DistanceSquared = -1.0; }
		}

		Cylinder3d _cylinder;
		public Cylinder3d Cylinder
		{
			get => _cylinder;
			set { _cylinder = value; DistanceSquared = -1.0; }
		}

		public double DistanceSquared = -1.0;

		// negative on inside
		public double SignedDistance = 0.0f;

		public bool IsInside => SignedDistance < 0;
		public double SolidDistance => (SignedDistance < 0) ? 0 : SignedDistance;

		public Vector3d CylinderClosest;

		public DistPoint3Cylinder3(in Vector3d PointIn, in Cylinder3d CylinderIn) {
			_point = PointIn;
			_cylinder = CylinderIn;
		}

		public DistPoint3Cylinder3 Compute() {
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

			if (_cylinder.Height >= double.MaxValue) {
				return Get_squared_infinite();
			}


			// Convert the point to the cylinder coordinate system.  In this system,
			// the point believes (0,0,0) is the cylinder axis origin and (0,0,1) is
			// the cylinder axis direction.
			var basis0 = _cylinder.Axis.Direction;
			Vector3d basis1 = Vector3d.Zero, basis2 = Vector3d.Zero;
			Vector3d.ComputeOrthogonalComplement(1, basis0, ref basis1, ref basis2);
			var height = Cylinder.Height / 2.0;

			var delta = _point - _cylinder.Axis.Origin;
			var P = new Vector3d(basis1.Dot(delta), basis2.Dot(delta), basis0.Dot(delta));

			var sqrRadius = _cylinder.Radius * _cylinder.Radius;
			var sqrDistance = (P[0] * P[0]) + (P[1] * P[1]);

			// The point is outside the infinite cylinder, or on the cylinder wall.
			var distance = Math.Sqrt(sqrDistance);
			var inf_distance = distance - Cylinder.Radius;
			var temp = Cylinder.Radius / distance;
			var inf_closest = new Vector3d(temp * P.x, temp * P.y, P.z);
			var bOutside = sqrDistance >= sqrRadius;

			var result_closest = inf_closest;
			var result_distance = inf_distance;

			if (inf_closest.z >= height) {
				result_closest = bOutside ? inf_closest : P;
				result_closest.z = height;
				result_distance = result_closest.Distance(P);       // TODO: only compute sqr here
				bOutside = true;
			}
			else if (inf_closest.z <= -height) {
				result_closest = bOutside ? inf_closest : P;
				result_closest.z = -height;
				result_distance = result_closest.Distance(P);       // TODO: only compute sqr here
				bOutside = true;
			}
			else if (bOutside == false) {
				if (inf_closest.z > 0 && Math.Abs(inf_closest.z - height) < Math.Abs(inf_distance)) {
					result_closest = P;
					result_closest.z = height;
					result_distance = result_closest.Distance(P);       // TODO: only compute sqr here
				}
				else if (inf_closest.z < 0 && Math.Abs(inf_closest.z - -height) < Math.Abs(inf_distance)) {
					result_closest = P;
					result_closest.z = -height;
					result_distance = result_closest.Distance(P);       // TODO: only compute sqr here
				}
			}
			SignedDistance = bOutside ? Math.Abs(result_distance) : -Math.Abs(result_distance);

			// Convert the closest point from the cylinder coordinate system to the
			// original coordinate system.
			CylinderClosest = _cylinder.Axis.Origin +
				(result_closest.x * basis1) +
				(result_closest.y * basis2) +
				(result_closest.z * basis0);

			DistanceSquared = result_distance * result_distance;

			return DistanceSquared;
		}



		public double Get_squared_infinite() {
			// Convert the point to the cylinder coordinate system.  In this system,
			// the point believes (0,0,0) is the cylinder axis origin and (0,0,1) is
			// the cylinder axis direction.
			var basis0 = _cylinder.Axis.Direction;
			Vector3d basis1 = Vector3d.Zero, basis2 = Vector3d.Zero;
			Vector3d.ComputeOrthogonalComplement(1, basis0, ref basis1, ref basis2);

			var delta = _point - _cylinder.Axis.Origin;
			var P = new Vector3d(basis1.Dot(delta), basis2.Dot(delta), basis0.Dot(delta));

			var sqrDistance = (P[0] * P[0]) + (P[1] * P[1]);

			// The point is outside the cylinder or on the cylinder wall.
			var distance = Math.Sqrt(sqrDistance);
			var result_distance = distance - Cylinder.Radius;
			var temp = Cylinder.Radius / distance;
			var result_closest = new Vector3d(temp * P.x, temp * P.y, P.z);


			// Convert the closest point from the cylinder coordinate system to the
			// original coordinate system.
			CylinderClosest = _cylinder.Axis.Origin +
				(result_closest.x * basis1) +
				(result_closest.y * basis2) +
				(result_closest.z * basis0);
			SignedDistance = result_distance;
			DistanceSquared = result_distance * result_distance;
			return DistanceSquared;
		}

	}
}
