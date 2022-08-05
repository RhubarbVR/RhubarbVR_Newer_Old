using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Visuals" })]
	public class UIRectangle : RenderUIComponent
	{
		[Exposed]
		public void AddRoundingSettings() {
			var rounding = Entity.AttachComponent<UIRounding>();
			rounding.BindRect(this);
		}

		public readonly AssetRef<RMaterial> Material;
		public override RMaterial RenderMaterial => Material.Asset;

		public readonly Sync<Colorf> Tint;
		[Default(5)]
		public readonly Sync<int> RoundingSteps;
		[Default(0f)]
		public readonly Sync<float> Rounding;
		public readonly Sync<bool> FullBox;

		public override void OnAttach() {
			base.OnAttach();
			Tint.Value = Colorf.White;
		}

		protected override void UpdateMesh() {
			if(UIRect.Canvas is null) {
				return;
			}
			var mesh = new SimpleMesh();
			var startDepth = new Vector3f(0, 0, Entity.UIRect.CachedDepth);
			var depth = new Vector3f(0, 0, Entity.UIRect.Depth.Value);
			var depthStart = startDepth + depth;
			Vector3f upleft, upright, downleft, downright = upleft = upright = downleft = depthStart;
			var max = Max;
			var min = Min;
			upleft += new Vector3f(min.x, max.y);
			upright += max.XY_;
			downright += new Vector3f(max.x, min.y);
			downleft += min.XY_;
			var rotrationOffseter = new Vector3f(Rounding.Value / 100, Rounding.Value / 100, 0);
			rotrationOffseter /= UIRect.Canvas.scale.Value / 10;
			if (Rounding.Value > 0 && UIRect.ParentRect is not null && !FullBox.Value) {
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
			if (Rounding.Value <= 0 || UIRect.ParentRect is null || FullBox.Value) {
				if (UIRect.ParentRect is null || FullBox.Value) {
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

				int AddCurv(int currentindex, bool flipAngle, Vector2f multaple, Vector3f pos, int a, int b, int c) {
					var lastIndex = a;
					var startindex = currentindex;
					for (var i = 0; i < RoundingSteps.Value; i++) {
						var angle = Math.PI / 180 * (90 / RoundingSteps.Value) * (i + 1);
						if (flipAngle) {
							angle = MathUtil.HALF_PI - angle;
						}
						var cos = Math.Cos(angle) * (Rounding.Value / 100);
						var sin = Math.Sin(angle) * (Rounding.Value / 100);
						var bindpos = new Vector3f(cos * multaple.x, sin * multaple.y, 0) / (UIRect.Canvas.scale.Value / 10);
						mesh.AppendVertex(new NewVertexInfo { bHaveN = true, n = Vector3f.AxisY, bHaveUV = true, uv = new Vector2f[] { Vector2f.AxisY }, bHaveC = false, v = pos + bindpos });
						currentindex++;
						mesh.AppendTriangle(lastIndex, b, currentindex);
						lastIndex = currentindex;
					}
					//if (flipAngle) {
					//	mesh.AppendTriangle(c, b, startindex + 1);
					//	mesh.AppendTriangle(currentindex - 1, b, c);
					//}
					//else {
						mesh.AppendTriangle(a, b, startindex + 1);
						mesh.AppendTriangle(currentindex - 1, b, c);
					//}
					return currentindex;
				}
				var currentindex = 11;
				currentindex = AddCurv(currentindex, true, Vector2f.One, upright, 9, 3, 8);
				currentindex = AddCurv(currentindex, false, new Vector2f(-1, 1), upleft, 6, 2, 7);
				currentindex = AddCurv(currentindex, false, new Vector2f(1, -1), downright, 10, 1, 11);
				AddCurv(currentindex, true, -Vector2f.One, downleft, 5, 0, 4);
			}
			StandaredBaseMesh = mesh;
		}
	}
}
