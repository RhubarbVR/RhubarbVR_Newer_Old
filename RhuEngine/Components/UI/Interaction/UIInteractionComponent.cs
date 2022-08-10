using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	public abstract class UIInteractionComponent : Component
	{
		public UIRect UIRect => Entity.UIRect;

	}
}
