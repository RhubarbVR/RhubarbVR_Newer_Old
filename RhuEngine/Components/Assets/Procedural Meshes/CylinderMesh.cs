using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class CylinderMesh : ProceduralMesh
	{

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> BaseRadius;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> TopRadius;
		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> Height;
		[Default(0.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> StartAngleDeg;
		[Default(360.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> EndAngleDeg;
		[Default(16)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<int> Slices;
		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> HasCaps;
		[Default(false)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> Clockwise;

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

		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (HasCaps.Value) 
			{
				var mesh = new CappedCylinderGenerator {
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
				mesh.Generate();
				GenMesh(mesh.MakeSimpleMesh());
			}
			else 
			{
				var mesh = new OpenCylinderGenerator {
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
				mesh.Generate();
				GenMesh(mesh.MakeSimpleMesh());
			}
		}
	}
}
