using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Sliders" })]
	public class FHSlider : UIComponent
	{
		public Sync<float> Value;
		public Sync<float> Min;
		public Sync<float> Max;
		public new Sync<float> Step;
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
