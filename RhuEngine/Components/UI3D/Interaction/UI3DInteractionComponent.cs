using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	public abstract class UI3DInteractionComponent : Component
	{
		public UI3DRect UIRect => Entity.UIRect;

	}
}
