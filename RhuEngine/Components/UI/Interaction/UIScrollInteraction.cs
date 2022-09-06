using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/Interaction" })]
	public class UIScrollInteraction : UIInteractionComponent
	{
		[Default(true)]
		public readonly Sync<bool> AllowAltSwitch;

		public readonly Sync<Vector2f> MouseScrollSpeed;

		public readonly SyncDelegate<Action<Vector2f>> OnScroll;

		[Default(0.5f)]
		public readonly Sync<float> GripPressForce;

		protected override void OnAttach() {
			base.OnAttach();
			MouseScrollSpeed.Value = Vector2f.One;
		}

		private void Scroll(Vector2f vector) {
			OnScroll.Target?.Invoke(vector);
		}

		public bool Hover = false;

		protected override void Step() {
			base.Step();
			if (UIRect is null) {
				return;
			}
			var firstLazer = true;
			var hitposes = UIRect.GetRectHitData();
			foreach (var item in hitposes) {
				if (firstLazer && item.Lazer) {
					if (RInput.Mouse.ScrollChange != Vector2f.Zero) {
						if (AllowAltSwitch && RInput.Key(Key.Alt).IsActive()) {
							Scroll(new Vector2f(RInput.Mouse.ScrollChange.y, RInput.Mouse.ScrollChange.x) * MouseScrollSpeed);
						}
						else {
							Scroll(RInput.Mouse.ScrollChange * MouseScrollSpeed);
						}

					}
					firstLazer = false;
				}
			}
			//if (HasFirst) {
			//	//DragScroll 
			//	var scroll = UIRect.ClickGripChange(GripPressForce.Value);
			//	var scrollavrage = Vector2f.Zero;
			//	foreach (var item in scroll) {
			//		if (scrollavrage == Vector2f.Zero) {
			//			scrollavrage += item.Xy;
			//		}
			//	}

			//	if (scrollavrage != Vector2f.Zero) {
			//		Scroll(scrollavrage * UIRect.Canvas.scale.Value.Xy * -5);
			//	}
			//}
		}
	}
}
