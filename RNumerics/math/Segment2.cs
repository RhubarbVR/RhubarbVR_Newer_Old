using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public struct Segment2d : IParametricCurve2d
	{
		// Center-direction-extent representation.
		public Vector2d Center;
		public Vector2d Direction;
		public double Extent;

		public Segment2d(in Vector2d p0, in Vector2d p1)
		{
			//update_from_endpoints(p0, p1);
			Center = 0.5 * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5 * Direction.Normalize();
		}
		public Segment2d(in Vector2d center, in Vector2d direction, in double extent)
		{
			Center = center;
			Direction = direction;
			Extent = extent;
		}

		public Vector2d P0
		{
			get => Center - (Extent * Direction);
			set => Update_from_endpoints(value, P1);
		}
		public Vector2d P1
		{
			get => Center + (Extent * Direction);
			set => Update_from_endpoints(P0, value);
		}
		public double Length => 2 * Extent;

		public Vector2d Endpoint(in int i)
		{
			return (i == 0) ? (Center - (Extent * Direction)) : (Center + (Extent * Direction));
		}

		// parameter is signed distance from center in direction
		public Vector2d PointAt(in double d)
		{
			return Center + (d * Direction);
		}

		// t ranges from [0,1] over [P0,P1]
		public Vector2d PointBetween(in double t)
		{
			return Center + (((2 * t) - 1) * Extent * Direction);
		}

		public double DistanceSquared(in Vector2d p)
		{
			var t = (p - Center).Dot(Direction);
			if (t >= Extent) {
				return P1.DistanceSquared(p);
			}
			else if (t <= -Extent) {
				return P0.DistanceSquared(p);
			}

			var proj = Center + (t * Direction);
			return proj.DistanceSquared(p);
		}
		public double DistanceSquared(in Vector2d p, out double t)
		{
			t = (p - Center).Dot(Direction);
			if (t >= Extent)
			{
				t = Extent;
				return P1.DistanceSquared(p);
			}
			else if (t <= -Extent)
			{
				t = -Extent;
				return P0.DistanceSquared(p);
			}
			var proj = Center + (t * Direction);
			return proj.DistanceSquared(p);
		}

		public Vector2d NearestPoint(in Vector2d p)
		{
			var t = (p - Center).Dot(Direction);
			return t >= Extent ? P1 : t <= -Extent ? P0 : Center + (t * Direction);
		}

		public double Project(in Vector2d p)
		{
			return (p - Center).Dot(Direction);
		}

		void Update_from_endpoints(in Vector2d p0, in Vector2d p1)
		{
			Center = 0.5 * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5 * Direction.Normalize();
		}




		/// <summary>
		/// Returns:
		///   +1, on right of line
		///   -1, on left of line
		///    0, on the line
		/// </summary>
		public int WhichSide(in Vector2d test, in double tol = 0)
		{
			// [TODO] subtract Center from test?
			var vec0 = Center + (Extent * Direction);
			var vec1 = Center - (Extent * Direction);
			var x0 = test.x - vec0.x;
			var y0 = test.y - vec0.y;
			var x1 = vec1.x - vec0.x;
			var y1 = vec1.y - vec0.y;
			var det = (x0 * y1) - (x1 * y0);
			return (det > tol ? +1 : (det < -tol ? -1 : 0));
		}



		// IParametricCurve2d interface

		public bool IsClosed => false;

		public double ParamLength => 1.0f;

		// t in range[0,1] spans arc
		public Vector2d SampleT(in double t)
		{
			return Center + (((2 * t) - 1) * Extent * Direction);
		}

		public Vector2d TangentT(in double t)
		{
			return Direction;
		}

		public bool HasArcLength => true;
		public double ArcLength => 2 * Extent;

		public Vector2d SampleArcLength(in double a)
		{
			return P0 + (a * Direction);
		}

		public void Reverse()
		{
			Update_from_endpoints(P1, P0);
		}

		public IParametricCurve2d Clone()
		{
			return new Segment2d(Center, Direction, Extent);
		}

		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform)
		{
			Center = xform.TransformP(Center);
			Direction = xform.TransformN(Direction);
			Extent = xform.TransformScalar(Extent);
		}



		/// <summary>
		/// distance from pt to segment (a,b), with no square roots
		/// </summary>
		public static double FastDistanceSquared(ref Vector2d a, ref Vector2d b, ref Vector2d pt)
		{
			double vx = b.x - a.x, vy = b.y - a.y;
			var len2 = (vx * vx) + (vy * vy);
			double dx = pt.x - a.x, dy = pt.y - a.y;
			if (len2 < 1e-13)
			{
				return (dx * dx) + (dy * dy);
			}
			var t = (dx * vx) + (dy * vy);
			if (t <= 0)
			{
				return (dx * dx) + (dy * dy);
			}
			else if (t >= len2)
			{
				dx = pt.x - b.x;
				dy = pt.y - b.y;
				return (dx * dx) + (dy * dy);
			}

			dx = pt.x - (a.x + (t * vx / len2));
			dy = pt.y - (a.y + (t * vy / len2));
			return (dx * dx) + (dy * dy);
		}


		/// <summary>
		/// Returns:
		///   +1, on right of line
		///   -1, on left of line
		///    0, on the line
		/// </summary>
		public static int WhichSide(ref Vector2d a, ref Vector2d b, ref Vector2d test, in double tol = 0)
		{
			var x0 = test.x - a.x;
			var y0 = test.y - a.y;
			var x1 = b.x - a.x;
			var y1 = b.y - a.y;
			var det = (x0 * y1) - (x1 * y0);
			return (det > tol ? +1 : (det < -tol ? -1 : 0));
		}




		/// <summary>
		/// Test if segments intersect. Returns true for parallel-line overlaps.
		/// Returns same result as IntrSegment2Segment2.
		/// </summary>
		public bool Intersects(in Segment2d seg2, in double dotThresh = double.Epsilon, in double intervalThresh = 0)
		{
			// see IntrLine2Line2 and IntrSegment2Segment2 for details on this code

			var diff = seg2.Center - Center;
			var D0DotPerpD1 = Direction.DotPerp(seg2.Direction);
			if (Math.Abs(D0DotPerpD1) > dotThresh)
			{   // Lines intersect in a single point.
				var invD0DotPerpD1 = ((double)1) / D0DotPerpD1;
				var diffDotPerpD0 = diff.DotPerp(Direction);
				var diffDotPerpD1 = diff.DotPerp(seg2.Direction);
				var s = diffDotPerpD1 * invD0DotPerpD1;
				var s2 = diffDotPerpD0 * invD0DotPerpD1;
				return Math.Abs(s) <= (Extent + intervalThresh)
						&& Math.Abs(s2) <= (seg2.Extent + intervalThresh);
			}

			// Lines are parallel.
			diff.Normalize();
			var diffNDotPerpD1 = diff.DotPerp(seg2.Direction);
			if (Math.Abs(diffNDotPerpD1) <= dotThresh)
			{
				// Compute the location of segment1 endpoints relative to segment0.
				diff = seg2.Center - Center;
				var t1 = Direction.Dot(diff);
				var tmin = t1 - seg2.Extent;
				var tmax = t1 + seg2.Extent;
				var extents = new Interval1d(-Extent, Extent);
				return extents.Overlaps(new Interval1d(tmin, tmax));
			}

			// lines are parallel but not collinear
			return false;
		}

	}







	public struct Segment2f
	{
		// Center-direction-extent representation.
		public Vector2f Center;
		public Vector2f Direction;
		public float Extent;

		public Segment2f(in Vector2f p0, in Vector2f p1)
		{
			//update_from_endpoints(p0, p1);
			Center = 0.5f * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5f * Direction.Normalize();
		}
		public Segment2f(in Vector2f center, in Vector2f direction, in float extent)
		{
			Center = center;
			Direction = direction;
			Extent = extent;
		}

		public Vector2f P0
		{
			get => Center - (Extent * Direction);
			set => Update_from_endpoints(value, P1);
		}
		public Vector2f P1
		{
			get => Center + (Extent * Direction);
			set => Update_from_endpoints(P0, value);
		}
		public float Length => 2 * Extent;


		// parameter is signed distance from center in direction
		public Vector2f PointAt(in float d)
		{
			return Center + (d * Direction);
		}

		// t ranges from [0,1] over [P0,P1]
		public Vector2f PointBetween(in float t)
		{
			return Center + (((2.0f * t) - 1.0f) * Extent * Direction);
		}

		public float DistanceSquared(in Vector2f p)
		{
			var t = (p - Center).Dot(Direction);
			if (t >= Extent) {
				return P1.DistanceSquared(p);
			}
			else if (t <= -Extent) {
				return P0.DistanceSquared(p);
			}

			var proj = Center + (t * Direction);
			return (proj - p).LengthSquared;
		}

		public Vector2f NearestPoint(in Vector2f p)
		{
			var t = (p - Center).Dot(Direction);
			return t >= Extent ? P1 : t <= -Extent ? P0 : Center + (t * Direction);
		}

		public float Project(in Vector2f p)
		{
			return (p - Center).Dot(Direction);
		}



		void Update_from_endpoints(in Vector2f p0, in Vector2f p1)
		{
			Center = 0.5f * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5f * Direction.Normalize();
		}




		/// <summary>
		/// distance from pt to segment (a,b), with no square roots
		/// </summary>
		public static float FastDistanceSquared(ref Vector2f a, ref Vector2f b, ref Vector2f pt)
		{
			float vx = b.x - a.x, vy = b.y - a.y;
			var len2 = (vx * vx) + (vy * vy);
			float dx = pt.x - a.x, dy = pt.y - a.y;
			if (len2 < 1e-7)
			{
				return (dx * dx) + (dy * dy);
			}
			var t = (dx * vx) + (dy * vy);
			if (t <= 0)
			{
				return (dx * dx) + (dy * dy);
			}
			else if (t >= len2)
			{
				dx = pt.x - b.x;
				dy = pt.y - b.y;
				return (dx * dx) + (dy * dy);
			}

			dx = pt.x - (a.x + (t * vx / len2));
			dy = pt.y - (a.y + (t * vy / len2));
			return (dx * dx) + (dy * dy);
		}

	}





	public sealed class Segment2dBox
	{
		public Segment2d Segment;

		public Segment2dBox() { }
		public Segment2dBox(in Segment2d seg)
		{
			Segment = seg;
		}

		public static implicit operator Segment2d(in Segment2dBox box) => box.Segment;
	}




}
