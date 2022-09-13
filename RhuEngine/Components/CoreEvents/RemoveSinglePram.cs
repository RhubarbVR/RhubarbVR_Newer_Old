using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public sealed class RemoveSinglePram<T> : Component
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
