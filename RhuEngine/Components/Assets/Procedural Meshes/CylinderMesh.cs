using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CylinderMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		[Default(1f)]
		public Sync<float> depth;

		[OnChanged(nameof(LoadMesh))]
		[Default(1f)]
		public Sync<float> diameter;

		[OnChanged(nameof(LoadMesh))]
		[Default(16)]
		public Sync<int> subdivisions;

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vec3> direction;

		public override void FirstCreation() {
			base.FirstCreation();
			direction.Value = Vec3.Forward;
		}

		private void LoadMesh() {
			Load(Mesh.GenerateCylinder(diameter.Value, depth.Value, direction.Value));
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
