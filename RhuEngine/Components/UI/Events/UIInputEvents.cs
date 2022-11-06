using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Events")]
	public sealed class UIInputEvents : ElementEvent<UIElement>
	{
		public readonly SyncDelegate InputEntered;
		public readonly SyncDelegate InputExited;

		protected override void LoadCanvasItem(UIElement data) {
			if(data is null) {
				return;
			}
			data.InputEntered += InputEntered.Invoke;
			data.InputExited += InputExited.Invoke;
		}

		protected override void UnLoadCanvasItem(UIElement data) {
			if (data is null) {
				return;
			}
			data.InputEntered -= InputEntered.Invoke;
			data.InputExited -= InputExited.Invoke;
		}
	}
}
