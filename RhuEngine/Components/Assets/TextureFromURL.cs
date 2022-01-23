using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public class TextureFromURL : AssetProvider<Tex>
	{
		[OnChanged(nameof(LoadTexture))]
		[Default("https://cataas.com/cat/says/Base%20Url%20For%20RhubarbVR")]
		public Sync<string> url;

		public async Task UpdateTexture() {
			Log.Info("Loading img URL:" + url.Value);
			using var client = new HttpClient();
			Log.Info("Client");
			using var response = await client.GetAsync(url.Value);
			using var streamToReadFrom = await response.Content.ReadAsStreamAsync();

			try {
				Log.Info("Downloaded");
				var _texture = new ImageSharpTexture(streamToReadFrom, true);
				Load(_texture.CreateTexture());
			}
			catch {
				Log.Info($"Failed to initialize image");
				Load(null);
			}
		}

		private void LoadTexture() {
			UpdateTexture().ConfigureAwait(false);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			LoadTexture();
		}
	}
}
