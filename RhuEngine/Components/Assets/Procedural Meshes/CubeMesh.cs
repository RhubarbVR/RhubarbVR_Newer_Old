using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CubeMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public Sync<Vec3> dimensions;

		[OnChanged(nameof(LoadMesh))]
		public Sync<int> subdivisions;

		public override void FirstCreation() {
			base.FirstCreation();
			dimensions.Value = new Vec3(0.1f, 0.1f, 0.1f);
		}

		private void LoadMesh() {
			Load(Mesh.GenerateCube(dimensions.Value, subdivisions.Value));
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
