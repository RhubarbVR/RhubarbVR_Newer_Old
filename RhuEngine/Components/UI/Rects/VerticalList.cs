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
		public readonly Sync<bool> Fit;

		public override bool RemoveFakeRecs => false;

		public readonly SafeList<BasicRectOvride> fakeRects = new();

		private Vector2f _maxScroll = Vector2f.Inf;

		private Vector2f _minScroll = Vector2f.NInf;

		public override Vector2f MaxScroll => _maxScroll;

		public override Vector2f MinScroll => _minScroll;

		public override void UpdateUIMeshes() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			UpdateMinMaxNoPross();
			fakeRects.SafeOperation((flist) => {
				flist.Clear();
				_childRects.SafeOperation((list) => {
					foreach (var item in list) {
						if (item is null) {
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
						flist.Add(fakeRec);
					}
				});
			});
			if (Fit) {
				fakeRects.SafeOperation((list) => {
					var inc = 1f / list.Count;
					var currentpos = 0f;
					foreach (var item in list) {
						item.Canvas = Canvas;
						item.AnchorMax = new Vector2f(1f, currentpos + inc);
						item.AnchorMin = new Vector2f(0f, currentpos);
						currentpos += inc;
						item.UpdateMinMaxNoPross();
					}
					_maxScroll = Vector2f.Zero;
					_minScroll = Vector2f.Zero;
				});
			}
			else {
				fakeRects.SafeOperation((list) => {
					var ypos = 0f;
					Vector2f? min = null;
					var max = Vector2f.Zero;
					foreach (var item in list) {
						item.Canvas = Canvas;
						item.ParentRect = this;
						var targetSize = item.Child.AnchorMaxValue - item.Child.AnchorMinValue;
						item.AnchorMin = new Vector2f(0, ypos);
						item.AnchorMax = new Vector2f(1, ypos + 1f);
						ypos += targetSize.y;
						item.UpdateMinMaxNoPross();
						min ??= item.Child.Min;
						max = item.Child.Max;
					}
					var scollmax = max - (min ?? Vector2f.Zero);
					var scale = Max - Min;
					if (scale.y >= scollmax.y) {
						_maxScroll = Vector2f.Zero;
						_minScroll = Vector2f.Zero;
					}
					else {
						_maxScroll = new Vector2f(0, 0);
						_minScroll = new Vector2f(-(scollmax.x - scale.x), -scollmax.y + scale.y);
					}
				});
			}
			base.UpdateUIMeshes();
		}
	}
}
