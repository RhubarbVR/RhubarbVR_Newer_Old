using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Rects" })]
	public class UIScrollInteraction : UIInteractionComponent
	{
		public Sync<bool> AllowOtherZones;

		public Sync<Vector2f> MouseScrollSpeed;

		public SyncDelegate<Action<Vector2f>> OnScroll;

		[Default(0.5f)]
		public Sync<float> FingerPressForce;

		public override void OnAttach() {
			base.OnAttach();
			MouseScrollSpeed.Value = Vector2f.One;
		}

		private void Scroll(Vector2f vector) {
			OnScroll.Target?.Invoke(vector);
		}

		public bool Hover = false;

		public override void Step() {
			base.Step();
			if(Rect is null) {
				return;
			}
			var HasFirst = false;
			var firstLazer = true;
			var hitposes = Rect.HitPoses(!AllowOtherZones.Value);
			foreach (var item in hitposes) {
				HasFirst = true;
				if (firstLazer && item.Laser) {
					if (RInput.Mouse.ScrollChange != Vector2f.Zero) {
						Scroll(RInput.Mouse.ScrollChange * MouseScrollSpeed * 5);
					}
					firstLazer = false;
				}
			}
			if (HasFirst) {
				//DragScroll 
				var scroll = Rect.ClickFingerChange(FingerPressForce.Value, !AllowOtherZones.Value);
				var scrollavrage = Vector2f.Zero;
				foreach (var item in scroll) {
					if (scrollavrage == Vector2f.Zero) {
						scrollavrage += item.Xy;
					}
				}

				if (scrollavrage != Vector2f.Zero) {
					Scroll(scrollavrage * Rect.Canvas.scale.Value.Xy * -5);
				}
			}
		}
	}
}
