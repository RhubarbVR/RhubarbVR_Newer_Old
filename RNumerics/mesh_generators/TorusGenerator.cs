using System;

namespace RNumerics
{
	public sealed class TorusGenerator : TubeGenerator
	{
		public float MajorRadius;
		public float MinorRadius;
		public int MajorSegments;
		public int MinorSegments;

		public TorusGenerator() : base() {
			Capped = false;
			NoSharedVertices = true;
			startCapCenterIndex = -1;
			endCapCenterIndex = -1;
			ClosedLoop = true;
		}

		public override MeshGenerator Generate() {
			Polygon = Polygon2d.MakeCircle(MinorRadius, MinorSegments);
			Vertices ??= new System.Collections.Generic.List<Vector3d>();
			Vertices.Clear();
			for (var i = 0; i < MajorSegments + 1; i++) {
				Vertices.Add( new Vector3d(
					Math.Cos(i * (MathUtil.TWO_PI / MajorSegments)) * (MajorRadius - (MinorRadius/2)),
					Math.Sin(i * (MathUtil.TWO_PI / MajorSegments)) * (MajorRadius - (MinorRadius / 2)),
					0));
			}
			return base.Generate();
		}
	}
}
