using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class RectangleMesh : ProceduralMesh
	{

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector2f> Dimensions;

		[OnChanged(nameof(LoadMesh))]
		public Sync<Vector3f> Normal;


		[OnChanged(nameof(LoadMesh))]
		public Sync<Index2i> IndicesMap;

		[Default(TrivialRectGenerator.UVModes.FullUVSquare)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<TrivialRectGenerator.UVModes> UVMode;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> WantUVs;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public Sync<bool> WantNormals;

		public override void FirstCreation() {
			base.FirstCreation();
			IndicesMap.Value = new(1, 3);
			Dimensions.Value = Vector2f.One;
			Normal.Value = Vector3f.AxisY;
		}

		private void LoadMesh() {
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

		public override void OnLoaded() {
			base.OnLoaded();
			LoadMesh();
		}
	}
}
