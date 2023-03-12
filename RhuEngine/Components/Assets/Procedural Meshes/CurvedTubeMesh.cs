using System.Collections.Generic;

using Assimp;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed partial class CurvedTubeMesh : ProceduralMesh
	{
		[Default(0.005)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<double> Radius;
		[Default(3)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> Steps;
		[Default(25)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> CurveSteps;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> AngleShiftRad;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> Capped;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> Clockwise;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> OverrideCapCenter;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantUVs;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> ClosedLoop;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3d> Startpoint;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3d> Endpoint;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3d> EndHandle;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3d> StartHandle;
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2d> CapCenter;

		protected override void FirstCreation() {
			base.FirstCreation();
			Endpoint.Value = new Vector3d(0, 0, -1);
			EndHandle.Value = new Vector3d(0, 0, 0);
			StartHandle.Value = new Vector3d(0, 0, -1);
		}
		private void LoadCurve(TubeGenerator mesh) {
			mesh.Vertices ??= new List<Vector3d>();
			mesh.Vertices.Clear();
			for (var i = 0; i < CurveSteps.Value; i++) {
				var poser = i / ((float)CurveSteps.Value - 1);
				mesh.Vertices.Add(Vector3d.Bezier(Startpoint.Value, StartHandle.Value, EndHandle.Value, Endpoint.Value, poser));
			}
		}
		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new TubeGenerator {
				Clockwise = Clockwise.Value,
				OverrideCapCenter = OverrideCapCenter.Value,
				CapCenter = CapCenter.Value,
				ClosedLoop = ClosedLoop.Value,
				WantUVs = WantUVs.Value,
				Capped = Capped.Value,
				Polygon = Polygon2d.MakeCircle(Radius.Value, Steps.Value, AngleShiftRad.Value)
			};
			LoadCurve(mesh);
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}