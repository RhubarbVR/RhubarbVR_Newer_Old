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

		public Linker<Vec2> scale;

		private void TextScale(Tex asset) {
			if(asset is null) {
				return;
			}
			if (scale.Linked) {
				scale.LinkedValue = new Vec2(asset.Width, asset.Height) / asset.Height;
			}
		}

	}
}
