using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreEvents" })]
	public class ButtonEventManager : Component
	{
		public readonly SyncDelegate Click;
		public readonly SyncDelegate Pressing;
		public readonly SyncDelegate Releases;

		public readonly Linker<Vector3f> WorldPos;

		public readonly Linker<Vector3f> WindowPos;

		public readonly Linker<uint> FingerIndex;

		public readonly Linker<float> Force;

		public readonly Linker<bool> Lazer;

		public readonly Linker<bool> Touch;

		public readonly Linker<bool> CustomTouch;

		[Exsposed]
		public void Call(ButtonEvent value) {
			if (value.IsPressing) {
				Pressing.Target?.Invoke();
			}
			if (value.IsClicked) {
				Click.Target?.Invoke();
			}
			if (value.IsReleased) {
				Releases.Target?.Invoke();
			}
			if (WorldPos.Linked) {
				WorldPos.LinkedValue = value.WorldPos;
			}
			if (WindowPos.Linked) {
				WindowPos.LinkedValue = value.WindowPos;
			}
			if (FingerIndex.Linked) {
				FingerIndex.LinkedValue = value.FingerIndex;
			}
			if (Force.Linked) {
				Force.LinkedValue = value.Force;
			}
			if (Lazer.Linked) {
				Lazer.LinkedValue = value.Lazer;
			}
			if (Touch.Linked) {
				Touch.LinkedValue = value.Touch;
			}
			if (CustomTouch.Linked) {
				CustomTouch.LinkedValue = value.CustomTouch;
			}
		}
	}
}
