using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Buttons" })]
	public class UIButton : UIComponent
	{
		[Default("Click Me")]
		public Sync<string> Text;

		public Sync<Vec2> Size;

		public SyncDelegate onClick;

		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			if(Size.Value.v == Vec2.Zero.v) {
				if (UI.Button(Text.Value ?? "")) {
					AddWorldCoroutine(() => onClick.Target?.Invoke());
				}
			}
			else {
				if (UI.Button(Text.Value ?? "", Size)) {
					AddWorldCoroutine(() => onClick.Target?.Invoke());
				}
			}
			UI.PopId();
		}
	}
}
