using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public abstract class RenderUIComponent : UIComponent
	{
		public SimpleMesh MainMesh { get; set; }

		public SimpleMesh ScrollMesh { get; set; }

		public abstract RMaterial RenderMaterial { get; }
		public abstract Colorf RenderTint { get; }

		public abstract void ProcessBaseMesh();

		public void ProcessMesh() {
			ProcessBaseMesh();
			Entity.UIRect?.UpdateMeshes();
		}

		public override void RenderTargetChange() {
			ProcessBaseMesh();
		}
		public override void Render(Matrix matrix) {
			//Not needed becase meshLoader
		}

		public void RenderScrollMesh(bool updateMesh = true) {
			if (MainMesh is null) {
				RenderTargetChange();
			}
			var scrollMesh = new SimpleMesh(MainMesh);
			scrollMesh.Translate(Rect.ScrollOffset.x, Rect.ScrollOffset.y, 0);
			ScrollMesh = scrollMesh;
			if (updateMesh) {
				RWorld.ExecuteOnMain(this, Rect.UpdateMeshes);
			}
		}
	}
}
