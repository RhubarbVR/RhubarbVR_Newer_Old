using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[Category(new string[] { "CoreData" })]
	public sealed class ValueField<T> : Component
	{
		public readonly Sync<T> Value;
	}
}
