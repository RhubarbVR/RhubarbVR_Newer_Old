using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public static class RayIntersection
	{

		// basic ray-sphere intersection
		public static bool Sphere(in Vector3f vOrigin, in Vector3f vDirection, in Vector3f vCenter, in float fRadius, out float fRayT) {
			var bHit = SphereSigned(vOrigin, vDirection, vCenter, fRadius, out fRayT);
			fRayT = Math.Abs(fRayT);
			return bHit;
		}

		public static bool SphereSigned(in Vector3f vOrigin, in Vector3f vDirection, in Vector3f vCenter, in float fRadius, out float fRayT) {
			fRayT = 0.0f;
			var m = vOrigin - vCenter;
			var b = m.Dot(vDirection);
			var c = m.Dot(m) - (fRadius * fRadius);

			// Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
			if (c > 0.0f && b > 0.0f) {
				return false;
			}

			var discr = (b * b) - c;

			// A negative discriminant corresponds to ray missing sphere 
			if (discr < 0.0f) {
				return false;
			}

			// Ray now found to intersect sphere, compute smallest t value of intersection
			fRayT = -b - (float)Math.Sqrt(discr);

			return true;
		}



		public static bool SphereSigned(in Vector3d vOrigin, in Vector3d vDirection, in Vector3d vCenter, in double fRadius, out double fRayT) {
			fRayT = 0.0;
			var m = vOrigin - vCenter;
			var b = m.Dot(vDirection);
			var c = m.Dot(m) - (fRadius * fRadius);

			// Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
			if (c > 0.0f && b > 0.0f) {
				return false;
			}

			var discr = (b * b) - c;
			// A negative discriminant corresponds to ray missing sphere 
			if (discr < 0.0) {
				return false;
			}
			// Ray now found to intersect sphere, compute smallest t value of intersection
			fRayT = -b - Math.Sqrt(discr);
			return true;
		}


		public static bool InfiniteCylinder(in Vector3f vOrigin, in Vector3f vDirection, in Vector3f vCylOrigin, in Vector3f vCylAxis, in float fRadius, out float fRayT) {
			var bHit = InfiniteCylinderSigned(vOrigin, vDirection, vCylOrigin, vCylAxis, fRadius, out fRayT);
			fRayT = Math.Abs(fRayT);
			return bHit;
		}
		public static bool InfiniteCylinderSigned(in Vector3f vOrigin, in Vector3f vDirection, in Vector3f vCylOrigin, in Vector3f vCylAxis, in float fRadius, out float fRayT) {
			// [RMS] ugh this is shit...not even sure it works in general, but works for a ray inside cylinder

			fRayT = 0.0f;


			var AB = vCylAxis;
			var AO = vOrigin - vCylOrigin;
			if (AO.DistanceSquared(AO.Dot(AB) * AB) > fRadius * fRadius) {
				return false;
			}

			var AOxAB = AO.Cross(AB);
			var VxAB = vDirection.Cross(AB);
			var ab2 = AB.Dot(AB);
			var a = VxAB.Dot(VxAB);
			var b = 2 * VxAB.Dot(AOxAB);
			var c = AOxAB.Dot(AOxAB) - (fRadius * fRadius * ab2);

			double discrim = (b * b) - (4 * a * c);
			if (discrim <= 0) {
				return false;
			}

			discrim = Math.Sqrt(discrim);
			fRayT = (-b - (float)discrim) / (2 * a);
			//float t1 = (-b + (float)discrim) / (2 * a);

			return true;
		}

	}
}
