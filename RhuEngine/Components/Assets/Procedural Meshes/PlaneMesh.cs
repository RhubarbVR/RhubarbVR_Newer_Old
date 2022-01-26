using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class PlaneMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public Sync<Vec2> dimensions;

		[OnChanged(nameof(LoadMesh))]
		public Sync<int> subdivisions;

		public override void FirstCreation() {
			base.FirstCreation();
			dimensions.Value = new Vec2(0.1f, 0.1f);
		}

		private void LoadMesh() {
			Load(Mesh.GeneratePlane(dimensions.Value, subdivisions.Value));
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
