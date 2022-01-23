using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class SphereMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		[Default(1f)]
		public Sync<float> diameter;

		[OnChanged(nameof(LoadMesh))]
		[Default(4)]
		public Sync<int> subdivisions;

		public override void FirstCreation() {
			base.FirstCreation();
		}

		private void LoadMesh() {
			Load(Mesh.GenerateSphere(diameter.Value, subdivisions.Value));
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
