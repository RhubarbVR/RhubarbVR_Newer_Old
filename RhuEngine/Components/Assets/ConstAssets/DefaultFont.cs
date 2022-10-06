using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	[AllowedOnWorldRoot]
	public sealed class MainFont : AssetProvider<RFont>
	{
		RFont _font;
		private void LoadFont() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_font = Engine.staticResources.MainFont;
			Load(_font);
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			LoadFont();
		}
	}
}
