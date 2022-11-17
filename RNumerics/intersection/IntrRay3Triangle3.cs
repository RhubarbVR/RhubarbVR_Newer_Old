using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public sealed class IntrRay3Triangle3
	{
		Ray3d _ray;
		public Ray3d Ray
		{
			get => _ray;
			set { _ray = value; Result = IntersectionResult.NotComputed; }
		}

		Triangle3d _triangle;
		public Triangle3d Triangle
		{
			get => _triangle;
			set { _triangle = value; Result = IntersectionResult.NotComputed; }
		}

		public int Quantity = 0;
		public IntersectionResult Result = IntersectionResult.NotComputed;
		public IntersectionType Type = IntersectionType.Empty;

		public bool IsSimpleIntersection => Result == IntersectionResult.Intersects && Type == IntersectionType.Point;


		public double RayParameter;
		public Vector3d TriangleBaryCoords;


		public IntrRay3Triangle3(in Ray3d r, in Triangle3d t) {
			_ray = r;
			_triangle = t;
		}


		public IntrRay3Triangle3 Compute() {
			Find();
			return this;
		}


		public bool Find() {
			if (Result != IntersectionResult.NotComputed) {
				return Result != IntersectionResult.NoIntersection;
			}

			// Compute the offset origin, edges, and normal.
			var diff = _ray.origin - _triangle.v0;
			var edge1 = _triangle.v1 - _triangle.v0;
			var edge2 = _triangle.v2 - _triangle.v0;
			var normal = edge1.Cross(edge2);

			// Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = ray direction,
			// E1 = kEdge1, E2 = kEdge2, N = Cross(E1,E2)) by
			//   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
			//   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
			//   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
			var DdN = _ray.direction.Dot(normal);
			double sign;
			if (DdN > MathUtil.ZERO_TOLERANCE) {
				sign = 1;
			}
			else if (DdN < -MathUtil.ZERO_TOLERANCE) {
				sign = -1;
				DdN = -DdN;
			}
			else {
				// Ray and triangle are parallel, call it a "no intersection"
				// even if the ray does intersect.
				Result = IntersectionResult.NoIntersection;
				return false;
			}

			var DdQxE2 = sign * _ray.direction.Dot(diff.Cross(edge2));
			if (DdQxE2 >= 0) {
				var DdE1xQ = sign * _ray.direction.Dot(edge1.Cross(diff));
				if (DdE1xQ >= 0) {
					if (DdQxE2 + DdE1xQ <= DdN) {
						// Line intersects triangle, check if ray does.
						var QdN = -sign * diff.Dot(normal);
						if (QdN >= 0) {
							// Ray intersects triangle.
							var inv = 1 / DdN;
							RayParameter = QdN * inv;
							var mTriBary1 = DdQxE2 * inv;
							var mTriBary2 = DdE1xQ * inv;
							TriangleBaryCoords = new Vector3d(1 - mTriBary1 - mTriBary2, mTriBary1, mTriBary2);
							Type = IntersectionType.Point;
							Quantity = 1;
							Result = IntersectionResult.Intersects;
							return true;
						}
						// else: t < 0, no intersection
					}
					// else: b1+b2 > 1, no intersection
				}
				// else: b2 < 0, no intersection
			}
			// else: b1 < 0, no intersection

			Result = IntersectionResult.NoIntersection;
			return false;
		}



		/// <summary>
		/// minimal intersection test, computes ray-t
		/// </summary>
		public static bool Intersects(ref Ray3d ray, ref Vector3d V0, ref Vector3d V1, ref Vector3d V2, out double rayT) {
			// Compute the offset origin, edges, and normal.
			var diff = ray.origin - V0;
			var edge1 = V1 - V0;
			var edge2 = V2 - V0;
			var normal = edge1.Cross(edge2);

			rayT = double.MaxValue;

			// Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = ray direction,
			// E1 = kEdge1, E2 = kEdge2, N = Cross(E1,E2)) by
			//   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
			//   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
			//   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
			var DdN = ray.direction.Dot(normal);
			double sign;
			if (DdN > MathUtil.ZERO_TOLERANCE) {
				sign = 1;
			}
			else if (DdN < -MathUtil.ZERO_TOLERANCE) {
				sign = -1;
				DdN = -DdN;
			}
			else {
				// Ray and triangle are parallel, call it a "no intersection"
				// even if the ray does intersect.
				return false;
			}

			var cross = diff.Cross(edge2);
			var DdQxE2 = sign * ray.direction.Dot(cross);
			if (DdQxE2 >= 0) {
				cross = edge1.Cross(diff);
				var DdE1xQ = sign * ray.direction.Dot(cross);
				if (DdE1xQ >= 0) {
					if (DdQxE2 + DdE1xQ <= DdN) {
						// Line intersects triangle, check if ray does.
						var QdN = -sign * diff.Dot(normal);
						if (QdN >= 0) {
							// Ray intersects triangle.
							var inv = 1 / DdN;
							rayT = QdN * inv;
							return true;
						}
						// else: t < 0, no intersection
					}
					// else: b1+b2 > 1, no intersection
				}
				// else: b2 < 0, no intersection
			}
			// else: b1 < 0, no intersection

			return false;
		}

	}
}
