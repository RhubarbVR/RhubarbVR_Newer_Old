using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Events")]
	public sealed partial class UIFocusEvents : ElementEvent<UIElement>
	{
		public readonly SyncDelegate FocusEntered;
		public readonly SyncDelegate FocusExited;

		protected override void LoadCanvasItem(UIElement data) {
			if(data is null) {
				return;
			}
			data.FocusEntered += FocusEntered.Invoke;
			data.FocusExited += FocusExited.Invoke;
		}

		protected override void UnLoadCanvasItem(UIElement data) {
			if (data is null) {
				return;
			}
			data.FocusEntered -= FocusEntered.Invoke;
			data.FocusExited -= FocusExited.Invoke;
		}
	}
}
