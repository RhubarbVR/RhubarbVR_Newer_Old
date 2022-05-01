using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI\\Rects" })]
	public class HorizontalList : RawScrollUIRect
	{
		[Default(false)]
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public Sync<bool> Fit;

		public override bool RemoveFakeRecs => false;

		public SafeList<BasicRectOvride> fakeRects = new();

		private Vector2f _maxScroll = Vector2f.Inf;

		private Vector2f _minScroll = Vector2f.NInf;

		public override Vector2f MaxScroll => _maxScroll;

		public override Vector2f MinScroll => _minScroll;

		public override void ChildRectAdded() {
			RegUpdateUIMeshes();
		}

		public override void UpdateUIMeshes() {
			fakeRects.SafeOperation((list) => list.Clear());
			_childRects.SafeOperation((list) => {
				foreach (var item in list) {
					if(item is null) {
						continue;
					}
					var fakeRec = new BasicRectOvride {
						Child = item,
						ParentRect = this,
						Canvas = Canvas,
						DepthValue = 0,
						AnchorMax = Vector2f.One,
						AnchorMin = Vector2f.Zero,
					};
					item.SetOverride(fakeRec);
					fakeRects.SafeAdd(fakeRec);
				}
			});
			if (Fit) {
				fakeRects.SafeOperation((list) => {
					var inc = 1f / list.Count;
					var currentpos = 0f;
					foreach (var item in list) {
						item.Canvas = Canvas;
						item.AnchorMax = new Vector2f(currentpos + inc,1f);
						item.AnchorMin = new Vector2f(currentpos,0f);
						currentpos += inc;
					}
					_maxScroll = Vector2f.Zero;
					_minScroll = Vector2f.Zero;
				});
			}
			else {
				fakeRects.SafeOperation((list) => {
					var xpos = 0f;
					var maxypos = 0f;
					foreach (var item in list) {
						item.Canvas = Canvas;
						var targetSize = item.Child.AnchorMaxValue - item.Child.AnchorMinValue;
						maxypos = Math.Max(targetSize.y, maxypos);
						xpos += targetSize.x;
						item.AnchorMax = new Vector2f(xpos,1);
						item.AnchorMin = new Vector2f(xpos - 1, 0);
					}
					_maxScroll = new Vector2f(1 - ((xpos - 1)/10),0);
					_minScroll = new Vector2f(-1 - ((xpos - 1) / 10),0);
				});
			}
			base.UpdateUIMeshes();
		}
	}
}
