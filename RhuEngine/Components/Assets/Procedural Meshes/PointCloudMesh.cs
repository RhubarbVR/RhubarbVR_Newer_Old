using System;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class PointCloudMesh : ProceduralMesh {

		public SyncValueList<int> iList;

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<double> Radius;

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new PointSplatsGenerator {
				Radius = Radius,
				PointIndices = (int[]) iList,
				PointF = PointF.Target,
				NormalF = NormalF.Target
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}

		public SyncDelegate<Func<int, Vector3d>> PointF;
		public SyncDelegate<Func<int, Vector3d>> NormalF;



	}
}
