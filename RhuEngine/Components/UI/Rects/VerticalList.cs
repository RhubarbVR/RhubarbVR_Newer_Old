using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
	public class VerticalList : UIRect
	{
		[Default(false)]
		public readonly Sync<bool> Fit;

		[Default(false)]
		public readonly Sync<bool> FlipOrder;

		public override void ChildRectUpdate() {
			if (Fit) {
				var recList = new Stack<UIRect>();
				foreach (Entity item in Entity.children) {
					var rect = item.UIRect;
					if (rect is not null) {
						recList.Push(rect);
					}
				}
				var size = recList.Count;
				var sizer = CachedElementSize.y / size;
				var fakeMax = new Vector2f(CachedElementSize.y / size, 1) + TrueMin;
				for (var i = 0; i < size; i++) {
					var currenti = !FlipOrder ? size - i - 1 : i;
					recList.Peek().StandardMinMaxCalculation(fakeMax + new Vector2f(0, sizer * currenti), TrueMin + new Vector2f(0, sizer * currenti), BadMin - new Vector2f(0, sizer * currenti));
					recList.Peek().RegisterNestedParentUpdate(false);
					recList.Pop();
				}
				CachedOverlapSize = CachedElementSize;
			}
			else {

			}
		}
	}
}
