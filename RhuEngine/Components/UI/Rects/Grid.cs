using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
	public class Grid : RawScrollUIRect
	{
		[Default(2)]
		[OnChanged(nameof(RegUpdateUIMeshes))]
		public readonly Sync<int> GridWidth;

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
			fakeRects.SafeOperation((list) => {
				list.Clear();
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
						fakeRects.SafeAdd(fakeRec);
					}
				});
			});
			fakeRects.SafeOperation((list) => {
				var inc = 1f / GridWidth;
				var yinc = Canvas.scale.Value.x/Canvas.scale.Value.y * inc;

				var xcurrentpos = 0f;
				var ycurrentpos = 1f - yinc;
				Vector2f? min = null;
				var max = Vector2f.Zero;
				foreach (var item in list) {
					item.Canvas = Canvas;
					item.AnchorMin = new Vector2f(xcurrentpos,ycurrentpos);
					xcurrentpos += inc;
					item.AnchorMax = new Vector2f(xcurrentpos, yinc + ycurrentpos);
					if (xcurrentpos == 1f) {
						xcurrentpos = 0f;
						ycurrentpos -= yinc;
					}
					item.UpdateMinMaxNoPross();
					min ??= item.Child.Min;
					max = item.Child.Max;
				}
				var scale = Max - Min;
				if (scale.y <= ycurrentpos) {
					_maxScroll = Vector2f.Zero;
					_minScroll = Vector2f.Zero;
				}
				else {
					_minScroll = Vector2f.Zero;
					_maxScroll = new Vector2f(0, -ycurrentpos * scale.y);
				}
			});
		}
	}
}
