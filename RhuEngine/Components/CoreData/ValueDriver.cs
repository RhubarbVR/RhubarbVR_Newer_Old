﻿using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[UpdatingComponent]
	[Category(new string[] { "CoreData" })]
	public class ValueDriver<T> : Component
	{
		public readonly Linker<T> driver;

		public readonly SyncRef<IValueSource<T>> source;

		protected override void Step() {
			if (driver.Linked) {
				if (source.Target != null) {
					driver.LinkedValue = source.Target.Value;
				}
			}
		}
	}
}
