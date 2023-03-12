using System;
using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed partial class VideoImporter : Importer
	{
		public override Task ImportAsset() {
			return Task.Run(async () => await ImportAsync(_importData.url_path, _importData.isUrl, _importData.rawData));
		}

		public async Task ImportAsync(string data, bool wasUri, Stream rawdata) {
			if (wasUri) {
				RLog.Info("Building video");
				Entity.AttachComponent<Grabbable>();
				Entity.AttachComponent<BoxShape>();
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<RectangleMesh, UnlitMaterial>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.Dimensions);
				scaler.scaleMultiplier.Value = 0.5f;
				var textur = Entity.AttachComponent<VideoTexture>();
				//Entity.AttachComponent<SoundSource>().sound.Target = textur;
				textur.url.Value = data;
				scaler.texture.Target = textur;
				mit.MainTexture.Target = textur;
				textur.Playback.Play();
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = World.CreateLocalAsset(File.ReadAllBytes(data), MimeTypeManagment.GetMimeType(data));
						await ImportAsync(newuri.ToString(), true, null);
					}
					else {
						RLog.Err("Video Load Uknown" + data);
					}
				}
				else {
					var newuri = await World.CreateLocalAsset(rawdata, MimeTypeManagment.GetMimeType(data));
					await ImportAsync(newuri.ToString(), true, null);
				}
			}
		}
	}
}
