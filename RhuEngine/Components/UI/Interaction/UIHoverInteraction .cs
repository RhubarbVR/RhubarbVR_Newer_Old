using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	

	[Category(new string[] { "UI/Interaction" })]
	public class UIHoverInteraction : UIInteractionComponent
	{
		[Default(true)]
		public readonly Sync<bool> Laserable;

		[Default(true)]
		public readonly Sync<bool> Touchable;

		[Default(true)]
		public readonly Sync<bool> CustomTochable;

		[Default(true)]
		public readonly Sync<bool> AllowOtherZones;

		public readonly SyncDelegate OnHover;
		public readonly SyncDelegate OnUnHover;

		private void Hover() {
			if (!_isHovering) {
				RWorld.ExecuteOnEndOfFrame(this, () => OnHover.Target?.Invoke());
			}
			_isHovering = true;
		}
		private void UnHover() {
			_isHovering = false;
			RWorld.ExecuteOnEndOfFrame(this, () => OnUnHover.Target?.Invoke());
		}

		private bool _isHovering;

		//public override void Step() {
		//	base.Step();
		//	if (Rect is null) {
		//		return;
		//	}
		//	var hoverThisFrame = false;
		//	foreach (var item in Rect.HitPoses(!AllowOtherZones.Value)) {
		//		if (item.Laser) {
		//			if (Laserable) {
		//				hoverThisFrame = true;
		//				Hover();
		//			}
		//		}
		//		else if (item.CustomTouch) {
		//			if (CustomTochable) {
		//				hoverThisFrame = true;
		//				Hover();
		//			}
		//		}
		//		else {
		//			if (Touchable) {
		//				hoverThisFrame = true;
		//				Hover();
		//			}
		//		}
		//	}
		//	if (!hoverThisFrame && _isHovering) {
		//		UnHover();
		//	}
		//}
	}
}
