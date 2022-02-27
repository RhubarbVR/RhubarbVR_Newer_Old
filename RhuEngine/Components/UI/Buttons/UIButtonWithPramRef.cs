using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Buttons" })]
	public class UIButtonWithPramRef<T> : UIComponent where T : class, IWorldObject
	{
		[Default("Click Me")]
		public Sync<string> Text;

		public Sync<Vec2> Size;

		public SyncRef<T> Pram;

		public SyncDelegate<Action<T>> onClick;

		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			if(Size.Value.v == Vec2.Zero.v) {
				if (UI.Button(Text.Value ?? "")) {
					AddWorldCoroutine(() => onClick.Target?.Invoke(Pram.Target));
				}
			}
			else {
				if (UI.Button(Text.Value ?? "", Size)) {
					AddWorldCoroutine(() => onClick.Target?.Invoke(Pram.Target));
				}
			}
			UI.PopId();
		}
	}
}
