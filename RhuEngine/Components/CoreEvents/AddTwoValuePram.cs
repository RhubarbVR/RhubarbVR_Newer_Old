using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class AddTwoValuePram<T1,T2> : Component
	{
		public Sync<T1> FirstValue;
		public Sync<T2> SecondValue;

		public SyncDelegate<Action<T1,T2>> Target;

		[Exsposed]
		public void Call() {
			Target.Target?.Invoke(FirstValue, SecondValue);
		}
	}
}
