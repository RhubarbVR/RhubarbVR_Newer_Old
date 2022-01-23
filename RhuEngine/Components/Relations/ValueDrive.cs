using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Relations" })]
	public class ValueDriver<T> : Component, IUpdatingComponent
	{
		public Linker<T> driver;

		public SyncRef<IValueSource<T>> source;

		public override void Step() {
			if (driver.Linked) {
				if (source.Target != null) {
					driver.LinkedValue = source.Target.Value;
				}
			}
		}
	}
}
