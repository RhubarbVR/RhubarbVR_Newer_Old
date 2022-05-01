using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class RemoveSinglePram<T> : Component
	{
		public Linker<T> Linker;

		public SyncDelegate Target;

		[Exsposed]
		public void Call(T value) {
			if (Linker.Linked) {
				Linker.LinkedValue = value;
			}
			Target.Target?.Invoke();
		}
	}
}
