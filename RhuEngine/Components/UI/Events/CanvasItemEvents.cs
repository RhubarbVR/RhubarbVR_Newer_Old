using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Events")]
	public sealed partial class CanvasItemEvents : ElementEvent<CanvasItem>
	{
		public readonly SyncDelegate VisibilityChanged;
		public readonly SyncDelegate Hidden;
		public readonly SyncDelegate ItemRectChanged;

		protected override void LoadCanvasItem(CanvasItem data) {
			if (data is null) {
				return;
			}
			data.VisibilityChanged += VisibilityChanged.Invoke;
			data.Hidden += Hidden.Invoke;
			data.ItemRectChanged += ItemRectChanged.Invoke;
		}

		protected override void UnLoadCanvasItem(CanvasItem data) {
			if (data is null) {
				return;
			}
			data.VisibilityChanged -= VisibilityChanged.Invoke;
			data.Hidden -= Hidden.Invoke;
			data.ItemRectChanged -= ItemRectChanged.Invoke;
		}
	}
}
