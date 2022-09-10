using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;
using System.Xml.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers" })]
	public class ObserverEnity : ObserverBase<Entity>
	{
		protected override UIRect BuildMainUIRect() {
			return Entity.AttachComponent<UIRect>();
		}
		protected override void LoadObservedUI(UIBuilder ui) {
			if (TargetElement is null) {
				return;
			}
			ui.PushRectNoDepth();
			ui.PushRectNoDepth(null, new Vector2f(0.5f, 1f));

			ui.PopRect();

			ui.PushRectNoDepth(new Vector2f(0.5f, 0f), null);
			ui.PushRect(new Vector2f(0, 1), null, 0.01f);
			ui.SetOffsetMinMax(new Vector2f(0, -ELMENTHIGHTSIZE), new Vector2f(-2f, 0f));
			ui.PushRectNoDepth();
			ui.AddRectangle(0.2f, 0.8f);
			ui.AddText("Entity", null, 2, 1, null, true);
			ui.PopRect();
			ui.PopRect();
			ui.PushRect(new Vector2f(1f, 1f), null, 0);
			ui.SetOffsetMinMax(new Vector2f(-2f, -ELMENTHIGHTSIZE), null);
			ui.AttachChildRect<HorizontalList>(null, null, 0).Fit.Value = true;
			ui.AddButtonEventLabled("Delete");
			ui.AddButtonEventLabled("Add Child");
			ui.PopRect();
			ui.PopRect();
			ui.PushRect(null, null, 0);
			ui.SetOffsetMinMax(null, new Vector2f(0, -ELMENTHIGHTSIZE));
			ui.PushRectNoDepth(null, null);
			ui.AttachChildRect<VerticalList>(new Vector2f(0.1f, 0f));
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
					var newOBserver = ui.CurretRectEntity.AddChild(objec.Name).AttachComponent<IObserver>(newObserver);
					newOBserver.SetUIRectAndMat(ui.MainMat);
					newOBserver.SetObserverd(objec);
				}
			}
			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
			ui.PushRectNoDepth(null, new Vector2f(1f, 0.5f));
			ui.AttachChildRect<CuttingUIRect>();
			var scroll = ui.AttachComponentToStack<UIScrollInteraction>();
			scroll.OnScroll.Target = ui.AttachChildRect<BasicScrollRect>().Scroll;
			ui.AttachChildRect<VerticalList>(null, null, 0.1f);
			foreach (var item in TargetElement.components) {
				var observer = item.GetObserver();
				if (observer is not null) {
					ui.CurretRectEntity.AddChild(item.Name).AttachComponent<IObserver>(observer).SetUIRectAndMat(ui.MainMat).SetObserverd(item);
				}
			}

			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
			ui.PopRect();
		}
	}
}
