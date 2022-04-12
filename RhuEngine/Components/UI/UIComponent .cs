using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public abstract class UIComponent : Component
	{
		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UIRect Rect => Entity.UIRect;

		public abstract void Render(Matrix matrix);

		public abstract void RenderTargetChange();
	}
}
