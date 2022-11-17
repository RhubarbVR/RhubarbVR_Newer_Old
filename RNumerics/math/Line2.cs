using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MessagePack;

namespace RNumerics
{
	[MessagePackObject]
	public struct Line2d
	{
		[Key(0)]
		public Vector2d origin;
		[Key(1)]
		public Vector2d direction;
		public Line2d() {
			origin = Vector2d.Zero;
			direction = Vector2d.Zero;
		}

		[Exposed, IgnoreMember]
		public Vector2d Origin
		{
			get => origin;
			set => origin = value;
		}
		[Exposed, IgnoreMember]
		public Vector2d Direction
		{
			get => direction;
			set => direction = value;
		}

		public Line2d(in Vector2d origin, in Vector2d direction) {
			this.origin = origin;
			this.direction = direction;
		}


		public static Line2d FromPoints(in Vector2d p0, in Vector2d p1) {
			return new Line2d(p0, (p1 - p0).Normalized);
		}

		// parameter is distance along Line
		public Vector2d PointAt(in double d) {
			return origin + (d * direction);
		}

		public double Project(in Vector2d p) {
			return (p - origin).Dot(direction);
		}

		public double DistanceSquared(in Vector2d p) {
			var t = (p - origin).Dot(direction);
			var proj = origin + (t * direction);
			return (proj - p).LengthSquared;
		}



		/// <summary>
		/// Returns:
		///   +1, on right of line
		///   -1, on left of line
		///    0, on the line
		/// </summary>
		public int WhichSide(in Vector2d test, in double tol = 0) {
			var x0 = test.x - origin.x;
			var y0 = test.y - origin.y;
			var x1 = direction.x;
			var y1 = direction.y;
			var det = (x0 * y1) - (x1 * y0);
			return det > tol ? +1 : (det < -tol ? -1 : 0);
		}



		/// <summary>
		/// Calculate intersection point between this line and another one.
		/// Returns Vector2d.MaxValue if lines are parallel.
		/// </summary>
		/// <returns></returns>
		public Vector2d IntersectionPoint(in Line2d other, in double dotThresh = MathUtil.ZERO_TOLERANCE) {
			// see IntrLine2Line2 for explanation of algorithm
			var diff = other.origin - origin;
			var D0DotPerpD1 = direction.DotPerp(other.direction);
			if (Math.Abs(D0DotPerpD1) > dotThresh) {                    // Lines intersect in a single point.
				var invD0DotPerpD1 = ((double)1) / D0DotPerpD1;
				var diffDotPerpD1 = diff.DotPerp(other.direction);
				var s = diffDotPerpD1 * invD0DotPerpD1;
				return origin + (s * direction);
			}
			// Lines are parallel.
			return Vector2d.MaxValue;
		}





		// conversion operators
		public static implicit operator Line2d(in Line2f v) => new(v.Origin, v.Direction);
		public static explicit operator Line2f(in Line2d v) => new((Vector2f)v.origin, (Vector2f)v.direction);


	}

	[MessagePackObject]
	public struct Line2f
	{
		[Key(0)]
		public Vector2f Origin;
		[Key(1)]
		public Vector2f Direction;

		public Line2f(in Vector2f origin, in Vector2f direction) {
			Origin = origin;
			Direction = direction;
		}

		// parameter is distance along Line
		public Vector2f PointAt(in float d) {
			return Origin + (d * Direction);
		}

		public float Project(in Vector2f p) {
			return (p - Origin).Dot(Direction);
		}

		public float DistanceSquared(in Vector2f p) {
			var t = (p - Origin).Dot(Direction);
			var proj = Origin + (t * Direction);
			return (proj - p).LengthSquared;
		}
	}
}
