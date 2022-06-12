using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class RemoveSinglePram<T> : Component
	{
		public readonly Linker<T> Linker;

		public readonly SyncDelegate Target;

		[Exposed]
		public void Call(T value) {
			if (Linker.Linked) {
				Linker.LinkedValue = value;
			}
			Target.Target?.Invoke();
		}
	}
}
