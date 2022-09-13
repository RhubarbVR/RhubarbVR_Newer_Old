using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers" })]
	public sealed class ObserverComponent : ObserverBase<Component>
	{
		protected override UIRect BuildMainUIRect() {
			return Entity.AttachComponent<VerticalList>();
		}
		protected override void LoadObservedUI(UIBuilder ui) {
			if(TargetElement is null) {
				return;
			}
			ui.PushRect(new Vector2f(0, 1),null, 0.01f);
			ui.SetOffsetMinMax(new Vector2f(0, -ELMENTHIGHTSIZE));
			ui.AddRectangle(0.2f,0.8f);
			ui.AddText(TargetElement.GetType().GetFormattedName(), null, 2, 1, null, true);
			ui.PopRect();
			var data = TargetElement.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var item in data) {
				if (item.GetCustomAttribute<NoShowAttribute>() is not null) {
					continue;
				}
				if (item.GetCustomAttribute<UnExsposedAttribute>() is not null) {
					continue;
				}
				var newObserver = item.FieldType.GetObserverFromType();
				if(newObserver is null) {
					continue;
				}
				if (item.GetValue(TargetElement) is IWorldObject objec) {
					var newOBserver = ui.CurretRectEntity.AddChild(objec.Name).AttachComponent<IObserver>(newObserver);
					newOBserver.SetUIRectAndMat(ui.MainMat);
					newOBserver.SetObserverd(objec);
				}
			}
		}
	}
}
