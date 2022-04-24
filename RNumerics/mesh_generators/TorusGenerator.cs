using System;

namespace RNumerics
{
	public class TorusGenerator : Curve3Axis3RevolveGenerator
	{
		private Vector3d[] _curve;
		public float MajorRadius;
		public float MinorRadius;
		public int MajorSegments;
		public int MinorSegments;

		public TorusGenerator() : base() {
			Axis = new Frame3f(new Vector3f(1, 0, 0), new Quaternionf(0, 0, 0, 1));
			Capped = false;
			NoSharedVertices = true;
			startCapCenterIndex = -1;
			endCapCenterIndex = -1;
		}

		public override MeshGenerator Generate() {
			_curve = new Vector3d[MinorSegments + 1];
			for (var i = 0; i < MinorSegments + 1; i++) {
				_curve[i] = new Vector3d(
					(Math.Cos(i * (MathUtil.TWO_PI / MinorSegments)) * MinorRadius) + MajorRadius,
					Math.Sin(i * (MathUtil.TWO_PI / MinorSegments)) * MinorRadius,
					0);
			}
			return base.Generate();
		}
	}
}
