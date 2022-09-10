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
	public class VerticalList : UIRect
	{
		[Default(false)]
		public readonly Sync<bool> Fit;

		[Default(false)]
		public readonly Sync<bool> FlipOrder;

		[Default(0f)]
		public readonly Sync<float> Padding;


		public override void FowParrentRectUpdate() {
			base.FowParrentRectUpdate();
			ChildRectUpdate();
		}

		public override void ChildRectUpdate() {
			if (Fit) {
				var recList = new Stack<UIRect>();
				//Todo: make forLoop
				foreach (var item in Entity.children.Cast<Entity>()) {
					var rect = item.UIRect;
					if (rect is not null && item.IsEnabled && !item.IsDestroying) {
						recList.Push(rect);
					}
				}
				var size = recList.Count;
				var elmentySize = new Vector2f(CachedElementSize.x, CachedElementSize.y / size);
				for (var i = 0; i < size; i++) {
					var currenti = !FlipOrder ? size - i - 1 : i;
					var MoveAmount = new Vector2f(0, elmentySize.y * currenti);
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
				var recList = new Stack<UIRect>();
				//Todo: make forLoop
				foreach (var item in (FlipOrder ? Entity.children: Entity.children.Reverse()).Cast<Entity>()) {
					var rect = item.UIRect;
					if (rect is not null && item.IsEnabled && !item.IsDestroying) {
						recList.Push(rect);
					}
				}
				var size = recList.Count;
				var MoveVec = new Vector2f(0,1);
				for (var i = 0; i < size; i++) {
					MoveVec -= recList.Peek().CachedOverlapSize * new Vector2f(0, 1);
					MoveVec -= new Vector2f(0, Padding.Value);
					recList.Peek().StandardMinMaxCalculation(TrueMax + MoveVec, TrueMin + MoveVec, BadMin - MoveVec);
					recList.Peek().RegisterNestedParentUpdate(false);
					recList.Pop();
				}
				CachedOverlapSize = MoveVec;
			}
		}
	}
}
