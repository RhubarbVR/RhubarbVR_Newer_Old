using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using TextCopy;
using RNumerics;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Utils" })]
	public class FontAtlasTexture : AssetProvider<RTexture2D>
	{
		[OnAssetLoaded(nameof(UpdateFont))]
		public readonly AssetRef<RFont> font;

		[OnChanged(nameof(UpdateTexture))]
		public readonly Sync<int> AtlasIndex;

		protected override void OnAttach() {
			base.OnAttach();
			font.Target = World.RootEntity.GetFirstComponentOrAttach<MainFont>();
		}

		RFont _lastFont;
		private void UpdateFont() {
			if (_lastFont is not null) {
				_lastFont.UpdateAtlas -= UpdateTexture;
			}
			_lastFont = font.Asset;
			if (font.Asset is null) {
				return;
			}
			_lastFont.UpdateAtlas += UpdateTexture;
			UpdateTexture();
		}

		private void UpdateTexture() {
			if(font.Asset is null) {
				Load(null);
				return;
			}
			if (font.Asset.fontAtlisParts.Count <= AtlasIndex) {
				Load(null);
				return;
			}
			if(font.Asset.fontAtlisParts.Count < 0) {
				Load(null);
				return;
			}
			Load(font.Asset.fontAtlisParts[AtlasIndex]._texture);
		}

	}
}
