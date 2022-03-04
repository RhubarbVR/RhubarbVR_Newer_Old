using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Buttons" })]
	public class UIButtonRound : UIComponent
	{
		public AssetRef<Sprite> Image;

		public Sync<float> Diameter;

		public SyncDelegate onClick;

		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			if (UI.ButtonRound("RoundButton",Image.Asset,Diameter)) {
				AddWorldCoroutine(() => onClick.Target?.Invoke());
			}
			UI.PopId();
		}
	}
}
