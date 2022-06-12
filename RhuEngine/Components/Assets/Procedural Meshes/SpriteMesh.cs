using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using RNumerics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public class SpriteMesh : ProceduralMesh
	{
		[OnChanged(nameof(LoadMesh))]
		public readonly SyncRef<SpriteProvder> Sprite;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> PosMin;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2i> PosMax;

		[Exposed]
		public void SetPos(Vector2i pos) {
			PosMax.Value = pos;
			PosMin.Value = pos;
		}

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> Dimensions;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> Normal;


		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Index2i> IndicesMap;

		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantUVs;


		[Default(true)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> WantNormals;

		public override void FirstCreation() {
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
				var (pos, size) = (Vector2f.Zero, Vector2f.One);
				if (Sprite.Target is not null) {
					(pos, size) = Sprite.Target.GetSpriteSizePoints(PosMin, PosMax);
				}
				var mesh = new SpriteRectGenerator {
					Width = Dimensions.Value.x,
					Height = Dimensions.Value.y,
					Normal = Normal,
					IndicesMap = IndicesMap,
					WantUVs = WantUVs.Value,
					WantNormals = WantNormals,
					uvbottom = pos.y,
					uvtop = (pos + size).y,
					uvleft = pos.x,
					uvright = (pos + size).x,
				};
				mesh.Generate();
				GenMesh(mesh.MakeSimpleMesh());
			}catch(Exception e) {
				RLog.Err("Error Loading rec mesh " + e);
			}
		}
	}
}
