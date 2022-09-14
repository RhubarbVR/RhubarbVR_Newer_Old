using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed class RectangleMesh : ProceduralMesh
	{

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> Dimensions;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> Normal;


		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Index2i> IndicesMap;

		[Default(TrivialRectGenerator.UVModes.FullUVSquare)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<TrivialRectGenerator.UVModes> UVMode;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantUVs;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantNormals;

		protected override void FirstCreation() {
			base.FirstCreation();
			IndicesMap.Value = new(1, 3);
			Dimensions.Value = Vector2f.One;
			Normal.Value = Vector3f.AxisY;
		}

		public override void ComputeMesh() {
			try {
				if (!Engine.EngineLink.CanRender) {
					return;
				}
				var mesh = new TrivialRectGenerator {
					Width = Dimensions.Value.x,
					Height = Dimensions.Value.y,
					UVMode = UVMode,
					Normal = Normal,
					IndicesMap = IndicesMap,
					WantUVs = WantUVs.Value,
					WantNormals = WantNormals,
				};
				mesh.Generate();
				GenMesh(mesh.MakeSimpleMesh());
			}catch(Exception e) {
				RLog.Err("Error Loading rec mesh " + e);
			}
		}
	}
}
