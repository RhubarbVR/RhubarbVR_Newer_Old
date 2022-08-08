using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Visuals" })]
	public class UISprite : RenderUIComponent
	{
		public override RMaterial RenderMaterial => Material.Asset;
		public override Colorf RenderTint => Tint;
		[OnChanged(nameof(ForceUpdate))]
		public readonly SyncRef<SpriteProvder> Sprite;
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Vector2i> PosMin;
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<Vector2i> PosMax;
		
		[Exposed]
		public void SetPos(Vector2i pos) {
			PosMax.Value = pos;
			PosMin.Value = pos;
		}

		public readonly AssetRef<RMaterial> Material;

		public readonly Sync<Colorf> Tint;

		[Default(EVerticalAlien.Center)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<EVerticalAlien> VerticalAlien;

		[Default(EHorizontalAlien.Middle)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<EHorizontalAlien> HorizontalAlien;

		[Default(true)]
		[OnChanged(nameof(ForceUpdate))]
		public readonly Sync<bool> KeepAspectRatio;


		public override void OnAttach() {
			base.OnAttach();
			Tint.Value = Colorf.White;
		}

		protected override void UpdateMesh() {
			var mesh = new SimpleMesh();
			if (Sprite.Target is null) {
				StandaredBaseMesh = mesh;
				return;
			}
			if (Sprite.Target.Texture.Asset is null) {
				StandaredBaseMesh = mesh;
				return;
			}

			var startDepth = new Vector3f(0, 0, Entity.UIRect.CachedDepth);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			Vector3f upleft, upright, downleft, downright = upleft = upright = downleft = depthStart;
			var max = Max;
			var min = Min;
			var boxsize = max - min;
			boxsize /= Math.Max(boxsize.x, boxsize.y);
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			var maxoffset = max;
			var minoffset = min;
			var spriteSize = Sprite.Target.GetSizeOfSprite(PosMin, PosMax);
			if (KeepAspectRatio.Value) {
				var texture = new Vector2f(spriteSize.x, spriteSize.y);
				texture /= canvassize;
				texture /= boxsize;
				texture /= Math.Max(texture.x, texture.y);
				var maxmin = (max - min) * texture;
				maxoffset = maxmin + min;
				minoffset = min;

				var offset = (max - min - maxmin) / 2;
				if (HorizontalAlien == EHorizontalAlien.Middle) {
					maxoffset = new Vector2f(maxoffset.x + offset.x, maxoffset.y);
					minoffset = new Vector2f(minoffset.x + offset.x, minoffset.y);
				}
				if (VerticalAlien == EVerticalAlien.Center) {
					maxoffset = new Vector2f(maxoffset.x, maxoffset.y + offset.y);
					minoffset = new Vector2f(minoffset.x, minoffset.y + offset.y);
				}
				if (HorizontalAlien == EHorizontalAlien.Right) {
					maxoffset = new Vector2f(max.x, maxoffset.y);
					minoffset = new Vector2f(max.x - maxmin.x, minoffset.y);
				}
				if (VerticalAlien == EVerticalAlien.Top) {
					maxoffset = new Vector2f(maxoffset.x, max.y);
					minoffset = new Vector2f(minoffset.x, max.y - maxmin.y);
				}
			}
			upleft += new Vector3f(minoffset.x, maxoffset.y);
			upright += maxoffset.XY_;
			downright += new Vector3f(maxoffset.x, minoffset.y);
			downleft += minoffset.XY_;

			var (pos, size) = Sprite.Target.GetSpriteSizePoints(PosMin, PosMax);
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { pos + new Vector2f(0, size.y) }, bHaveC = false, v = downleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { pos + size }, bHaveC = false, v = downright });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { pos }, bHaveC = false, v = upleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { pos + new Vector2f(size.x, 0) }, bHaveC = false, v = upright });
			mesh.AppendTriangle(0, 1, 2);
			mesh.AppendTriangle(1, 3, 2);

			if (UIRect.ParentRect is null) {
				//Add back if first rec
				//Depth
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = downleft });
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = downright });
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upleft });
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upright });
				upleft -= depth;
				upright -= depth;
				downleft -= depth;
				downright -= depth;
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = downleft });
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = downright });
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upleft });
				mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upright });
				mesh.AppendTriangle(4, 8, 9);
				mesh.AppendTriangle(9, 5, 4);
				mesh.AppendTriangle(5, 9, 11);
				mesh.AppendTriangle(11, 7, 5);
				mesh.AppendTriangle(6, 10, 8);
				mesh.AppendTriangle(8, 4, 6);
				mesh.AppendTriangle(7, 11, 6);
				mesh.AppendTriangle(10, 6, 11);
				mesh.AppendTriangle(10, 9, 8);
				mesh.AppendTriangle(10, 11, 9);
			}
			StandaredBaseMesh = mesh;
		}
	}
}
