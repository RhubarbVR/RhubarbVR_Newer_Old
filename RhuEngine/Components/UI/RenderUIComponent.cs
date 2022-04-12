using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public abstract class RenderUIComponent : UIComponent
	{
		public SimpleMesh MainMesh { get; set; }

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
	}
}
