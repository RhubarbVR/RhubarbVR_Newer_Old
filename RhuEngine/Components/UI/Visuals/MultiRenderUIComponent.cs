using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class MultiRenderUIComponent : BaseRenderUIComponent
	{
		public abstract bool UseSingle { get; }
		public abstract Colorf RenderTintSingle { get; }
		public abstract Colorf[] RenderTint { get; }
		public abstract RMaterial[] RenderMaterial { get; }
		public RMesh[] mesh = Array.Empty<RMesh>();

		public SimpleMesh[] StandaredBaseMesh = Array.Empty<SimpleMesh>();
		public SimpleMesh[] MovedMesh = Array.Empty<SimpleMesh>();
		public SimpleMesh[] FinalMesh = Array.Empty<SimpleMesh>();

		public override void OnLoaded() {
			base.OnLoaded();
			UIRect?.RenderComponents?.SafeAdd(this);
		}

		protected abstract void UpdateMesh();

		protected virtual void MovedMeshUpdate() {
			Array.Resize(ref MovedMesh, StandaredBaseMesh.Length);
			for (var i = 0; i < StandaredBaseMesh.Length; i++) {
				MovedMesh[i] = StandaredBaseMesh[i]?.Copy();
				if (MovedMesh[i] is null) {
					return;
				}
				MovedMesh[i].Translate(MoveAmount);
				if (HasBeenCut) {
					MovedMesh[i] = MovedMesh[i].Cut(CutMax, CutMin);
				}
			}
		}
		protected virtual void FinalMeshUpdate() {
			Array.Resize(ref FinalMesh, MovedMesh.Length);
			for (var i = 0; i < MovedMesh.Length; i++) {
				FinalMesh[i] = MovedMesh[i]?.Copy();
				if (FinalMesh[i] is null) {
					return;
				}
				if (UIRect.Canvas.TopOffset.Value) {
					FinalMesh[i].OffsetTop(UIRect.Canvas.TopOffsetValue.Value);
				}
				if (UIRect.Canvas.FrontBind.Value) {
					FinalMesh[i] = FinalMesh[i].UIBind(UIRect.Canvas.FrontBindAngle.Value, UIRect.Canvas.FrontBindRadus.Value, UIRect.Canvas.FrontBindSegments.Value, UIRect.Canvas.scale);
				}
				FinalMesh[i].Scale(UIRect.Canvas.scale.Value.x / 10, UIRect.Canvas.scale.Value.y / 10, UIRect.Canvas.scale.Value.z / 10);
			}
		}

		public void UpdateRMeshForRender() {
			Array.Resize(ref mesh, FinalMesh.Length);
			for (var i = 0; i < FinalMesh.Length; i++) {
				mesh[i] ??= new RMesh((IMesh)null, true);
				mesh[i].LoadMesh(FinalMesh[i]);
			}
		}

		public override void Render(Matrix pos, int Depth) {
			if (UseSingle) {
				for (var i = 0; i < RenderMaterial.Length; i++) {
					if (RenderMaterial[i] is null) {
						continue;
					}
					if (mesh.Length <= i) {
						continue;
					}
					mesh[i]?.Draw(RenderMaterial[i], pos, RenderTintSingle, Depth);
				}
			}
			else {
				for (var i = 0; i < RenderMaterial.Length; i++) {
					if (RenderMaterial[i] is null) {
						continue;
					}
					if (mesh.Length <= i) {
						continue;
					}
					mesh[i]?.Draw(RenderMaterial[i], pos, RenderTint[i], Depth);
				}
			}
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
