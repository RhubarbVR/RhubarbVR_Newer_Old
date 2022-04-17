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

		public Sync<Vector2f> TouchOverShot;

		public Sync<Vector2f> MouseScrollSpeed;

		public SyncDelegate<Action<Vector2f>> OnScroll;

		public override void OnAttach() {
			base.OnAttach();
			TouchOverShot.Value = new Vector2f(0.5f);
			MouseScrollSpeed.Value = Vector2f.One;
		}

		private void Scroll(Vector2f vector) {
			RWorld.ExecuteOnStartOfFrame(() => OnScroll.Target?.Invoke(vector));
		}

		public bool Hover = false;

		private Vector2f _lastVol;

		public override void Step() {
			base.Step();
			if(Rect is null) {
				return;
			}
			var HasFirst = false;
			var firstLazer = true;
			foreach (var item in Rect.HitPoses(!AllowOtherZones.Value)) {
				HasFirst = true;
				if (firstLazer && item.Laser) {
					if (RInput.Mouse.ScrollChange != Vector2f.Zero) {
						Scroll(RInput.Mouse.ScrollChange * MouseScrollSpeed * 5);
					}
					firstLazer = false;
				}
			}
			if (!HasFirst && _lastVol != Vector2f.Zero) {
				Scroll(_lastVol * TouchOverShot);
				_lastVol = Vector2f.Zero;
			}
			if (HasFirst) {
				//DragScroll 
				var scroll = Rect.ClickFingerChange(0.7f, !AllowOtherZones.Value);
				var scrollavrage = Vector2f.Zero;
				foreach (var item in scroll) {
					if (scrollavrage == Vector2f.Zero) {
						scrollavrage += item.Xy;
					}
				}

				if (scrollavrage != Vector2f.Zero) {
					_lastVol = scrollavrage * Rect.Canvas.scale.Value.Xy * -5;
					Scroll(_lastVol);
				}
			}
		}
	}
}
