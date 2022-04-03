using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Segment3d : IParametricCurve3d
	{
		// Center-direction-extent representation.
		// Extent is half length of segment
		[Key(0)]
		public Vector3d Center;
		[Key(1)]
		public Vector3d Direction;
		[Key(2)]
		public double Extent;

		public Segment3d(Vector3d p0, Vector3d p1)
		{
			//update_from_endpoints(p0, p1);
			Center = 0.5 * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5 * Direction.Normalize();
		}
		public Segment3d(Vector3d center, Vector3d direction, double extent)
		{
			Center = center;
			Direction = direction;
			Extent = extent;
		}

		public void SetEndpoints(Vector3d p0, Vector3d p1)
		{
			Update_from_endpoints(p0, p1);
		}

		[IgnoreMember]
		public Vector3d P0
		{
			get => Center - (Extent * Direction);
			set => Update_from_endpoints(value, P1);
		}
		[IgnoreMember]
		public Vector3d P1
		{
			get => Center + (Extent * Direction);
			set => Update_from_endpoints(P0, value);
		}
		[IgnoreMember]
		public double Length => 2 * Extent;

		// parameter is signed distance from center in direction
		public Vector3d PointAt(double d)
		{
			return Center + (d * Direction);
		}

		// t ranges from [0,1] over [P0,P1]
		public Vector3d PointBetween(double t) {
			return Center + (((2 * t) - 1) * Extent * Direction);
		}

		public double DistanceSquared(Vector3d p)
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
		public double DistanceSquared(Vector3d p, out double t)
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
			return (proj - p).LengthSquared;
		}


		public Vector3d NearestPoint(Vector3d p)
		{
			var t = (p - Center).Dot(Direction);
			return t >= Extent ? P1 : t <= -Extent ? P0 : Center + (t * Direction);
		}

		public double Project(Vector3d p)
		{
			return (p - Center).Dot(Direction);
		}


		void Update_from_endpoints(Vector3d p0, Vector3d p1)
		{
			Center = 0.5 * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5 * Direction.Normalize();
		}


		// conversion operators
		public static implicit operator Segment3d(Segment3f v) => new (v.Center, v.Direction, v.Extent);
		public static explicit operator Segment3f(Segment3d v) => new ((Vector3f)v.Center, (Vector3f)v.Direction, (float)v.Extent);


		// IParametricCurve3d interface

		[IgnoreMember]
		public bool IsClosed => false;

		[IgnoreMember]
		public double ParamLength => 1.0f;

		// t in range[0,1] spans arc
		public Vector3d SampleT(double t)
		{
			return Center + (((2 * t) - 1) * Extent * Direction);
		}

		public Vector3d TangentT(double t)
		{
			return Direction;
		}

		[IgnoreMember]
		public bool HasArcLength => true;
		[IgnoreMember]
		public double ArcLength => 2 * Extent;

		public Vector3d SampleArcLength(double a)
		{
			return P0 + (a * Direction);
		}

		public void Reverse()
		{
			Update_from_endpoints(P1, P0);
		}

		public IParametricCurve3d Clone()
		{
			return new Segment3d(Center, Direction, Extent);
		}


	}


	[MessagePackObject]
	public struct Segment3f
	{
		// Center-direction-extent representation.
		// Extent is half length of segment
		[Key(0)]
		public Vector3f Center;
		[Key(1)]
		public Vector3f Direction;
		[Key(2)]
		public float Extent;

		public Segment3f(Vector3f p0, Vector3f p1)
		{
			//update_from_endpoints(p0, p1);
			Center = 0.5f * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5f * Direction.Normalize();
		}
		public Segment3f(Vector3f center, Vector3f direction, float extent)
		{
			Center = center;
			Direction = direction;
			Extent = extent;
		}


		public void SetEndpoints(Vector3f p0, Vector3f p1)
		{
			Update_from_endpoints(p0, p1);
		}


		[IgnoreMember]
		public Vector3f P0
		{
			get => Center - (Extent * Direction);
			set => Update_from_endpoints(value, P1);
		}
		[IgnoreMember]
		public Vector3f P1
		{
			get => Center + (Extent * Direction);
			set => Update_from_endpoints(P0, value);
		}
		public float Length => 2 * Extent;

		// parameter is signed distance from center in direction
		public Vector3f PointAt(float d)
		{
			return Center + (d * Direction);
		}


		// t ranges from [0,1] over [P0,P1]
		public Vector3f PointBetween(float t)
		{
			return Center + (((2 * t) - 1) * Extent * Direction);
		}


		public float DistanceSquared(Vector3f p)
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

		public Vector3f NearestPoint(Vector3f p)
		{
			var t = (p - Center).Dot(Direction);
			return t >= Extent ? P1 : t <= -Extent ? P0 : Center + (t * Direction);
		}


		public float Project(Vector3f p)
		{
			return (p - Center).Dot(Direction);
		}




		void Update_from_endpoints(Vector3f p0, Vector3f p1)
		{
			Center = 0.5f * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5f * Direction.Normalize();
		}
	}

}
