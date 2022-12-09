using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed class TrivialBox3MeshAndStick : ProceduralMesh
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

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> BaseRadius;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> TopRadius;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Height;
		[Default(0.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> StartAngleDeg;
		[Default(360.0f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> EndAngleDeg;
		[Default(16)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<int> Slices;
		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> HasCaps;
		[Default(false)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> Clockwise;

		protected override void FirstCreation() {
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
			SimpleMesh stick;
			if (HasCaps.Value) {
				var stickgen = new CappedCylinderGenerator {
					BaseRadius = BaseRadius,
					Height = Height,
					StartAngleDeg = StartAngleDeg,
					EndAngleDeg = EndAngleDeg,
					TopRadius = TopRadius,
					Slices = Slices,
					NoSharedVertices = NoSharedVertices.Value,
					WantUVs = WantUVs,
					WantNormals = WantNormals,
					Clockwise = Clockwise,
				};
				stickgen.Generate();
				stick = stickgen.MakeSimpleMesh();
			}
			else {
				var stickgen = new OpenCylinderGenerator {
					BaseRadius = BaseRadius,
					Height = Height,
					StartAngleDeg = StartAngleDeg,
					EndAngleDeg = EndAngleDeg,
					TopRadius = TopRadius,
					Slices = Slices,
					NoSharedVertices = NoSharedVertices.Value,
					WantUVs = WantUVs,
					WantNormals = WantNormals,
					Clockwise = Clockwise,
				};
				stickgen.Generate();
				stick = stickgen.MakeSimpleMesh();
			}


			var box = new TrivialBox3Generator {
				Box = new Box3f { extent = Extent.Value, center = Center.Value, axisX = AxisX.Value, axisY = AxisY.Value, axisZ = AxisZ.Value },
				NoSharedVertices = NoSharedVertices.Value,
				WantUVs = WantUVs,
				WantNormals = WantNormals,
			};
			box.Generate();
			var mesh = box.MakeSimpleMesh();
			mesh.AppendMesh(stick);
			GenMesh(mesh);
		}
	}
}
