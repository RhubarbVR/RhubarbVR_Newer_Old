using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[Category(new string[] { "CoreEvents" })]
	public class AddSingleValuePram<T> : Component
	{
		public readonly Sync<T> Value;

		public readonly SyncDelegate<Action<T>> Target;

		[Exposed]
		public void Call() {
			Target.Target?.Invoke(Value);
		}
	}
}
