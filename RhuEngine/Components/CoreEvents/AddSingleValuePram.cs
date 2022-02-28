using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class AddSingleValuePram<T> : Component
	{
		public Sync<T> Value;
		
		public SyncDelegate<Action<T>> Target;

		[Exsposed]
		public void Call() {
			Target.Target?.Invoke(Value);
		}
	}
}
