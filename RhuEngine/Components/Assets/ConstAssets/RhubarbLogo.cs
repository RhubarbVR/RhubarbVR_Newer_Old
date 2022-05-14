using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	public class RhubarbLogo : AssetProvider<RTexture2D>
	{
		[OnChanged(nameof(LoadTexture))]
		public readonly Sync<bool> Filled;

		private void LoadTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (Filled) {
				Load(Engine.staticResources.RhubarbLogoV1);
			}
			else {
				Load(Engine.staticResources.RhubarbLogoV2);
			}
		}
		public override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
