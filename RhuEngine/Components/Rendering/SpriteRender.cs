using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Rendering" })]
	public class SpriteRender : RenderingComponent
	{
		public AssetRef<Sprite> sprite;

		public Sync<TextAlign> anchorPosition;

		public Sync<Color32> color;

		public override void OnAttach() {
			base.OnAttach();
			color.Value = Color32.White;
		}

		public override void Render() {
			if (sprite.Asset is not null) {
				sprite.Asset.Draw(Entity.GlobalTrans, anchorPosition, color);
			}
		}
	}
}
