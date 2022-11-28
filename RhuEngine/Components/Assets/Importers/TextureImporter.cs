using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed class TextureImporter : Importer
	{
		private bool _srgbTextures;

		public void ImportAsync(string data, bool wasUri, byte[] rawdata) {
			RLog.Info($"Loaded Texture Data {data} Uri{wasUri}");
			if (wasUri) {
				Entity.AttachComponent<BoxShape>();
				Entity.AttachComponent<Grabbable>();
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<RectangleMesh, UnlitMaterial>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.Dimensions);
				scaler.scaleMultiplier.Value = 0.1f;
				var textur = Entity.AttachComponent<StaticTexture>();
				scaler.texture.Target = textur;
				textur.url.Value = data;
				mit.MainTexture.Target = textur;
				Destroy();
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newtexture = new ImageSharpTexture(data, _srgbTextures).CreateTextureAndDisposes();
						var textureURI = Entity.World.CreateLocalAsset(newtexture);
						ImportAsync(textureURI.ToString(), true, null);
					}
					else {
						RLog.Err("Texture Load Uknown" + data);
					}
				}
				else {
					var newtexture = new ImageSharpTexture(new MemoryStream(rawdata), _srgbTextures).CreateTextureAndDisposes();
					var textureURI = Entity.World.CreateLocalAsset(newtexture);
					ImportAsync(textureURI.ToString(), true, null);
				}
			}
		}

		public override void Import(string data, bool wasUri, byte[] rawdata) {
			Task.Run(() => ImportAsync(data, wasUri, rawdata));
		}
	}
}
