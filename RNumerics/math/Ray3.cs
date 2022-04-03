using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Ray3d
	{
		[Key(0)]
		public Vector3d Origin;
		[Key(1)]
		public Vector3d Direction;

		public Ray3d(Vector3d origin, Vector3d direction, bool bIsNormalized = false)
		{
			Origin = origin;
			Direction = direction;
			if (bIsNormalized == false && Direction.IsNormalized == false) {
				Direction.Normalize();
			}
		}

		public Ray3d(Vector3f origin, Vector3f direction)
		{
			Origin = origin;
			Direction = direction;
			Direction.Normalize();     // float cast may not be normalized in double, is trouble in algorithms!
		}

		// parameter is distance along ray
		public Vector3d PointAt(double d)
		{
			return Origin + (d * Direction);
		}


		public double Project(Vector3d p)
		{
			return (p - Origin).Dot(Direction);
		}

		public double DistanceSquared(Vector3d p)
		{
			var t = (p - Origin).Dot(Direction);
			if (t < 0)
			{
				return Origin.DistanceSquared(p);
			}
			else
			{
				var proj = Origin + (t * Direction);
				return (proj - p).LengthSquared;
			}
		}

		public Vector3d ClosestPoint(Vector3d p)
		{
			var t = (p - Origin).Dot(Direction);
			return t < 0 ? Origin : Origin + (t * Direction);
		}


		// conversion operators
		public static implicit operator Ray3d(Ray3f v) => new (v.Origin, ((Vector3d)v.Direction).Normalized);
		public static explicit operator Ray3f(Ray3d v) => new ((Vector3f)v.Origin, ((Vector3f)v.Direction).Normalized);
	}


	[MessagePackObject]
	public struct Ray3f
	{
		[Key(0)]
		public Vector3f Origin;
		[Key(1)]
		public Vector3f Direction;

		public Ray3f(Vector3f origin, Vector3f direction, bool bIsNormalized = false)
		{
			Origin = origin;
			Direction = direction;
			if (bIsNormalized == false && Direction.IsNormalized == false) {
				Direction.Normalize();
			}
		}

		// parameter is distance along ray
		public Vector3f PointAt(float d)
		{
			return Origin + (d * Direction);
		}

		public float Project(Vector3f p)
		{
			return (p - Origin).Dot(Direction);
		}

		public float DistanceSquared(Vector3f p)
		{
			var t = (p - Origin).Dot(Direction);
			var proj = Origin + (t * Direction);
			return (proj - p).LengthSquared;
		}
	}
}
