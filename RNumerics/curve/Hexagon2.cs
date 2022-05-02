using System;

namespace RNumerics
{
	public class Hexagon2d
	{
		public enum TopModes
		{
			Flat = 0,
			Tip = 1
		}

		public Vector2d Center;
		public double Radius;   // distance from center to corners
		public TopModes TopMode;

		public Hexagon2d(Vector2d center, double radius, TopModes mode = TopModes.Flat)
		{
			Center = center;
			Radius = radius;
			TopMode = mode;
		}


		public bool IsClosed => true;


		public Hexagon2d Clone()
		{
			return new Hexagon2d(Center, Radius, TopMode);
		}



		public double InnerRadius
		{
			get => MathUtil.SQRT_THREE * Radius / 2.0;
			set => Radius = 2.0 * value / MathUtil.SQRT_THREE;
		}


		public Vector2d Corner(int i)
		{
			var angle_deg = 60.0 * (double)i;
			if (TopMode == TopModes.Tip) {
				angle_deg += 30;
			}

			var angle_rad = angle_deg * MathUtil.DEG_2_RAD;
			return new Vector2d(Center.x + (Radius * Math.Cos(angle_rad)),
								Center.y + (Radius * Math.Sin(angle_rad)));
		}


		public double Width => (TopMode == TopModes.Flat) ? (Radius * 2) : (MathUtil.SQRT_THREE / 2.0 * Height);
		public double Height => (TopMode == TopModes.Flat) ? (MathUtil.SQRT_THREE / 2.0 * Width) : (Radius * 2);


		public double VertSpacing => (TopMode == TopModes.Flat) ? Height : (Height * 3.0 / 4.0);

		public double HorzSpacing => (TopMode == TopModes.Flat) ? (Width * 3.0 / 4.0) : Width;



		//public bool HasArcLength { get {return true;} }

		//public double ArcLength {
		//	get {
		//		return MathUtil.TwoPI * Radius;
		//	}
		//}


		//public Vector2d SampleArcLength(double a) {
		//	double t = a / ArcLength;
		//	double theta = (IsReversed) ? -t*MathUtil.TwoPI : t*MathUtil.TwoPI;
		//	double c = Math.Cos(theta), s = Math.Sin(theta);
		//	return new Vector2d(Center.x + Radius*c, Center.y + Radius*s);
		//}


		//      public bool Contains (Vector2d p ) {
		//          double d = Center.DistanceSquared(p);
		//          return d <= Radius * Radius;
		//      }


		//      public double Circumference {
		//	get { return MathUtil.TwoPI * Radius; }
		//          set { Radius = value / MathUtil.TwoPI; }
		//}
		//      public double Diameter {
		//	get { return 2 * Radius; }
		//          set { Radius = value / 2; }
		//}
		//public double Area {
		//    get { return Math.PI * Radius * Radius; }
		//    set { Radius = Math.Sqrt(value / Math.PI); }
		//}

		public AxisAlignedBox2d Bounds => new (Center, Width / 2, Height / 2);

		//public double SignedDistance(Vector2d pt)
		//{
		//    double d = Center.Distance(pt);
		//    return d - Radius;
		//}
		//public double Distance(Vector2d pt)
		//{
		//    double d = Center.Distance(pt);
		//    return Math.Abs(d - Radius);
		//}

	}
}
