using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	

	[Category(new string[] { "UI/Interaction" })]
	public sealed class UIHoverInteraction : UIInteractionComponent
	{
		[Default(true)]
		public readonly Sync<bool> Laserable;

		[Default(true)]
		public readonly Sync<bool> Touchable;

		[Default(true)]
		public readonly Sync<bool> CustomTochable;

		public readonly SyncDelegate OnHover;
		public readonly SyncDelegate OnUnHover;

		private void Hover() {
			if (!_isHovering) {
				RUpdateManager.ExecuteOnEndOfFrame(this, () => OnHover.Target?.Invoke());
			}
			_isHovering = true;
		}
		private void UnHover() {
			_isHovering = false;
			RUpdateManager.ExecuteOnEndOfFrame(this, () => OnUnHover.Target?.Invoke());
		}

		private bool _isHovering;

		protected override void Step() {
			base.Step();
			if (UIRect is null) {
				return;
			}
			var hoverThisFrame = false;
			foreach (var item in UIRect.GetRectHitData()) {
				if (item.Lazer) {
					if (Laserable) {
						hoverThisFrame = true;
						Hover();
					}
				}
				else if (item.CustomTouch) {
					if (CustomTochable) {
						hoverThisFrame = true;
						Hover();
					}
				}
				else {
					if (Touchable) {
						hoverThisFrame = true;
						Hover();
					}
				}
			}
			if (!hoverThisFrame && _isHovering) {
				UnHover();
			}
		}
	}
}
