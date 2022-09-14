using System;

namespace RNumerics
{
	public sealed class Circle2d : IParametricCurve2d
	{
		public Vector2d Center;
		public double Radius;
		public bool IsReversed;     // use ccw orientation instead of cw

		public Circle2d(in double radius) {
			IsReversed = false;
			Center = Vector2d.Zero;
			Radius = radius;
		}

		public Circle2d(in Vector2d center, in double radius) {
			IsReversed = false;
			Center = center;
			Radius = radius;
		}


		public double Curvature => 1.0 / Radius;
		public double SignedCurvature => IsReversed ? (-1.0 / Radius) : (1.0 / Radius);


		public bool IsClosed => true;

		public void Reverse() {
			IsReversed = !IsReversed;
		}

		public IParametricCurve2d Clone() {
			return new Circle2d(Center, Radius) { IsReversed = IsReversed };
		}

		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform) {
			Center = xform.TransformP(Center);
			Radius = xform.TransformScalar(Radius);
		}



		// angle in range [0,360] (but works for any value, obviously)
		public Vector2d SampleDeg(in double degrees) {
			var theta = degrees * MathUtil.DEG_2_RAD;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
		}

		// angle in range [0,2pi] (but works for any value, obviously)
		public Vector2d SampleRad(in double radians) {
			double c = Math.Cos(radians), s = Math.Sin(radians);
			return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
		}


		public double ParamLength => 1.0f;

		// t in range[0,1] spans circle [0,2pi]
		public Vector2d SampleT(in double t) {
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
		}

		public Vector2d TangentT(in double t) {
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			var tangent = new Vector2d(-Math.Sin(theta), Math.Cos(theta));
			if (IsReversed) {
				tangent = -tangent;
			}

			tangent.Normalize();
			return tangent;
		}


		public bool HasArcLength => true;

		public double ArcLength => MathUtil.TWO_PI * Radius;

		public Vector2d SampleArcLength(in double a) {
			var t = a / ArcLength;
			var theta = IsReversed ? -t * MathUtil.TWO_PI : t * MathUtil.TWO_PI;
			double c = Math.Cos(theta), s = Math.Sin(theta);
			return new Vector2d(Center.x + (Radius * c), Center.y + (Radius * s));
		}


		public bool Contains(in Vector2d p) {
			var d = Center.DistanceSquared(p);
			return d <= Radius * Radius;
		}


		public double Circumference
		{
			get => MathUtil.TWO_PI * Radius;
			set => Radius = value / MathUtil.TWO_PI;
		}
		public double Diameter
		{
			get => 2 * Radius;
			set => Radius = value / 2;
		}
		public double Area
		{
			get => Math.PI * Radius * Radius;
			set => Radius = Math.Sqrt(value / Math.PI);
		}


		public AxisAlignedBox2d Bounds => new(Center, Radius, Radius);

		public double SignedDistance(in Vector2d pt) {
			var d = Center.Distance(pt);
			return d - Radius;
		}
		public double Distance(in Vector2d pt) {
			var d = Center.Distance(pt);
			return Math.Abs(d - Radius);
		}



		public static double RadiusArea(in double r) {
			return Math.PI * r * r;
		}
		public static double RadiusCircumference(in double r) {
			return MathUtil.TWO_PI * r;
		}

		/// <summary>
		/// Radius of n-sided regular polygon that contains circle of radius r
		/// </summary>
		public static double BoundingPolygonRadius(in double r, in int n) {
			var theta = MathUtil.TWO_PI / (double)n / 2.0;
			return r / Math.Cos(theta);
		}
	}
}
