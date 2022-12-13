using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	[AllowedOnWorldRoot]
	public sealed class IconsTex : AssetProvider<RTexture2D>
	{
		public override bool AutoDisposes => false;

		private void LoadTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Load(Engine.staticResources.Icons);
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
