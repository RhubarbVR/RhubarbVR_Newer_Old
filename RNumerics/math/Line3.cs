using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RNumerics
{
	public struct Line3d : ISerlize<Line3d>
	{
		public Vector3d origin;
		public Vector3d direction;

		public void Serlize(BinaryWriter binaryWriter) {
			origin.Serlize(binaryWriter);
			direction.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			origin.DeSerlize(binaryReader);
			direction.DeSerlize(binaryReader);
		}

		[Exposed]
		public Vector3d Origin
		{
			get => origin;
			set => origin = value;
		}
		[Exposed]
		public Vector3d Direction
		{
			get => direction;
			set => direction = value;
		}
		public Line3d() {
			origin = Vector3d.Zero;
			direction = Vector3d.Zero;
		}
		public Line3d(in Vector3d origin, in Vector3d direction) {
			this.origin = origin;
			this.direction = direction;
		}

		// parameter is distance along Line
		public Vector3d PointAt(in double d) {
			return origin + (d * direction);
		}

		public double Project(in Vector3d p) {
			return (p - origin).Dot(direction);
		}

		public double DistanceSquared(in Vector3d p) {
			var t = (p - origin).Dot(direction);
			var proj = origin + (t * direction);
			return (proj - p).LengthSquared;
		}

		public Vector3d ClosestPoint(in Vector3d p) {
			var t = (p - origin).Dot(direction);
			return origin + (t * direction);
		}

		// conversion operators
		public static implicit operator Line3d(in Line3f v) => new(v.Origin, v.Direction);
		public static explicit operator Line3f(in Line3d v) => new((Vector3f)v.origin, (Vector3f)v.direction);


	}

	public struct Line3f : ISerlize<Line3f>
	{
		public Vector3f Origin;
		public Vector3f Direction;

		public void Serlize(BinaryWriter binaryWriter) {
			Origin.Serlize(binaryWriter);
			Direction.Serlize(binaryWriter);
		}

		public void DeSerlize(BinaryReader binaryReader) {
			Origin.DeSerlize(binaryReader);
			Direction.DeSerlize(binaryReader);
		}

		public Line3f(in Vector3f origin, in Vector3f direction) {
			Origin = origin;
			Direction = direction;
		}

		// parameter is distance along Line
		public Vector3f PointAt(in float d) {
			return Origin + (d * Direction);
		}

		public float Project(in Vector3f p) {
			return (p - Origin).Dot(Direction);
		}

		public float DistanceSquared(in Vector3f p) {
			var t = (p - Origin).Dot(Direction);
			var proj = Origin + (t * Direction);
			return (proj - p).LengthSquared;
		}

		public Vector3f ClosestPoint(in Vector3f p) {
			var t = (p - Origin).Dot(Direction);
			return Origin + (t * Direction);
		}
	}
}
