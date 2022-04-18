using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class TrivialBox3Mesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector3f> Extent;

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector3f> Center;

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector3f> AxisX;

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector3f> AxisY;

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector3f> AxisZ;

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
			Extent.Value = new Vector3f(0.5f);
			AxisX.Value = Vector3f.AxisX;
			AxisY.Value = Vector3f.AxisY;
			AxisZ.Value = Vector3f.AxisZ;
		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var box = new TrivialBox3Generator {
				Box = new Box3f { Extent = Extent.Value, Center = Center.Value, AxisX = AxisX.Value, AxisY = AxisY.Value, AxisZ = AxisZ.Value },
				NoSharedVertices = NoSharedVertices.Value,
				WantUVs = WantUVs,
				WantNormals = WantNormals,
			};
			box.Generate();
			GenMesh(box.MakeSimpleMesh());
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
