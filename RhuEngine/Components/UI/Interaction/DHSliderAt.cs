using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction" })]
	public class DHSliderAt : UIComponent
	{
		public Sync<double> Value;
		public Sync<double> Min;
		public Sync<double> Max;
		public new Sync<double> Step;
		public Sync<Vec2> Size;
		public Sync<Vec3> TopLeft;
		[Default(UIConfirm.Push)]
		public Sync<UIConfirm> ConfirmMethod;

		public SyncDelegate OnChanged;
		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			var e = Value.Value;
			if (UI.HSliderAt("HSlider",ref e,Min,Max,Step, TopLeft,Size, ConfirmMethod)) {
				AddWorldCoroutine(()=> OnChanged.Target?.Invoke());
			}
			if (e != Value.Value) {
				Value.Value = e;
			}
			UI.PopId();
		}
	}
}
