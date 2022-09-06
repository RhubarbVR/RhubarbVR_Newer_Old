using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers" })]
	public class ObserverWorldObject : ObserverBase<IWorldObject>
	{
		protected override UIRect BuildMainUIRect() {
			return Entity.AttachComponent<UIRect>();
		}
		protected override void LoadObservedUI(UIBuilder ui) {
			if(TargetElement is null) {
				return;
			}
			ui.PushRectNoDepth();
			ui.PushRect(new Vector2f(0, 1),null, 0.01f);
			ui.SetOffsetMinMax(new Vector2f(0, -ELMENTHIGHTSIZE));
			ui.AddRectangle(0.2f,0.8f);
			ui.AddText(TargetElement.GetType().GetFormattedName(), null, 2, 1, null, true);
			ui.PopRect();
			ui.AttachChildRect<CuttingUIRect>(null, null, 0);
			ui.SetOffsetMinMax(null, new Vector2f(0, -ELMENTHIGHTSIZE));

			var scroller = ui.AttachComponentToStack<UIScrollInteraction>();
			var itemScroller = ui.AttachChildRect<BasicScrollRect>(null, null, 0);
			scroller.OnScroll.Target = itemScroller.Scroll;
			ui.AttachChildRect<VerticalList>(null, null, 0);
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
			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
		}
	}
}
