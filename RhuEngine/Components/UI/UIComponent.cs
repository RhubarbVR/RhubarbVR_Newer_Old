using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI" })]
	public abstract class UIComponent : Component
	{
		public SimpleMesh MainMesh { get; set; }

		public abstract RMaterial RenderMaterial { get; }
		public abstract Colorf RenderTint { get; }

		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UIRect Rect => Entity.UIRect;

		public abstract void ProcessBaseMesh();

		public void ProcessMesh() {
			ProcessBaseMesh();
			Entity.UIRect?.UpdateMeshes();
		}

	}
}
