﻿using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public sealed class EventMultiplexer : Component
	{
		public readonly Sync<int> Index;

		public readonly SyncObjList<SyncDelegate> Events;

		[Exposed]
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
