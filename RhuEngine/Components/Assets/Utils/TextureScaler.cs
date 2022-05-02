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
	[Category(new string[] { "Assets\\Utils" })]
	public class TextureScaler : Component
	{
		[OnAssetLoaded(nameof(TextScale))]
		public readonly AssetRef<RTexture2D> texture;

		[OnChanged(nameof(TextScale))]
		public readonly Sync<float> scaleMultiplier;

		public readonly Linker<Vector2f> scale;

		private void TextScale() {
			if(texture.Asset is null) {
				return;
			}
			if (scale.Linked) {
				scale.LinkedValue = new Vector2f(texture.Asset.Width, texture.Asset.Height) / texture.Asset.Height * scaleMultiplier;
			}
		}

	}
}
