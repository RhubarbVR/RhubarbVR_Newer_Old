using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Rects" })]
	public class ScrollUIRect : RawScrollUIRect
	{
		public readonly Sync<Vector2f> Max_Scroll;
		public readonly Sync<Vector2f> Min_Scroll;
		public override Vector2f MaxScroll => Max_Scroll;
		public override Vector2f MinScroll => Min_Scroll;

		public override void OnAttach() {
			base.OnAttach();
			Max_Scroll.Value = Vector2f.Inf;
			Min_Scroll.Value = Vector2f.NInf;
		}
	}
}
