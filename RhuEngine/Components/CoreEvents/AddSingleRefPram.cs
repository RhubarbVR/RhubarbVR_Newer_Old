using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class AddSingleRefPram<T> : Component where T : class, IWorldObject
	{
		public SyncRef<T> Ref;
		
		public SyncDelegate<Action<T>> Target;

		[Exsposed]
		public void Call() {
			Target.Target?.Invoke(Ref.Target);
		}
	}
}
