using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CapsuleMesh : ProceduralMesh
	{
		[Default(32)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> Longitudes;
		[Default(16)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> Latitudes;
		[Default(0)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> Rings;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> Depth;
		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> Radius;
		[Default(CapsuleGenerator.UvProfile.Aspect)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<CapsuleGenerator.UvProfile> Profile;


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
			var mesh = new CapsuleGenerator {
				Longitudes = Longitudes,
				Latitudes = Latitudes,
				Rings = Rings,
				Depth = Depth,
				Radius = Radius,
				Profile = Profile,
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
