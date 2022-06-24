using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	public abstract class RawScrollUIRect : UIRect
	{
		[OnChanged(nameof(ScrollPosChange))]
		public readonly Sync<Vector2f> ScrollPos;

		public readonly Sync<Vector2f> ScrollSpeed;
		public virtual Vector2f MaxScroll => Vector2f.Inf;


		public virtual Vector2f MinScroll => Vector2f.NInf;

		public override void OnAttach() {
			base.OnAttach();
			ScrollSpeed.Value = Vector2f.One;
		}

		internal void ScrollPosChange() {
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					item.Scroll(ScrollPos.Value.XY_ + ParentRect.ScrollOffset);
				}
			});
		}

		public override void OnLoaded() {
			base.OnLoaded();
			ScrollPosChange();
		}

		[Exposed]
		public void Scroll(Vector2f scroll) {
			scroll = scroll * ScrollSpeed.Value / Canvas.scale.Value.Xy;
			if (!(scroll.x == 0 && scroll.y == 0)) {
				var newvalue = ScrollPos.Value + scroll;
				ScrollPos.Value = MathUtil.Clamp(newvalue, MinScroll, MaxScroll);
			}
		}
	}
}
