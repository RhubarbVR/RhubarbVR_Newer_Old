using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed partial class TrivialBox3Mesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> Extent;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> Center;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> AxisX;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> AxisY;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> AxisZ;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> NoSharedVertices;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantUVs;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantNormals;

		protected override void FirstCreation() {
			base.FirstCreation();
			Extent.Value = new Vector3f(1f);
			AxisX.Value = Vector3f.AxisX / 2f;
			AxisY.Value = Vector3f.AxisY / 2f;
			AxisZ.Value = Vector3f.AxisZ / 2f;
		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var box = new TrivialBox3Generator {
				Box = new Box3f { extent = Extent.Value, center = Center.Value, axisX = AxisX.Value, axisY = AxisY.Value, axisZ = AxisZ.Value },
				NoSharedVertices = NoSharedVertices.Value,
				WantUVs = WantUVs,
				WantNormals = WantNormals,
			};
			box.Generate();
			GenMesh(box.MakeSimpleMesh());
		}
	}
}
