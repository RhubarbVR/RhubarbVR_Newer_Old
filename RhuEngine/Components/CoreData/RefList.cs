using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	public sealed class RefList<T> : Component where T :class , IWorldObject
	{
		public readonly SyncObjList<SyncRef<T>> Refs;
	}
}
