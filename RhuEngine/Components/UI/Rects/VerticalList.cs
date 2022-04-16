using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI\\Rects" })]
	public class VerticalList : RawScrollUIRect
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
						item.AnchorMax = new Vector2f(1f,currentpos + inc);
						item.AnchorMin = new Vector2f(0f, currentpos);
						currentpos += inc;
					}
					_maxScroll = Vector2f.Zero;
					_minScroll = Vector2f.Zero;
					RLog.Info("Updated Face Rects");
				});
			}
			else {
				fakeRects.SafeOperation((list) => {
					var ypos = 0f;
					var ysize = 0f;
					var maxXpos = 0f;
					foreach (var item in list) {
						item.Canvas = Canvas;
						var targetSize = item.Child.AnchorMaxValue - item.Child.AnchorMinValue;
						maxXpos = Math.Max(targetSize.x, maxXpos);
						ypos += targetSize.y;
						item.AnchorMax = new Vector2f(1,ypos);
						item.AnchorMin = new Vector2f(0,ypos - 1);
					}
					_maxScroll = new Vector2f(0, 1 - ((ypos - 1)/10));
					_minScroll = new Vector2f(0,-1 - ((ypos - 1) / 10));
					RLog.Info($"Updated Face Rects {ypos}");
				});
			}
			base.UpdateUIMeshes();
		}
	}
}
