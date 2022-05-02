using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class ArrowMesh : ProceduralMesh
	{
		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> StickRadius;

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> StickLength;

		[Default(1.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> HeadBaseRadius;

		[Default(0.0f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> TipRadius;

		[Default(0.5f)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<float> HeadLength;

		[Default(false)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> DoubleSided;

		public override void FirstCreation() {
			base.FirstCreation();
		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			var mesh = new Radial3DArrowGenerator {
				StickRadius = StickRadius,
				StickLength = StickLength,
				HeadBaseRadius = HeadBaseRadius,
				TipRadius = TipRadius,
				HeadLength = HeadLength,
				DoubleSided = DoubleSided,
			};
			mesh.Generate();
			GenMesh(mesh.MakeSimpleMesh());
		}
	}
}