using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using TextCopy;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets\\Utils" })]
	public class TextureScaler : Component
	{
		[OnAssetLoaded(nameof(TextScale))]
		public AssetRef<Tex> texture;

		[OnChanged(nameof(TextScale))]
		public Sync<float> scaleMultiplier;

		public Linker<Vec2> scale;

		private void TextScale() {
			if(texture.Asset is null) {
				return;
			}
			if (scale.Linked) {
				scale.LinkedValue = new Vec2(texture.Asset.Width, texture.Asset.Height) / texture.Asset.Height * scaleMultiplier;
			}
		}

	}
}
