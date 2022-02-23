using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public class TextureImporter : Importer
	{
		public static bool IsValidImport(string path) {
			path = path.ToLower();
			return
				path.EndsWith(".png") ||
				path.EndsWith(".jpeg") ||
				path.EndsWith(".jpg") ||
				path.EndsWith(".bmp") ||
				path.EndsWith(".pdm") ||
				path.EndsWith(".gif") ||
				path.EndsWith(".tiff") ||
				path.EndsWith(".tga") ||
				path.EndsWith(".webp");
		}

		public override void Import(string data, bool wasUri, byte[] rawdata) {
			Log.Info($"Loaded Texture Data {data} Uri{wasUri}");
			if (wasUri) {
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<PlaneMesh, UnlitShader>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.dimensions);
				scaler.scaleMultiplier.Value = 0.1f;
				var textur = Entity.AttachComponent<StaticTexture>();
				scaler.texture.Target = textur;
				textur.url.Value = data;
				mit.faceCull.Value = Cull.None;
				mit.SetPram("diffuse", textur);
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = World.LoadLocalAsset(File.ReadAllBytes(data), data);
						Import(newuri.ToString(), true,null);
					}
					else {
						Log.Err("Texture Load Uknown" + data);
					}
				}
				else {
					var newuri = World.LoadLocalAsset(rawdata, data);
					Import( newuri.ToString(), true,null);
				}
			}
		}
	}
}
