using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[UpdatingComponent]
	[Category(new string[] { "CoreData" })]
	public sealed partial class ValueMultiDriver<T> : Component
	{
		public readonly SyncObjList<Linker<T>> drivers;

		public readonly SyncRef<IValueSource<T>> source;

		protected override void Step() {
			for (var i = 0; i < drivers.Count; i++) {
				var driver = drivers[i];
				if (driver.Linked) {
					if (source.Target != null) {
						driver.LinkedValue = source.Target.Value;
					}
				}
			}
		}
	}
}
