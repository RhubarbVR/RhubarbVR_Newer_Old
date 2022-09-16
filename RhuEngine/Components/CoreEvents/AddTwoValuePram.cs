﻿using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[Category(new string[] { "CoreEvents" })]
	public sealed class AddTwoValuePram<T1,T2> : Component
	{ 
		public readonly Sync<T1> FirstValue;
		public readonly Sync<T2> SecondValue;

		public readonly SyncDelegate<Action<T1,T2>> Target;

		[Exposed]
		public void Call() {
			Target.Target?.Invoke(FirstValue, SecondValue);
		}
	}
}
