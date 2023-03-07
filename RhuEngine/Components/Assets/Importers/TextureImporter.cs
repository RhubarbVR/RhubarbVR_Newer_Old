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
		public readonly SyncRef<IValueSource<bool>> srgbTextures;

		private bool SrgbTextures => srgbTextures.Target?.Value ?? false;

		public override void BuildUI(Entity rootBox) {
			var checkBOx = rootBox.AddChild("CheckBox").AttachComponent<CheckBox>();
			checkBOx.Text.Value = "SRGB";
			srgbTextures.Target = checkBOx.ButtonPressed;
			checkBOx.ButtonPressed.Value = true;
			base.BuildUI(rootBox);
		}

		public override Task ImportAsset() {
			return Task.Run(() => ImportAsync(_importData.url_path, _importData.isUrl, _importData.rawData));
		}

		public void ImportAsync(string data, bool wasUri, Stream rawdata) {
			RLog.Info($"Loaded Texture Data {data} Uri{wasUri}");
			if (wasUri) {
				var box = Entity.AttachComponent<BoxShape>();
				Entity.AttachComponent<Grabbable>();
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<RectangleMesh, UnlitMaterial>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.Dimensions);
				scaler.scaleMultiplier.Value = 0.1f;
				scaler.boxScale.SetLinkerTarget(box.Size);
				var textur = Entity.AttachComponent<StaticTexture>();
				scaler.texture.Target = textur;
				textur.url.Value = data;
				mit.MainTexture.Target = textur;
				mit.DullSided.Value = true;
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newtexture = new ImageSharpTexture(data, SrgbTextures).CreateTexture();
						var textureURI = Entity.World.CreateLocalAsset(newtexture);
						ImportAsync(textureURI.ToString(), true, null);
						newtexture.Dispose();
					}
					else {
						RLog.Err("Texture Load Uknown" + data);
					}
				}
				else {
					var newtexture = new ImageSharpTexture(rawdata, SrgbTextures).CreateTexture();
					var textureURI = Entity.World.CreateLocalAsset(newtexture);
					ImportAsync(textureURI.ToString(), true, null);
					newtexture.Dispose();
				}
			}
		}

	}
}
