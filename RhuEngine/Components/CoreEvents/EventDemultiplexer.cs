﻿using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class EventDemultiplexer : Component
	{
		public Linker<int> Event;

		public SyncDelegate Called;

		public SyncObjList<InnerEvent> Events;

		public class InnerEvent : SyncObject
		{
			[Exsposed]
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
