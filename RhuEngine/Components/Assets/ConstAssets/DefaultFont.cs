using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	[AllowedOnWorldRoot]
	public sealed partial class MainFont : AssetProvider<RFont>
	{
		public override bool AutoDisposes => false;

		private void LoadFont() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Load(Engine.staticResources.MainFont);
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			LoadFont();
		}
	}
}
