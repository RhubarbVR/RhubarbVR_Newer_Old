using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class RoundedCubeMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public Sync<Vec3> dimensions;

		[OnChanged(nameof(LoadMesh))]
		public Sync<int> subdivisions;

		[OnChanged(nameof(LoadMesh))]
		public Sync<float> edgeRadius;

		public override void FirstCreation() {
			base.FirstCreation();
			dimensions.Value = new Vec3(0.1f, 0.1f, 0.1f);
			edgeRadius.Value = 0.1f;
		}

		private void LoadMesh() {
			Load(Mesh.GenerateRoundedCube(dimensions.Value, edgeRadius.Value,subdivisions.Value));
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
