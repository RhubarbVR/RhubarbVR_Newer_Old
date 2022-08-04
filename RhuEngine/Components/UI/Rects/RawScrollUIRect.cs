using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	public abstract class RawScrollUIRect : UIRect
	{
		public readonly Sync<Vector2f> ScrollPos;

		public readonly Sync<Vector2f> ScrollSpeed;
		public virtual Vector2f MaxScroll => Vector2f.Inf;


		public virtual Vector2f MinScroll => Vector2f.NInf;

		public override void OnAttach() {
			base.OnAttach();
			ScrollSpeed.Value = Vector2f.One;
		}

		[Exposed]
		public virtual void Scroll(Vector2f scrollpos) {
			
		}
	}
}
