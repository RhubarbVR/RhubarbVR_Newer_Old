using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public abstract class UI3DComponent : Component
	{
		public UI3DRect UIRect => Entity.UIRect;

	}
}
