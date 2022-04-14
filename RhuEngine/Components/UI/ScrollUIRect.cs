using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Normal)]
	[Category(new string[] { "UI" })]
	public class ScrollUIRect : RawScrollUIRect
	{
		public Sync<Vector2f> Max_Scroll;
		public Sync<Vector2f> Min_Scroll;
		public override Vector2f MaxScroll => Max_Scroll;
		public override Vector2f MinScroll => Min_Scroll;

		public override void OnAttach() {
			base.OnAttach();
			Max_Scroll.Value = Vector2f.Inf;
			Min_Scroll.Value = Vector2f.NInf;
		}
	}
}
