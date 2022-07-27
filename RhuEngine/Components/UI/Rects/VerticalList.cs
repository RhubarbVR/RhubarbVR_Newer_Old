using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{

	[Category(new string[] { "UI/Rects" })]
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
			fakeRects.SafeOperation((list) => {
				foreach (var item in list) {
					try {
						item.Child.AddedSizeCHange -= UpdateUIMeshes;
					}
					catch { }
				}
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
						item.AddedSizeCHange += UpdateUIMeshes;
						item.SetOverride(fakeRec);
						fakeRects.SafeAdd(fakeRec);
					}
				});
			});
			if (Fit) {
				fakeRects.SafeOperation((list) => {
					var inc = 1f / list.Count;
					var currentpos = 0f;
					foreach (var item in list) {
						item.Canvas = Canvas;
						item.AnchorMin = new Vector2f(0f, currentpos);
						currentpos += inc;
						item.AnchorMax = new Vector2f(1f, currentpos);
						item.UpdateMinMaxNoPross();
					}
					_maxScroll = Vector2f.Zero;
					_minScroll = Vector2f.Zero;
				});
			}
			else {
				var scale = Max - Min;
				fakeRects.SafeOperation((list) => {
					var ypos = 1f;
					var min = Vector2f.Zero;
					Vector2f? max = null;
					foreach (var item in list) {
						item.Canvas = Canvas;
						item.ParentRect = this;
						item.AnchorMin = new Vector2f(0);
						item.AnchorMax = new Vector2f(1);
						var targetSize = item.Child.AnchorMax.Value - item.Child.AnchorMin.Value;
						targetSize += item.Child.AddedSize;
						ypos -= targetSize.y;
						item.AnchorMin = new Vector2f(0, ypos);
						item.AnchorMax = new Vector2f(1, ypos + 1f);
						item.UpdateMinMaxNoPross();
						min = item.Child.Min;
						max ??= item.Child.Max;
					}
					var scollmax = (max ?? Vector2f.Zero) - min;
					if (scale.y <= ypos) {
						_maxScroll = Vector2f.Zero;
						_minScroll = Vector2f.Zero;
					}
					else {
						_minScroll = Vector2f.Zero;
						_maxScroll = new Vector2f(0, -ypos * scale.y);
					}
				});
			}
			base.UpdateUIMeshes();
		}
	}
}
