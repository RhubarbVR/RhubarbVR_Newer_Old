using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using RNumerics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class TorusMesh : ProceduralMesh
	{
		private Vector3d[] _curve;
		private static readonly Frame3f _axis = new Frame3f(new Vector3f(1, 0, 0), new Quaternionf(0, 0, 0, 1));
		private const bool CAPPED = false;
		private const bool NO_SHARED_VERTICES = true;
		private const int START_CAP_CENTER_INDEX = -1;
		private const int END_CAP_CENTER_INDEX = -1;

		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> MinorRadius;

		[Default(2.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> MajorRadius;

		[Default(48)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> MajorSegments;

		[Default(12)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> MinorSegments;

		public override void ComputeMesh() {
			_curve = new Vector3d[MinorSegments + 1];
			for (var i = 0; i < MinorSegments + 1; i++) {
				_curve[i] = new Vector3d(
					(Math.Cos(i * (MathUtil.TWO_PI / MinorSegments)) * MinorRadius) + MajorRadius,
					Math.Sin(i * (MathUtil.TWO_PI / MinorSegments)) * MinorRadius,
					0);
			}

			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new Curve3Axis3RevolveGenerator {
				Curve = _curve,
				Axis = _axis,
				Capped = CAPPED,
				Slices = MajorSegments,
				NoSharedVertices = NO_SHARED_VERTICES,
				startCapCenterIndex = START_CAP_CENTER_INDEX,
				endCapCenterIndex = END_CAP_CENTER_INDEX
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
