﻿using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
	public class HorizontalList : UIRect
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
				var sizer = CachedElementSize.x / size;
				var fakeMax = new Vector2f(CachedElementSize.x / size, 1) + CachedMin;
				for (var i = 0; i < size; i++) {
					var currenti = !FlipOrder ? size - i - 1 : i;
					recList.Peek().StandardMinMaxCalculation(fakeMax + new Vector2f(sizer * currenti, 0), TrueMin + new Vector2f(sizer * currenti, 0), BadMin - new Vector2f(sizer * currenti, 0));
					recList.Peek().RegisterNestedParentUpdate(false);
					recList.Pop();
				}
				CachedOverlapSize = CachedElementSize;
			}
			else {
				var recList = new Stack<UIRect>();
				foreach (Entity item in FlipOrder ? Entity.children.Reverse() : Entity.children) {
					var rect = item.UIRect;
					if (rect is not null) {
						recList.Push(rect);
					}
				}
				var size = recList.Count;
				var MoveVec = Vector2f.Zero;
				for (var i = 0; i < size; i++) {
					recList.Peek().StandardMinMaxCalculation(TrueMax + MoveVec, TrueMin + MoveVec, BadMin - MoveVec);
					MoveVec += recList.Peek().CachedOverlapSize * new Vector2f(1, 0);
					recList.Peek().RegisterNestedParentUpdate(false);
					recList.Pop();
				}
				CachedOverlapSize = MoveVec;
			}
		}
	}
}
