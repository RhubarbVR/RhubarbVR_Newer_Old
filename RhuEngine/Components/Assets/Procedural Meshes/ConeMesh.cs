using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class ConeMesh : ProceduralMesh
	{

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> BaseRadius;

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> Height;

		[Default(0.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> StartAngleDeg;

		[Default(360.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> EndAngleDeg;

		[Default(16)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> Slices;


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
			var mesh = new ConeGenerator {
				BaseRadius = BaseRadius,
				Height = Height,
				StartAngleDeg = StartAngleDeg,
				EndAngleDeg = EndAngleDeg,
				Slices = Slices,
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
