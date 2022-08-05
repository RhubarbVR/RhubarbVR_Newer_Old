using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class Sphere3NormalizedCubeMesh : ProceduralMesh
	{
		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Radius;

		[Default(Sphere3Generator_NormalizedCube.NormalizationTypes.CubeMapping)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sphere3Generator_NormalizedCube.NormalizationTypes NormalizeType;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> NoSharedVertices;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantUVs;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantNormals;


		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new Sphere3Generator_NormalizedCube {
				Radius = Radius,
				NormalizeType = NormalizeType,
				NoSharedVertices = NoSharedVertices.Value,
				WantUVs = WantUVs,
				WantNormals = WantNormals,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}
