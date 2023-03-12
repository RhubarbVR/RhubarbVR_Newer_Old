using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public sealed partial class EventDemultiplexer : Component
	{
		public readonly Linker<int> Event;

		public readonly SyncDelegate Called;

		public readonly SyncObjList<InnerEvent> Events;

		public sealed partial class InnerEvent : SyncObject
		{
			[Exposed]
			public void Call() {
				try {
					((EventDemultiplexer)Parent.Parent).CalledMethod(this);
				}catch {

				}
			}
		}
		public void CalledMethod(InnerEvent innerEvent) {
			var index = Events.IndexOf(innerEvent);
			if (Event.Linked) {
				Event.LinkedValue = index;
			}
			Called.Target.Invoke();
		}
	}
}
