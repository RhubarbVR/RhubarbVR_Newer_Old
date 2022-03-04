using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Sliders" })]
	public class DHSlider : UIComponent
	{
		public Sync<double> Value;
		public Sync<double> Min;
		public Sync<double> Max;
		public new Sync<double> Step;
		public Sync<float> Width;
		[Default(UIConfirm.Push)]
		public Sync<UIConfirm> ConfirmMethod;

		public SyncDelegate OnChanged;
		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			var e = Value.Value;
			if (UI.HSlider("HSlider",ref e,Min,Max,Step,Width,ConfirmMethod)) {
				AddWorldCoroutine(()=> OnChanged.Target?.Invoke());
			}
			if (e != Value.Value) {
				Value.Value = e;
			}
			UI.PopId();
		}
	}
}
