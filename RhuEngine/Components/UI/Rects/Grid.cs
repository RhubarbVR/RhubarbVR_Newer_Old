using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
	public sealed class Grid : RawScrollUIRect
	{
		[Default(2)]
		public readonly Sync<int> GridWidth;
		[Default(2)]
		public readonly Sync<int> GridHeight;
		public readonly Sync<bool> FlipOrder;

		public override void ChildRectUpdate() {
			var recList = new Stack<UIRect>();
			//Todo: UseForLoop
			foreach (var item in (!FlipOrder? Entity.children.Reverse(): Entity.children).Cast<Entity>()) {
				var rect = item.UIRect;
				if (rect is not null && item.IsEnabled && !item.IsDestroying) {
					recList.Push(rect);
				}
			}
			var size = recList.Count;
			var elmentySize = new Vector2f(CachedElementSize.x / GridWidth, CachedElementSize.y/ GridHeight);
			var MoveAmount = new Vector2f(CachedElementSize.x, CachedElementSize.y);
			var yPos = 0f;
			for (var i = 0; i < size; i++) {
				MoveAmount += new Vector2f(elmentySize.x, 0);
				if (MoveAmount.x >= CachedElementSize.x) {
					MoveAmount = new Vector2f(0, MoveAmount.y - elmentySize.y);
					yPos += elmentySize.y;
				}
				var fakeMax = elmentySize + TrueMin + MoveAmount;
				var fakeMin = TrueMin + MoveAmount;
				var fakeBadMin = BadMin - MoveAmount;
				recList.Peek().StandardMinMaxCalculation(fakeMax, fakeMin, fakeBadMin);
				recList.Peek().RegisterNestedParentUpdate(false);
				recList.Pop();
			}
			CachedOverlapSize = new Vector2f(CachedElementSize.x, Math.Max(yPos, CachedElementSize.y));
		}

	}
}
