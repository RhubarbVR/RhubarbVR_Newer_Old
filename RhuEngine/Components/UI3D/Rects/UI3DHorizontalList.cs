using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/UI3D" })]
	public sealed class UI3DHorizontalList : UI3DRect
	{
		[Default(false)]
		public readonly Sync<bool> Fit;

		[Default(false)]
		public readonly Sync<bool> FlipOrder;

		public override void ChildRectUpdate() {
			if (Fit) {
				var recList = new Stack<UI3DRect>();
				//Todo: make forLoop
				foreach (var item in Entity.children.Cast<Entity>()) {
					var rect = item.UIRect;
					if (rect is not null && item.IsEnabled && !item.IsDestroying) {
						recList.Push(rect);
					}
				}
				var size = recList.Count;
				var elmentySize = new Vector2f(CachedElementSize.x / size, CachedElementSize.y);
				for (var i = 0; i < size; i++) {
					var currenti = !FlipOrder ? size - i - 1 : i;
					var MoveAmount = new Vector2f(elmentySize.x * currenti, 0);
					var fakeMax = elmentySize + TrueMin + MoveAmount;
					var fakeMin = TrueMin + MoveAmount;
					var fakeBadMin = BadMin - MoveAmount;
					recList.Peek().StandardMinMaxCalculation(fakeMax, fakeMin, fakeBadMin);
					recList.Peek().RegisterNestedParentUpdate(false);
					recList.Pop();
				}
				CachedOverlapSize = CachedElementSize;
			}
			else {
				var recList = new Stack<UI3DRect>();
				//Todo: make forLoop
				foreach (var item in (FlipOrder ? Entity.children : Entity.children.Reverse()).Cast<Entity>()) {
					var rect = item.UIRect;
					if (rect is not null && item.IsEnabled && !item.IsDestroying) {
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
