using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	public abstract class UIInteractionComponent : Component
	{
		[NoShow]
		[NoSave]
		[NoSync]
		[NoLoad]
		public UIRect Rect => Entity.UIRect;

	}
}
