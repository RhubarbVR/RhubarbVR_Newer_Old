using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{
	public abstract class ObserverSyncElement<T> : ObserverBase<T> where T : class, ISyncMember
	{
		protected override UIRect BuildMainUIRect() {
			var rect = Entity.AttachComponent<UIRect>();
			rect.AnchorMin.Value = new Vector2f(0, 0);
			rect.AnchorMax.Value = new Vector2f(1f, 0f);
			rect.OffsetMin.Value = new Vector2f(0, -ELMENTHIGHTSIZE);
			return rect;
		}

		protected abstract void LoadSideUI(UIBuilder ui);

		protected override void LoadObservedUI(UIBuilder ui) {
			if (TargetElement is null) {
				return;
			}
			ui.PushRectNoDepth(null, new Vector2f(0.25f, 1));
			ui.AddText(TargetElement.Name, null, 2, 1, null, true).HorizontalAlien.Value = EHorizontalAlien.Right;
			ui.PopRect();
			ui.PushRectNoDepth(new Vector2f(0.26f, 0.1f), new Vector2f(0.9f, 0.9f));
			LoadSideUI(ui);
			ui.PopRect();
		}
	}
}
