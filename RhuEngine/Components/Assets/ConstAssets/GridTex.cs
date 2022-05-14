using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	public class GridTex : AssetProvider<RTexture2D>
	{
		private void LoadTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Load(Engine.staticResources.Grid);
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
