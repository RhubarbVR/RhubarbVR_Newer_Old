using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public abstract class UIComponent: Component
	{
		public abstract void RenderUI();
	}
}
