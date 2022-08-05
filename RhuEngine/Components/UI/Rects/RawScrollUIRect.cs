using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	public abstract class RawScrollUIRect : UIRect
	{
		[OnChanged(nameof(ApplyScrollMovement))]
		public readonly Sync<Vector2f> ScrollPos;

		public void ApplyScrollMovement() {
			ApplyMovement(ScrollPos.Value);
		}

		public readonly Sync<Vector2f> ScrollSpeed;

		public virtual Vector2f MaxScroll => CachedOverlapSize;
		public virtual Vector2f MinScroll => -CachedOverlapSize;

		public override void OnAttach() {
			base.OnAttach();
			ScrollSpeed.Value = Vector2f.One;
		}

		[Exposed]
		public virtual void Scroll(Vector2f scrollpos) {
			ScrollPos.Value += scrollpos;
		}
	}
}
