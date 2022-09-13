using System;

using MessagePack;
namespace RNumerics
{
	//3D plane, based on WildMagic5 Wm5Plane3 class
	[MessagePackObject]
	public struct Plane3d
	{
		[Key(0)]
		public Vector3d Normal;
		[Key(1)]
		public double Constant;

		public Plane3d() {
			Normal = Vector3d.Zero;
			Constant = 0;
		}

		public Plane3d(in Vector3d normal, in double constant) {
			Normal = normal;
			Constant = constant;
		}

		// N is specified, c = Dot(N,P) where P is a point on the plane.
		public Plane3d(in Vector3d normal, in Vector3d point) {
			Normal = normal;
			Constant = Normal.Dot(point);
		}

		// N = Cross(P1-P0,P2-P0)/Length(Cross(P1-P0,P2-P0)), c = Dot(N,P0) where
		// P0, P1, P2 are points on the plane.
		public Plane3d(in Vector3d p0, in Vector3d p1, in Vector3d p2) {
			var edge1 = p1 - p0;
			var edge2 = p2 - p0;
			Normal = edge1.UnitCross(edge2);
			Constant = Normal.Dot(p0);
		}


		// Compute d = Dot(N,P)-c where N is the plane normal and c is the plane
		// constant.  This is a signed distance.  The sign of the return value is
		// positive if the point is on the positive side of the plane, negative if
		// the point is on the negative side, and zero if the point is on the
		// plane.
		public double DistanceTo(in Vector3d p) {
			return Normal.Dot(p) - Constant;
		}

		// The "positive side" of the plane is the half space to which the plane
		// normal points.  The "negative side" is the other half space.  The
		// function returns +1 when P is on the positive side, -1 when P is on the
		// the negative side, or 0 when P is on the plane.
		public int WhichSide(in Vector3d p) {
			var distance = DistanceTo(p);
			return distance < 0 ? -1 : distance > 0 ? +1 : 0;
		}
		public Vector3d ClosestPointOnPlane(in Vector3d point) {
			var num = Vector3d.Dot(Normal, point) + Constant;
			return point - (Normal * num);
		}

		public Vector3d IntersectLine(in Vector3d a, in Vector3d b) {
			var ba = b - a;
			var nDotA = Normal.Dot(a);
			var nDotBA = Normal.Dot(ba);
			return a + ((Constant - nDotA) / nDotBA * ba);
		}
	}



	[MessagePackObject]
	public struct Plane3f
	{
		[Key(0)]
		public Vector3f Normal;
		[Key(1)]
		public float Constant;

		public Plane3f(in Vector3f normal, in float constant) {
			Normal = normal;
			Constant = constant;
		}

		// N is specified, c = Dot(N,P) where P is a point on the plane.
		public Plane3f(in Vector3f normal, in Vector3f point) {
			Normal = normal;
			Constant = Normal.Dot(point);
		}

		// N = Cross(P1-P0,P2-P0)/Length(Cross(P1-P0,P2-P0)), c = Dot(N,P0) where
		// P0, P1, P2 are points on the plane.
		public Plane3f(in Vector3f p0, in Vector3f p1, in Vector3f p2) {
			var edge1 = p1 - p0;
			var edge2 = p2 - p0;
			Normal = edge1.UnitCross(edge2);
			Constant = Normal.Dot(p0);
		}


		// Compute d = Dot(N,P)-c where N is the plane normal and c is the plane
		// constant.  This is a signed distance.  The sign of the return value is
		// positive if the point is on the positive side of the plane, negative if
		// the point is on the negative side, and zero if the point is on the
		// plane.
		public float DistanceTo(in Vector3f p) {
			return Normal.Dot(p) - Constant;
		}

		// The "positive side" of the plane is the half space to which the plane
		// normal points.  The "negative side" is the other half space.  The
		// function returns +1 when P is on the positive side, -1 when P is on the
		// the negative side, or 0 when P is on the plane.
		public int WhichSide(in Vector3f p) {
			var distance = DistanceTo(p);
			return distance < 0 ? -1 : distance > 0 ? +1 : 0;
		}
		public Vector3f ClosestPointOnPlane(in Vector3f point) {
			var num = Vector3f.Dot(Normal, point) + Constant;
			return point - (Normal * num);
		}

		public Vector3f IntersectLine(in Vector3f a, Vector3f b) {
			var ba = b - a;
			var nDotA = Normal.Dot(a);
			var nDotBA = Normal.Dot(ba);
			return a + ((Constant - nDotA) / nDotBA * ba);
		}
	}


}
