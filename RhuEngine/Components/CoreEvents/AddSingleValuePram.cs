using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[Category(new string[] { "CoreEvents" })]
	public class AddSingleValuePram<T> : Component
	{
		public readonly Sync<T> Value;

		public readonly SyncDelegate<Action<T>> Target;

		[Exsposed]
		public void Call() {
			Target.Target?.Invoke(Value);
		}
	}
}
