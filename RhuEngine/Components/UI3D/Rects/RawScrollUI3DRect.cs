using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	public abstract class RawScrollUI3DRect : UI3DRect
	{
		[OnChanged(nameof(ApplyScrollMovement))]
		public readonly Sync<Vector2f> ScrollPos;

		public void ApplyScrollMovement() {
			ScrollPos.Value = MathUtil.Clamp(ScrollPos.Value, MinScroll, MaxScroll);
			ApplyMovement(ScrollPos.Value);
		}

		public readonly Sync<Vector2f> ScrollSpeed;

		public Vector2f GetMaxScroll() {
			return -new Vector2f(0, -CachedOverlapSize.y + CachedElementSize.y);
		}
		public Vector2f GetMinScroll() {
			return -new Vector2f(CachedOverlapSize.x - CachedElementSize.x, 0);
		}
		public virtual Vector2f MaxScroll => GetMaxScroll();
		public virtual Vector2f MinScroll => GetMinScroll();

		protected override void OnAttach() {
			base.OnAttach();
			ScrollSpeed.Value = Vector2f.One;
		}

		[Exposed]
		public virtual void Scroll(Vector2f scrollpos) {
			ScrollPos.Value += scrollpos;
		}
	}
}
