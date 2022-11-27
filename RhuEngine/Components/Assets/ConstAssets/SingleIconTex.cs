using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	[AllowedOnWorldRoot]
	public sealed class SingleIconTex : AssetProvider<RTexture2D>
	{
		[OnChanged(nameof(LoadTexture))]
		public readonly Sync<RhubarbAtlasSheet.RhubarbIcons> Icon;
		private void LoadTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Load(Engine.staticResources.IconSheet.GetElement(Icon.Value));
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
