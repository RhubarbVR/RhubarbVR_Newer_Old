using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Events")]
	public sealed class UISizeEvents : ElementEvent<UIElement>
	{
		public readonly SyncDelegate Resized;
		public readonly SyncDelegate SizeFlagsChanged;
		public readonly SyncDelegate MinimumSizeChanged;

		protected override void LoadCanvasItem(UIElement data) {
			if (data is null) {
				return;
			}
			data.MinimumSizeChanged += MinimumSizeChanged.Invoke;
			data.SizeFlagsChanged += SizeFlagsChanged.Invoke;
			data.Resized += Resized.Invoke;
		}

		protected override void UnLoadCanvasItem(UIElement data) {
			if (data is null) {
				return;
			}
			data.MinimumSizeChanged -= MinimumSizeChanged.Invoke;
			data.SizeFlagsChanged -= SizeFlagsChanged.Invoke;
			data.Resized -= Resized.Invoke;
		}
	}
}
