using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public enum IntersectionResult
	{
		NotComputed,
		Intersects,
		NoIntersection,
		InvalidQuery
	}

	public enum IntersectionType
	{
		Empty, Point, Segment, Line, Polygon, Plane, Unknown
	}

	/// <summary>
	/// returned by linear-primitive intersection functions
	/// </summary>
	public struct LinearIntersection
	{
		public bool intersects;
		public int numIntersections;       // 0, 1, or 2
		public Interval1d parameter;       // t-values along ray
	}



	public static class IntersectionUtil
	{



		// same code as IntrRay3Triangle3, but can be called w/o constructing additional data structures
		public static bool Intersects(in Vector3d rayOrigin, in Vector3d rayDirection, in Vector3d V0, in Vector3d V1, in Vector3d V2) {
			// Compute the offset origin, edges, and normal.
			var diff = rayOrigin - V0;
			var edge1 = V1 - V0;
			var edge2 = V2 - V0;
			var normal = edge1.Cross(edge2);

			// Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = ray direction,
			// E1 = kEdge1, E2 = kEdge2, N = Cross(E1,E2)) by
			//   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
			//   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
			//   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
			var DdN = rayDirection.Dot(normal);
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

			var DdQxE2 = sign * rayDirection.Dot(diff.Cross(edge2));
			if (DdQxE2 >= 0) {
				var DdE1xQ = sign * rayDirection.Dot(edge1.Cross(diff));
				if (DdE1xQ >= 0) {
					if (DdQxE2 + DdE1xQ <= DdN) {
						// Line intersects triangle, check if ray does.
						var QdN = -sign * diff.Dot(normal);
						if (QdN >= 0) {
							// Ray intersects triangle.
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



		/// <summary>
		/// Test if line intersects sphere
		/// </summary>
		public static bool LineSphereTest(ref Vector3d lineOrigin, ref Vector3d lineDirection, ref Vector3d sphereCenter, in double sphereRadius) {
			// [RMS] adapted from GeometricTools GTEngine
			// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Sphere3.h

			// The sphere is (X-C)^T*(X-C)-1 = 0 and the line is X = P+t*D.
			// Substitute the line equation into the sphere equation to obtain a
			// quadratic equation Q(t) = t^2 + 2*a1*t + a0 = 0, where a1 = D^T*(P-C),
			// and a0 = (P-C)^T*(P-C)-1.

			var diff = lineOrigin - sphereCenter;
			var a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
			var a1 = lineDirection.Dot(diff);

			// Intersection occurs when Q(t) has real roots.
			var discr = (a1 * a1) - a0;
			return discr >= 0;
		}


		/// <summary>
		/// Intersect ray with sphere and return intersection info (# hits, ray parameters)
		/// </summary>
		public static bool LineSphere(ref Vector3d lineOrigin, ref Vector3d lineDirection, ref Vector3d sphereCenter, in double sphereRadius, ref LinearIntersection result) {
			// [RMS] adapted from GeometricTools GTEngine
			// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrLine3Sphere3.h

			// The sphere is (X-C)^T*(X-C)-1 = 0 and the line is X = P+t*D.
			// Substitute the line equation into the sphere equation to obtain a
			// quadratic equation Q(t) = t^2 + 2*a1*t + a0 = 0, where a1 = D^T*(P-C),
			// and a0 = (P-C)^T*(P-C)-1.
			var diff = lineOrigin - sphereCenter;
			var a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
			var a1 = lineDirection.Dot(diff);

			// Intersection occurs when Q(t) has real roots.
			var discr = (a1 * a1) - a0;
			if (discr > 0) {
				result.intersects = true;
				result.numIntersections = 2;
				var root = Math.Sqrt(discr);
				result.parameter.a = -a1 - root;
				result.parameter.b = -a1 + root;
			}
			else if (discr < 0) {
				result.intersects = false;
				result.numIntersections = 0;
			}
			else {
				result.intersects = true;
				result.numIntersections = 1;
				result.parameter.a = -a1;
				result.parameter.b = -a1;
			}
			return result.intersects;
		}
		public static LinearIntersection LineSphere(ref Vector3d lineOrigin, ref Vector3d lineDirection, ref Vector3d sphereCenter, in double sphereRadius) {
			var result = new LinearIntersection();
			LineSphere(ref lineOrigin, ref lineDirection, ref sphereCenter, sphereRadius, ref result);
			return result;
		}



		/// <summary>
		/// Test if ray intersects sphere
		/// </summary>
		public static bool RaySphereTest(ref Vector3d rayOrigin, ref Vector3d rayDirection, ref Vector3d sphereCenter, in double sphereRadius) {
			// [RMS] adapted from GeometricTools GTEngine
			// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Sphere3.h

			// The sphere is (X-C)^T*(X-C)-1 = 0 and the line is X = P+t*D.
			// Substitute the line equation into the sphere equation to obtain a
			// quadratic equation Q(t) = t^2 + 2*a1*t + a0 = 0, where a1 = D^T*(P-C),
			// and a0 = (P-C)^T*(P-C)-1.

			var diff = rayOrigin - sphereCenter;
			var a0 = diff.LengthSquared - (sphereRadius * sphereRadius);
			if (a0 <= 0) {
				return true;  // P is inside the sphere.
			}
			// else: P is outside the sphere
			var a1 = rayDirection.Dot(diff);
			if (a1 >= 0) {
				return false;
			}

			// Intersection occurs when Q(t) has double roots.
			var discr = (a1 * a1) - a0;
			return discr >= 0;
		}


		/// <summary>
		/// Intersect ray with sphere and return intersection info (# hits, ray parameters)
		/// </summary>
		public static bool RaySphere(ref Vector3d rayOrigin, ref Vector3d rayDirection, ref Vector3d sphereCenter, in double sphereRadius, ref LinearIntersection result) {
			// [RMS] adapted from GeometricTools GTEngine
			// https://www.geometrictools.com/GTEngine/Include/Mathematics/GteIntrRay3Sphere3.h

			LineSphere(ref rayOrigin, ref rayDirection, ref sphereCenter, sphereRadius, ref result);
			if (result.intersects) {
				// The line containing the ray intersects the sphere; the t-interval
				// is [t0,t1].  The ray intersects the sphere as long as [t0,t1]
				// overlaps the ray t-interval [0,+infinity).
				if (result.parameter.b < 0) {
					result.intersects = false;
					result.numIntersections = 0;
				}
				else if (result.parameter.a < 0) {
					result.numIntersections--;
					result.parameter.a = result.parameter.b;
				}
			}
			return result.intersects;
		}
		public static LinearIntersection RaySphere(ref Vector3d rayOrigin, ref Vector3d rayDirection, ref Vector3d sphereCenter, in double sphereRadius) {
			var result = new LinearIntersection();
			LineSphere(ref rayOrigin, ref rayDirection, ref sphereCenter, sphereRadius, ref result);
			return result;
		}

	}



}
