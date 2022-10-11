using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/Primitives" })]
	public class PrimitiveSyncObserver : ObserverBase<ISync>
	{
		[OnChanged(nameof(LinkerLoaded))]
		public readonly Linker<string> linker;

		private void LinkerLoaded() {
			if (linker.Linked) {
				linker.LinkedValue = TargetElement?.GetValue()?.ToString() ?? " NULL ";
			}
		}

		protected override void LoadObservedUI(UIBuilder2D ui) {
			ui.MinSize = new Vector2i(0, (int)(ELMENTHIGHTSIZE * 1.5f));
			var UIElement = ui.PushElement<UIElement>();
			UIElement.HorizontalFilling.Value = RFilling.Fill;
			UIElement.VerticalFilling.Value = RFilling.Fill;

			var trains = ui.PushElement<TextLabel>();
			trains.TextSize.Value = ELMENTHIGHTSIZE;
			trains.Text.Value = TargetElement.Name;
			trains.HorizontalAlignment.Value = RHorizontalAlignment.Right;
			trains.VerticalAlignment.Value = RVerticalAlignment.Center;
			ui.Pop();
			var lineEditor = ui.PushElement<LineEdit>();
			linker.Target = lineEditor.Text;
			lineEditor.MinSize.Value = new Vector2i(0, (int)(ELMENTHIGHTSIZE * 1.5f) - 5);
			trains.Max.Value = new Vector2f(0.49f, 1f);
			lineEditor.Min.Value = new Vector2f(0.51f, 0f);

			ui.Pop();
			ui.Pop();
		}
	}
}
