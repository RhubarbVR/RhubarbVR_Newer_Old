using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Sliders" })]
	public class UITextInput : UIComponent
	{
		public Sync<TextContext> Type;

		public Sync<Vec2> Size;

		public Linker<string> Value;
			
		public SyncDelegate OnChanged;

		[Default(true)]
		public Sync<bool> NullButton;

		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			string input = null;
			if (Value.Linked) {
				input = Value.LinkedValue;
			}
			if (NullButton) {
				if (UI.Button("X")) {
					AddWorldCoroutine(() => {
						if (Value.Linked) {
							Value.LinkedValue = null;
						}
						OnChanged.Target?.Invoke();
					});
				}
				UI.SameLine();
			}
			if (UI.Input("In",ref input, Size, Type)) {
				AddWorldCoroutine(() => {
					if (Value.Linked) {
						Value.LinkedValue = input;
					}
					OnChanged.Target?.Invoke();
				});
			}
			UI.PopId();
		}
	}
}
