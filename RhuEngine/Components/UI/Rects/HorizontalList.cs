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
		public readonly Sync<bool> Fit;

		public override bool RemoveFakeRecs => false;

		public readonly SafeList<BasicRectOvride> fakeRects = new();

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
						item.AnchorMin = new Vector2f(currentpos,0f);
						currentpos += inc;
						item.AnchorMax = new Vector2f(currentpos, 1f);
					}
					_maxScroll = Vector2f.Zero;
					_minScroll = Vector2f.Zero;
				});
			}
			else {
				fakeRects.SafeOperation((list) => {
					var xpos = 0f;
					Vector2f? min = null;
					var max = Vector2f.Zero;
					foreach (var item in list) {
						item.Canvas = Canvas;
						item.ParentRect = this;
						var targetSize = item.Child.AnchorMaxValue - item.Child.AnchorMinValue;
						item.AnchorMin = new Vector2f(xpos, 0);
						item.AnchorMax = new Vector2f(xpos+1f,1);
						xpos += targetSize.x;
						min ??= item.Child.Min;
						max = item.Child.Max;
					}
					var scollmax = max - (min ?? Vector2f.Zero);
					var scale = Max - Min;
					if (scale.x >= scollmax.x) {
						_maxScroll = Vector2f.Zero;
						_minScroll = Vector2f.Zero;
					}
					else {
						_maxScroll = new Vector2f(0, 0);
						_minScroll = new Vector2f(-scollmax.x + scale.x, -(scollmax.y - scale.y));
					}
				});
			}
			base.UpdateUIMeshes();
		}
	}
}
