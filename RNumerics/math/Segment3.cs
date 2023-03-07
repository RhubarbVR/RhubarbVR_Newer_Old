using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;


namespace RNumerics
{
	public struct Segment3d : IParametricCurve3d, ISerlize<Segment3d>
	{
		// Center-direction-extent representation.
		// Extent is half length of segment
		public Vector3d center;
		public Vector3d direction;
		public double extent;


		public void Serlize(BinaryWriter binaryWriter) {
			center.Serlize(binaryWriter);
			direction.Serlize(binaryWriter);
			binaryWriter.Write(extent);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			center.DeSerlize(binaryReader);
			direction.DeSerlize(binaryReader);
			extent = binaryReader.ReadDouble();
		}

		[Exposed]
		public Vector3d Center
		{
			get => center;
			set => center = value;
		}
		[Exposed]
		public Vector3d Direction
		{
			get => direction;
			set => direction = value;
		}
		[Exposed]
		public double Extent
		{
			get => extent;
			set => extent = value;
		}

		public Segment3d(in Vector3d p0, in Vector3d p1)
		{
			//update_from_endpoints(p0, p1);
			center = 0.5 * (p0 + p1);
			direction = p1 - p0;
			extent = 0.5 * direction.Normalize();
		}
		public Segment3d(in Vector3d center, in Vector3d direction, in double extent)
		{
			this.center = center;
			this.direction = direction;
			this.extent = extent;
		}

		public void SetEndpoints(in Vector3d p0, in Vector3d p1)
		{
			Update_from_endpoints(p0, p1);
		}

		
		public Vector3d P0
		{
			get => center - (extent * direction);
			set => Update_from_endpoints(value, P1);
		}
		
		public Vector3d P1
		{
			get => center + (extent * direction);
			set => Update_from_endpoints(P0, value);
		}
		
		public double Length => 2 * extent;

		// parameter is signed distance from center in direction
		public Vector3d PointAt(in double d)
		{
			return center + (d * direction);
		}

		// t ranges from [0,1] over [P0,P1]
		public Vector3d PointBetween(in double t) {
			return center + (((2 * t) - 1) * extent * direction);
		}

		public double DistanceSquared(in Vector3d p)
		{
			var t = (p - center).Dot(direction);
			if (t >= extent) {
				return P1.DistanceSquared(p);
			}
			else if (t <= -extent) {
				return P0.DistanceSquared(p);
			}

			var proj = center + (t * direction);
			return (proj - p).LengthSquared;
		}
		public double DistanceSquared(in Vector3d p, out double t)
		{
			t = (p - center).Dot(direction);
			if (t >= extent)
			{
				t = extent;
				return P1.DistanceSquared(p);
			}
			else if (t <= -extent)
			{
				t = -extent;
				return P0.DistanceSquared(p);
			}
			var proj = center + (t * direction);
			return (proj - p).LengthSquared;
		}


		public Vector3d NearestPoint(in Vector3d p)
		{
			var t = (p - center).Dot(direction);
			return t >= extent ? P1 : t <= -extent ? P0 : center + (t * direction);
		}

		public double Project(in Vector3d p)
		{
			return (p - center).Dot(direction);
		}


		void Update_from_endpoints(in Vector3d p0, in Vector3d p1)
		{
			center = 0.5 * (p0 + p1);
			direction = p1 - p0;
			extent = 0.5 * direction.Normalize();
		}


		// conversion operators
		public static implicit operator Segment3d(in Segment3f v) => new (v.Center, v.Direction, v.Extent);
		public static explicit operator Segment3f(in Segment3d v) => new ((Vector3f)v.center, (Vector3f)v.direction, (float)v.extent);


		// IParametricCurve3d interface

		
		public bool IsClosed => false;

		
		public double ParamLength => 1.0f;

		// t in range[0,1] spans arc
		public Vector3d SampleT(in double t)
		{
			return center + (((2 * t) - 1) * extent * direction);
		}

		public Vector3d TangentT(in double t)
		{
			return direction;
		}

		
		public bool HasArcLength => true;
		
		public double ArcLength => 2 * extent;

		public Vector3d SampleArcLength(in double a)
		{
			return P0 + (a * direction);
		}

		public void Reverse()
		{
			Update_from_endpoints(P1, P0);
		}

		public IParametricCurve3d Clone()
		{
			return new Segment3d(center, direction, extent);
		}


	}


	public struct Segment3f : ISerlize<Segment3f>
	{
		// Center-direction-extent representation.
		// Extent is half length of segment
		public Vector3f Center;
		public Vector3f Direction;
		public float Extent;

		public void Serlize(BinaryWriter binaryWriter) {
			Center.Serlize(binaryWriter);
			Direction.Serlize(binaryWriter);
			binaryWriter.Write(Extent);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			Center.DeSerlize(binaryReader);
			Direction.DeSerlize(binaryReader);
			Extent = binaryReader.ReadSingle();
		}

		public Segment3f(in Vector3f p0, in Vector3f p1)
		{
			//update_from_endpoints(p0, p1);
			Center = 0.5f * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5f * Direction.Normalize();
		}
		public Segment3f(in Vector3f center, in Vector3f direction, in float extent)
		{
			Center = center;
			Direction = direction;
			Extent = extent;
		}


		public void SetEndpoints(in Vector3f p0, in Vector3f p1)
		{
			Update_from_endpoints(p0, p1);
		}


		
		public Vector3f P0
		{
			get => Center - (Extent * Direction);
			set => Update_from_endpoints(value, P1);
		}
		
		public Vector3f P1
		{
			get => Center + (Extent * Direction);
			set => Update_from_endpoints(P0, value);
		}
		public float Length => 2 * Extent;

		// parameter is signed distance from center in direction
		public Vector3f PointAt(in float d)
		{
			return Center + (d * Direction);
		}


		// t ranges from [0,1] over [P0,P1]
		public Vector3f PointBetween(in float t)
		{
			return Center + (((2 * t) - 1) * Extent * Direction);
		}


		public float DistanceSquared(in Vector3f p)
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

		public Vector3f NearestPoint(in Vector3f p)
		{
			var t = (p - Center).Dot(Direction);
			return t >= Extent ? P1 : t <= -Extent ? P0 : Center + (t * Direction);
		}


		public float Project(in Vector3f p)
		{
			return (p - Center).Dot(Direction);
		}




		void Update_from_endpoints(in Vector3f p0, in Vector3f p1)
		{
			Center = 0.5f * (p0 + p1);
			Direction = p1 - p0;
			Extent = 0.5f * Direction.Normalize();
		}
	}

}
