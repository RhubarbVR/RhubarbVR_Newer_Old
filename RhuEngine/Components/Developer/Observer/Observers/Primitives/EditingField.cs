using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;
using static RNumerics.Units;

namespace RhuEngine.Components
{
	public abstract class EditingField<T> : ObserverBase<T> where T : class, IWorldObject
	{

		protected override void LoadObservedUI(UIBuilder2D ui) {
			ui.MinSize = new Vector2i(0, (int)(ELMENTHIGHTSIZE * 1.5f));
			var UIElement = ui.PushElement<UIElement>();
			UIElement.InputFilter.Value = RInputFilter.Pass;
			UIElement.HorizontalFilling.Value = RFilling.Fill;
			UIElement.VerticalFilling.Value = RFilling.Fill;

			var trains = ui.PushElement<TextLabel>();
			trains.InputFilter.Value = RInputFilter.Pass;
			trains.TextSize.Value = ELMENTHIGHTSIZE;
			trains.Text.Value = TargetElement.Name;
			trains.HorizontalAlignment.Value = RHorizontalAlignment.Right;
			trains.VerticalAlignment.Value = RVerticalAlignment.Center;
			ui.Pop();
			var holder = ui.PushElement<UIElement>();
			holder.MinSize.Value = new Vector2i(0, (int)(ELMENTHIGHTSIZE * 1.5f) - 5);
			trains.Max.Value = new Vector2f(0.49f, 1f);
			holder.Min.Value = new Vector2f(0.51f, 0f);
			LoadEditor(ui);
			ui.Pop();
			ui.Pop();
		}


		protected abstract void LoadEditor(UIBuilder2D ui);
	}
}
