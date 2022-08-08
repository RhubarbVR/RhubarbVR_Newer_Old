using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class RenderUIComponent : BaseRenderUIComponent
	{
		public abstract Colorf RenderTint { get; }
		public abstract RMaterial RenderMaterial { get; }
		public RMesh mesh;

		public SimpleMesh StandaredBaseMesh;
		public SimpleMesh MovedMesh;
		public SimpleMesh FinalMesh;

		public override void OnLoaded() {
			base.OnLoaded();
			UIRect?.RenderComponents?.SafeAdd(this);
		}

		protected virtual void UpdateMesh() {

		}

		protected virtual void MovedMeshUpdate() {
			MovedMesh = StandaredBaseMesh?.Copy();
			if (MovedMesh is null) {
				return;
			}
			MovedMesh.Translate(MoveAmount);
			if (HasBeenCut) {
				MovedMesh = MovedMesh.Cut(CutMax, CutMin);
			}
		}
		protected virtual void FinalMeshUpdate() {
			FinalMesh = MovedMesh?.Copy();
			if (FinalMesh is null) {
				return;
			}
			if (UIRect.Canvas.TopOffset.Value) {
				FinalMesh.OffsetTop(UIRect.Canvas.TopOffsetValue.Value);
			}
			if (UIRect.Canvas.FrontBind.Value) {
				FinalMesh = FinalMesh.UIBind(UIRect.Canvas.FrontBindAngle.Value, UIRect.Canvas.FrontBindRadus.Value, UIRect.Canvas.FrontBindSegments.Value, UIRect.Canvas.scale);
			}
			FinalMesh.Scale(UIRect.Canvas.scale.Value.x / 10, UIRect.Canvas.scale.Value.y / 10, UIRect.Canvas.scale.Value.z / 10);
		}

		public void UpdateRMeshForRender() {
			mesh ??= new RMesh((IMesh)null, true);
			mesh.LoadMesh(FinalMesh);
		}

		public override void Render(Matrix pos, int Depth) {
			if (RenderMaterial is null) {
				return;
			}
			mesh?.Draw(RenderMaterial, pos, RenderTint, Depth);
		}

		public override void ProcessMeshUpdate() {
			switch (UIRect.RenderMeshUpdate) {
				case UIRect.RenderMeshUpdateType.FullResized:
					UpdateMesh();
					MovedMeshUpdate();
					FinalMeshUpdate();
					UpdateRMeshForRender();
					break;
				case UIRect.RenderMeshUpdateType.Movment:
					MovedMeshUpdate();
					FinalMeshUpdate();
					UpdateRMeshForRender();
					break;
				case UIRect.RenderMeshUpdateType.CutMesh:
					MovedMeshUpdate();
					FinalMeshUpdate();
					UpdateRMeshForRender();
					break;
				case UIRect.RenderMeshUpdateType.BindAndCanvasScale:
					FinalMeshUpdate();
					UpdateRMeshForRender();
					break;
				default:
					break;
			}
		}
	}
}
