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
		protected override void LoadObservedUI(UIBuilder2D ui) {
			if (TargetElement is null) {
				return;
			}
			var table = ui.PushElement<TextLabel>();
			ui.Min = new Vector2f(0, 1);
			ui.MinSize = new Vector2i(0, ELMENTHIGHTSIZE);
			table.TextSize.Value = ELMENTHIGHTSIZE;
			table.Text.Value = TargetElement.Name + " : " + TargetElement.GetType().GetFormattedName();
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
					element.AttachComponent<BoxContainer>().Vertical.Value = true;
					var newOBserver = element.AttachComponent<IObserver>(newObserver);
					newOBserver.SetObserverd(objec);
				}
			}
		}
	}
}