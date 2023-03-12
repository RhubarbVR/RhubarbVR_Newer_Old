
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/ConstAssets" })]
	[AllowedOnWorldRoot]
	public sealed partial class SingleIconTex : AssetProvider<RTexture2D>
	{
		public override bool AutoDisposes => false;

		[OnChanged(nameof(LoadTexture))]
		public readonly Sync<RhubarbAtlasSheet.RhubarbIcons> Icon;

		public readonly Linker<Vector2f> MaxUV;

		public readonly Linker<Vector2f> MinUV;

		private void LoadTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if(MaxUV.Linked && MinUV.Linked) {
				var uvs = Engine.staticResources.IconSheet.GetUVs(Icon.Value);
				MaxUV.LinkedValue = uvs.max;
				MinUV.LinkedValue = uvs.min;
			}
			Load(Engine.staticResources.IconSheet.GetElement(Icon.Value));
		}
		protected override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
