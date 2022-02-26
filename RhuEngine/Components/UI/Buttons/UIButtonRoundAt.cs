using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Buttons" })]
	public class UIButtonRoundAt : UIComponent
	{
		public AssetRef<Sprite> Image;

		public Sync<Vec3> TopLeft;

		public Sync<float> Diameter;

		public SyncDelegate onClick;

		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			if (UI.ButtonRoundAt("RoundButton",Image.Asset, TopLeft, Diameter)) {
				AddWorldCoroutine(() => onClick.Target?.Invoke());
			}
			UI.PopId();
		}
	}
}
