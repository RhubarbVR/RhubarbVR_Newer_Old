using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Visuals/UI3DAssemblers" })]
	public sealed class CheckBox : Component
	{
		[OnChanged(nameof(OpenChange))]
		public readonly Linker<Vector2i> Minicon;
		[OnChanged(nameof(OpenChange))]
		public readonly Linker<Vector2i> Maxicon;

		public readonly Sync<Vector2i> MaxOpen;
		public readonly Sync<Vector2i> MinOpen;

		public readonly Sync<Vector2i> MaxClose;
		public readonly Sync<Vector2i> MinClose;

		[OnChanged(nameof(OpenChange))]
		public readonly Sync<bool> Open;

		public readonly SyncDelegate<Action<bool>> StateChange;

		public void OpenChange() {
			if (Minicon.Linked) {
				Minicon.LinkedValue = Open.Value ? MinOpen.Value : MinClose.Value;
			}
			if (Maxicon.Linked) {
				Maxicon.LinkedValue = Open.Value ? MaxOpen.Value : MaxClose.Value;
			}
		}

		[Exposed]
		public void Click(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			Open.Value = !Open.Value;
			StateChange.Target?.Invoke(Open.Value);
		}
	}
}
