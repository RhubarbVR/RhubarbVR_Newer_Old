using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	public sealed partial class RefField<T> : Component where T :class , IWorldObject
	{
		public readonly SyncRef<T> Ref;
	}
}
