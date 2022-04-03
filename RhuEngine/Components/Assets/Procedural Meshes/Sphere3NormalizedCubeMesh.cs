using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class Sphere3NormalizedCubeMesh : ProceduralMesh
	{
		[Default(1f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> Diameter;

		[Default(Sphere3Generator_NormalizedCube.NormalizationTypes.CubeMapping)]
		[OnChanged(nameof(LoadMesh))]
		public Sphere3Generator_NormalizedCube.NormalizationTypes NormalizeType;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> NoSharedVertices;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> WantUVs;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> WantNormals;

		public override void FirstCreation() {
			base.FirstCreation();

		}

		private void LoadMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new Sphere3Generator_NormalizedCube {
				Radius = Diameter/2,
				NormalizeType = NormalizeType,
				NoSharedVertices = NoSharedVertices.Value,
				WantUVs = WantUVs,
				WantNormals = WantNormals,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
