using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[Category(new string[] { "CoreData" })]
	public sealed partial class ValueList<T> : Component
	{
		public readonly SyncValueList<T> Value;
	}
}
