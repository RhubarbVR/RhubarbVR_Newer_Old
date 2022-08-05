using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public abstract class UIComponent : Component
	{
		public UIRect UIRect => Entity.UIRect;

	}
}
