using System;
using MessagePack;
namespace RNumerics
{
	// somewhat ported from WildMagic5
	[MessagePackObject]
	public class Circle3d
	{
		// The plane containing the circle is Dot(N,X-C) = 0, where X is any point
		// in the plane.  Vectors U, V, and N form an orthonormal right-handed set
		// (matrix [U V N] is orthonormal and has determinant 1).  The circle
		// within the plane is parameterized by X = C + R*(cos(t)*U + sin(t)*V),
		// where t is an angle in [-pi,pi).
		[Key(0)]
		public Vector3d Center;
		[Key(1)]
		public Vector3d Normal;
		[Key(2)]
		public Vector3d PlaneX;
		[Key(3)]
		public Vector3d PlaneY;
		[Key(4)]
		public double Radius;
		[Key(5)]
		public bool IsReversed;     // use ccw orientation instead of cw

		public Circle3d(Vector3d center, double radius, Vector3d axis0, Vector3d axis1, Vector3d normal) {
			IsReversed = false;
			Center = center;
			Normal = normal;
			PlaneX = axis0;
			PlaneY = axis1;
			Radius = radius;
		}
		public Circle3d(Frame3f frame, double radius, int nNormalAxis = 1) {
			IsReversed = false;
			Center = frame.Origin;
			Normal = frame.GetAxis(nNormalAxis);
			PlaneX = frame.GetAxis((nNormalAxis + 1) % 3);
			PlaneY = frame.GetAxis((nNormalAxis + 2) % 3);
			Radius = radius;
		}
		public Circle3d(Vector3d center, double radius) {
			IsReversed = false;
			Center = center;
			Normal = Vector3d.AxisY;
			PlaneX = Vector3d.AxisX;
			PlaneY = Vector3d.AxisZ;
			Radius = radius;
		}
		[IgnoreMember]
		public bool IsClosed => true;

		public void Reverse() {
			IsReversed = !IsReversed;
		}


		// angle in range [0,360] (but works for any value, obviously)
		public Vector3d SampleDeg(double degrees) {
			var theta = degrees * MathUtil.DEG_2_RAD;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return Center + (c * Radius * PlaneX) + (s * Radius * PlaneY);
		}

		// angle in range [0,2pi] (but works for any value, obviously)
		public Vector3d SampleRad(double radians) {
			double c = Math.Cos(radians), s = Math.Sin(radians);
			return Center + (c * Radius * PlaneX) + (s * Radius * PlaneY);
		}



		[IgnoreMember]
		public double ParamLength => 1.0f;

		// t in range[0,1] spans circle [0,2pi]
		public Vector3d SampleT(double t) {
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return Center + (c * Radius * PlaneX) + (s * Radius * PlaneY);
		}

		[IgnoreMember]
		public bool HasArcLength => true;

		[IgnoreMember]
		public double ArcLength => MathUtil.TWO_PI * Radius;

		public Vector3d SampleArcLength(double a) {
			var t = a / ArcLength;
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return Center + (c * Radius * PlaneX) + (s * Radius * PlaneY);
		}


		[IgnoreMember]
		public double Circumference => MathUtil.TWO_PI * Radius;
		[IgnoreMember]
		public double Diameter => 2 * Radius;
		[IgnoreMember]
		public double Area => Math.PI * Radius * Radius;

	}
}
