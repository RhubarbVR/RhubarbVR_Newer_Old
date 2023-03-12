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
	public sealed partial class TextureScaler : Component
	{
		[OnAssetLoaded(nameof(TextScale))]
		public readonly AssetRef<RTexture2D> texture;

		[OnChanged(nameof(TextScale))]
		[Default(1f)]
		public readonly Sync<float> scaleMultiplier;
		[OnChanged(nameof(TextScale))]
		[Default(0.001f)]
		public readonly Sync<float> depth;

		[OnChanged(nameof(TextScale))]
		public readonly Linker<Vector2f> scale;
		[OnChanged(nameof(TextScale))]
		public readonly Linker<Vector3f> boxScale;

		private void TextScale(RTexture2D asset) {
			TextScale();
		}

		private void TextScale() {
			if (texture.Asset is null) {
				return;
			}
			var mainSize = new Vector2f(texture.Asset.Width, texture.Asset.Height) / texture.Asset.Height * scaleMultiplier;
			if (scale.Linked) {
				scale.LinkedValue = mainSize;
			}
			if (boxScale.Linked) {
				boxScale.LinkedValue = new Vector3f(mainSize.x, depth.Value, mainSize.y);
			}
		}

	}
}
