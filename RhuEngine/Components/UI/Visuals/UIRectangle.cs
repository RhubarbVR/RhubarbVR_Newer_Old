using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Visuals" })]
	public class UIRectangle : RenderUIComponent
	{
		public readonly AssetRef<RMaterial> Material;

		public readonly Sync<Colorf> Tint;
		[Default(5)]
		[OnChanged(nameof(ProcessMesh))]
		public readonly Sync<int> RoundingSteps;
		[Default(1f)]
		[OnChanged(nameof(ProcessMesh))]
		public readonly Sync<float> Rounding;
		public override RMaterial RenderMaterial => Material.Asset;
		public override Colorf RenderTint => Tint.Value;

		public override bool HasPhysics => true;

		public override void OnAttach() {
			base.OnAttach();
			Tint.Value = Colorf.White;
		}

		public override void ProcessBaseMesh() {
			var mesh = new SimpleMesh();
			var startDepth = new Vector3f(0, 0, Entity.UIRect.StartPoint);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			Vector3f upleft, upright, downleft, downright = upleft = upright = downleft = depthStart;
			var max = Rect.Max;
			var min = Rect.Min;
			upleft += new Vector3f(min.x, max.y);
			upright += max.XY_;
			downright += new Vector3f(max.x, min.y);
			downleft += min.XY_;
			var rotrationOffseter = new Vector3f(Rounding.Value / 100, Rounding.Value / 100, 0);
			rotrationOffseter /= Rect.Canvas.scale.Value / 10;
			if (Rounding.Value > 0) {				
				upright -= rotrationOffseter;
				downleft += rotrationOffseter;
				downright += new Vector3f(-1 * rotrationOffseter.x, rotrationOffseter.y);
				upleft += new Vector3f(rotrationOffseter.x, -1 * rotrationOffseter.y);
			}
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = downleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.One }, bHaveC = false, v = downright });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.Zero }, bHaveC = false, v = upleft });
			mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisX }, bHaveC = false, v = upright });
			mesh.AppendTriangle(0, 1, 2);
			mesh.AppendTriangle(1, 3, 2);
			if (Rounding.Value <= 0) {
				if (Rect.ParentRect is null) {
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
			}
			else {
				if (Rounding.Value > 0) {
					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = downleft - new Vector3f(rotrationOffseter.x, 0, 0) });
					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = downleft - new Vector3f(0, rotrationOffseter.y, 0) });

					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = upleft - new Vector3f(rotrationOffseter.x, 0, 0) });
					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = upleft + new Vector3f(0, rotrationOffseter.y, 0) });

					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = upright + new Vector3f(rotrationOffseter.x, 0, 0) });
					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = upright + new Vector3f(0, rotrationOffseter.y, 0) });

					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = downright + new Vector3f(rotrationOffseter.x, 0, 0) });
					mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = downright - new Vector3f(0, rotrationOffseter.y, 0) });
					mesh.AppendTriangle(5, 1, 0);
					mesh.AppendTriangle(5, 11, 1);

					mesh.AppendTriangle(3, 9, 7);
					mesh.AppendTriangle(3, 7, 2);

					mesh.AppendTriangle(2, 6, 4);
					mesh.AppendTriangle(0, 2, 4);

					mesh.AppendTriangle(3, 10, 8);
					mesh.AppendTriangle(1, 10, 3);


				}
				if (Rect.ParentRect is null) {
					//Add back if first rec

				}

			}
			MainMesh = mesh;
		}
	}
}
