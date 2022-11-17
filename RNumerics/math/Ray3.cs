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
		public Vector3d origin;
		[Key(1)]
		public Vector3d direction;

		[Exposed, IgnoreMember]
		public Vector3d Origin
		{
			get => origin;
			set => origin = value;
		}
		[Exposed, IgnoreMember]
		public Vector3d Direction
		{
			get => direction;
			set => direction = value;
		}
		public Ray3d(in Vector3d origin, in Vector3d direction, in bool bIsNormalized = false)
		{
			this.origin = origin;
			this.direction = direction;
			if (bIsNormalized == false && this.direction.IsNormalized == false) {
				this.direction.Normalize();
			}
		}

		public Ray3d(in Vector3f origin, in Vector3f direction)
		{
			this.origin = origin;
			this.direction = direction;
			this.direction.Normalize();     // float cast may not be normalized in double, is trouble in algorithms!
		}

		// parameter is distance along ray
		public Vector3d PointAt(in double d)
		{
			return origin + (d * direction);
		}


		public double Project(in Vector3d p)
		{
			return (p - origin).Dot(direction);
		}

		public double DistanceSquared(in Vector3d p)
		{
			var t = (p - origin).Dot(direction);
			if (t < 0)
			{
				return origin.DistanceSquared(p);
			}
			else
			{
				var proj = origin + (t * direction);
				return (proj - p).LengthSquared;
			}
		}

		public Vector3d ClosestPoint(in Vector3d p)
		{
			var t = (p - origin).Dot(direction);
			return t < 0 ? origin : origin + (t * direction);
		}


		// conversion operators
		public static implicit operator Ray3d(in Ray3f v) => new (v.origin, ((Vector3d)v.direction).Normalized);
		public static explicit operator Ray3f(in Ray3d v) => new ((Vector3f)v.origin, ((Vector3f)v.direction).Normalized);
	}


	[MessagePackObject]
	public struct Ray3f
	{
		[Key(0)]
		public Vector3f origin;
		[Key(1)]
		public Vector3f direction;

		[Exposed, IgnoreMember]
		public Vector3f Origin
		{
			get => origin;
			set => origin = value;
		}
		[Exposed, IgnoreMember]
		public Vector3f Direction
		{
			get => direction;
			set => direction = value;
		}
		public Ray3f(in Vector3f origin, in Vector3f direction, in bool bIsNormalized = false)
		{
			this.origin = origin;
			this.direction = direction;
			if (bIsNormalized == false && direction.IsNormalized == false) {
				direction.Normalize();
			}
		}

		// parameter is distance along ray
		public Vector3f PointAt(in float d)
		{
			return origin + (d * direction);
		}

		public float Project(in Vector3f p)
		{
			return (p - origin).Dot(direction);
		}

		public float DistanceSquared(in Vector3f p)
		{
			var t = (p - origin).Dot(direction);
			var proj = origin + (t * direction);
			return (proj - p).LengthSquared;
		}
	}
}
