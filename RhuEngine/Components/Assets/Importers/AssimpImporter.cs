using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System.IO;
using StereoKit;
using Assimp;
using System.Threading.Tasks;
using System.Net.Http;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public class AssimpImporter : Importer
	{
		public static bool IsValidImport(string path) {
			path = path.ToLower();
			return
				path.EndsWith(".fbx") ||
				path.EndsWith(".dea") ||
				path.EndsWith(".gltf") || path.EndsWith(".glb") ||
				path.EndsWith(".blend") ||
				path.EndsWith(".3ds") ||
				path.EndsWith(".ase") ||
				path.EndsWith(".obj") ||
				path.EndsWith(".ifc") ||
				path.EndsWith(".xgl") || path.EndsWith(".zgl") ||
				path.EndsWith(".ply") ||
				path.EndsWith(".dxf") ||
				path.EndsWith(".lwo") ||
				path.EndsWith(".lws") ||
				path.EndsWith(".lxo") ||
				path.EndsWith(".stl") ||
				path.EndsWith(".x") ||
				path.EndsWith(".ac") ||
				path.EndsWith(".ms3d") ||
				path.EndsWith(".cob") || path.EndsWith(".scn") ||
				path.EndsWith(".bvh") ||
				path.EndsWith(".csm") ||
				path.EndsWith(".mdl") ||
				path.EndsWith(".md2") ||
				path.EndsWith(".md3") ||
				path.EndsWith(".pk3") ||
				path.EndsWith(".mdc") ||
				path.EndsWith(".md5") ||
				path.EndsWith(".smd") || path.EndsWith(".vta") ||
				path.EndsWith(".ogex") ||
				path.EndsWith(".b3d") ||
				path.EndsWith(".q3d") ||
				path.EndsWith(".q3s") ||
				path.EndsWith(".nff") ||
				path.EndsWith(".off") ||
				path.EndsWith(".raw") ||
				path.EndsWith(".ter") ||
				path.EndsWith(".hmp") ||
				path.EndsWith(".ndo");
		}

		AssimpContext _assimpContext;

		public async Task ImportAsync(string path_url, bool isUrl, byte[] rawData) {
			_assimpContext?.Dispose();
			_assimpContext = new AssimpContext();
			if (isUrl) {
				using var client = new HttpClient();
				using var response = await client.GetAsync(path_url);
				using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
				_assimpContext.ImportFileFromStream(streamToReadFrom);
			}
			else {
				if (rawData is null) {
					_assimpContext.ImportFileFromStream(new MemoryStream(rawData));
				}
				else {
					_assimpContext.ImportFile(path_url);
				}
			}
			Log.Err("Not Supported At the Moment");
		}
		public override void Import(string path_url, bool isUrl, byte[] rawData) {
			ImportAsync(path_url,isUrl,rawData).ConfigureAwait(false);
		}
	}
}
