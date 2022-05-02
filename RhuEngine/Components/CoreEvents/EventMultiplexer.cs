using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class EventMultiplexer : Component
	{
		public Sync<int> Index;

		public SyncObjList<SyncDelegate> Events;

		[Exsposed]
		public void Call() {
			if(Index.Value < 0) {
				return;
			}
			if (Index.Value > Events.Count) {
				return;
			}
			Events[Index.Value].Target.Invoke();
		}		
	}
}
