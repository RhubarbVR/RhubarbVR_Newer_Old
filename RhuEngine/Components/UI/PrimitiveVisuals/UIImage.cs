using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/PrimitiveVisuals" })]
	public class UIImage : RenderUIComponent
	{
		[OnAssetLoaded(nameof(ProcessMesh))]
		public AssetRef<RTexture2D> Texture;

		public AssetRef<RMaterial> Material;

		public Sync<Colorf> Tint;

		[Default(true)]
		[OnChanged(nameof(ProcessMesh))]
		public Sync<bool> CenterX;

		[Default(true)]
		[OnChanged(nameof(ProcessMesh))]
		public Sync<bool> CenterY;

		[Default(true)]
		[OnChanged(nameof(ProcessMesh))]
		public Sync<bool> KeepAspectRatio;

		public override RMaterial RenderMaterial => Material.Asset;
		public override Colorf RenderTint => Tint.Value;

		public override void OnAttach() {
			base.OnAttach();
			Tint.Value = Colorf.White;
		}

		public override void ProcessBaseMesh() {
			var mesh = new SimpleMesh();
			var startDepth = new Vector3f(0, 0, Entity.UIRect.StartPoint);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			Vector3f upleft , upright , downleft , downright = upleft = upright = downleft = depthStart;
			var texture = Vector2f.One;
			var max = Rect.Max;
			var min = Rect.Min;
			var boxsize = max - min;
			boxsize /= Math.Max(boxsize.x, boxsize.y);
			var canvassize = Entity.UIRect.Canvas?.scale.Value.Xy ?? Vector2f.One;
			if (KeepAspectRatio.Value) {
				texture = new Vector2f(Texture.Asset?.Width ?? 1, Texture.Asset?.Height ?? 1);
				texture /= canvassize;
				texture /= boxsize;
				texture /= Math.Max(texture.x, texture.y);
			}
			var maxmin = (max - min) * texture;
			var maxoffset = maxmin + min;
			var minoffset = min;

			var offset = (max - min - maxmin) / 2;
			if (CenterX) {
				maxoffset = new Vector2f(maxoffset.x + offset.x, maxoffset.y);
				minoffset = new Vector2f(minoffset.x + offset.x, minoffset.y);
			}
			if (CenterY) {
				maxoffset = new Vector2f(maxoffset.x, maxoffset.y + offset.y);
				minoffset = new Vector2f(minoffset.x, minoffset.y + offset.y);
			}
			upleft += new Vector3f(minoffset.x, maxoffset.y);
			upright += maxoffset.XY_;
			downright += new Vector3f(maxoffset.x, minoffset.y);
			downleft += minoffset.XY_;

			mesh.AppendVertex(new NewVertexInfo { bHaveN = true,n = Vector3f.AxisY, bHaveUV = true,uv = new Vector2f[] { Vector2f.AxisY },bHaveC = false,v = downleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.One },  bHaveC = false, v = downright });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisX }, bHaveC = false, v = upright });
			mesh.AppendTriangle(0, 1, 2);
			mesh.AppendTriangle(1, 3, 2);


			//Depth
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero },  bHaveC = false, v = downleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero },  bHaveC = false, v = downright });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero },  bHaveC = false, v = upleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero },  bHaveC = false, v = upright });
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
			if (Rect.ParentRect is null) {
				//Add back if first rec
				mesh.AppendTriangle(10, 9, 8);
				mesh.AppendTriangle(10, 11, 9);
			}
			MainMesh = mesh;
		}
	}
}
