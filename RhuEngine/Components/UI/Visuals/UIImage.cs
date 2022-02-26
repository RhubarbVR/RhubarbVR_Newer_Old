using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Visuals" })]
	public class UIImage : UIComponent
	{
		public AssetRef<Sprite> Sprite;
		public Sync<Vec2> Size;
		public override void RenderUI() {
			UI.Image(Sprite.Asset, Size);
		}
	}
}
