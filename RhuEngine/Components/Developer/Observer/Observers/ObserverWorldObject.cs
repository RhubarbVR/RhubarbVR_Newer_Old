using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers" })]
	public sealed class ObserverWorldObject : ObserverBase<IWorldObject>
	{
		public readonly Linker<string> LableText;
		protected override void LoadObservedUI(UIBuilder2D ui) {
			if (TargetElement is null) {
				return;
			}
			var table = ui.PushElement<TextLabel>();
			LableText.Target = table.Text;
			ui.Min = new Vector2f(0, 1);
			ui.MinSize = new Vector2i(0, ELMENTHIGHTSIZE);
			table.TextSize.Value = ELMENTHIGHTSIZE;
			ui.Pop();
			var data = TargetElement.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var item in data) {
				if (item.GetCustomAttribute<NoShowAttribute>() is not null) {
					continue;
				}
				if (item.GetCustomAttribute<UnExsposedAttribute>() is not null) {
					continue;
				}
				var newObserver = item.FieldType.GetObserverFromType();
				if (newObserver is null) {
					continue;
				}
				if (item.GetValue(TargetElement) is IWorldObject objec) {
					var element = ui.Entity.AddChild(objec.Name);
					var boxXo = element.AttachComponent<BoxContainer>();
					boxXo.Vertical.Value = true;
					boxXo.InputFilter.Value = RInputFilter.Pass;

					var newOBserver = element.AttachComponent<IObserver>(newObserver);
					newOBserver.SetObserverd(objec);
				}
			}
		}

		protected override void LoadValueIn() {
			if (LableText.Linked) {
				LableText.LinkedValue = TargetElement.Name + " : " + TargetElement.GetType().GetFormattedName();

			}
		}
	}
}