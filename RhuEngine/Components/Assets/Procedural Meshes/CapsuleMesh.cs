using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed class CapsuleMesh : ProceduralMesh
	{
		[Default(32)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> Longitudes;
		[Default(16)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> Latitudes;
		[Default(0)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> Rings;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Depth;
		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Radius;
		[Default(CapsuleGenerator.UvProfile.Aspect)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<CapsuleGenerator.UvProfile> Profile;


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
	}
}
