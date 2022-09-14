using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessagePack;
namespace RNumerics
{
	[MessagePackObject]
	public struct Line3d
	{
		[Key(0)]
		public Vector3d Origin;
		[Key(1)]
		public Vector3d Direction;
		public Line3d() {
			Origin = Vector3d.Zero;
			Direction = Vector3d.Zero;
		}
		public Line3d(in Vector3d origin, in Vector3d direction)
		{
			Origin = origin;
			Direction = direction;
		}

		// parameter is distance along Line
		public Vector3d PointAt(in double d)
		{
			return Origin + (d * Direction);
		}

		public double Project(in Vector3d p)
		{
			return (p - Origin).Dot(Direction);
		}

		public double DistanceSquared(in Vector3d p)
		{
			var t = (p - Origin).Dot(Direction);
			var proj = Origin + (t * Direction);
			return (proj - p).LengthSquared;
		}

		public Vector3d ClosestPoint(in Vector3d p)
		{
			var t = (p - Origin).Dot(Direction);
			return Origin + (t * Direction);
		}

		// conversion operators
		public static implicit operator Line3d(in Line3f v) => new (v.Origin, v.Direction);
		public static explicit operator Line3f(in Line3d v) => new ((Vector3f)v.Origin, (Vector3f)v.Direction);


	}

	[MessagePackObject]
	public struct Line3f
	{
		[Key(0)]
		public Vector3f Origin;
		[Key(1)]
		public Vector3f Direction;

		public Line3f(in Vector3f origin, in Vector3f direction)
		{
			Origin = origin;
			Direction = direction;
		}

		// parameter is distance along Line
		public Vector3f PointAt(in float d)
		{
			return Origin + (d * Direction);
		}

		public float Project(in Vector3f p)
		{
			return (p - Origin).Dot(Direction);
		}

		public float DistanceSquared(in Vector3f p)
		{
			var t = (p - Origin).Dot(Direction);
			var proj = Origin + (t * Direction);
			return (proj - p).LengthSquared;
		}

		public Vector3f ClosestPoint(in Vector3f p)
		{
			var t = (p - Origin).Dot(Direction);
			return Origin + (t * Direction);
		}
	}
}
