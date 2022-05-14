using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	public class IconsTex : AssetProvider<RTexture2D>
	{
		private void LoadTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Load(Engine.staticResources.Icons);
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
