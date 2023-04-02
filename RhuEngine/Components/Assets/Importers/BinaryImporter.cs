using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed partial class BinaryImporter : Importer
	{
		public readonly SyncRef<IValueSource<bool>> wasmScript;

		private bool WasmScript => wasmScript.Target?.Value ?? false;

		public override void BuildUI(Entity rootBox) {
			base.BuildUI(rootBox);
			var checkBOx = rootBox.AddChild("CheckBox").AttachComponent<CheckBox>();
			checkBOx.Text.Value = "WasmScript";
			wasmScript.Target = checkBOx.ButtonPressed;
			checkBOx.ButtonPressed.Value = true;
		}

		public override Task ImportAsset() {
			return ImportAsync(_importData.url_path, _importData.isUrl, _importData.rawData);
		}

		public async Task ImportAsync(string data, bool wasUri, Stream rawdata) {
			RLog.Info($"Loaded Binary Data {data} Uri{wasUri}");
			if (wasUri) {
				var box = Entity.AttachComponent<BoxShape>();
				Entity.AttachComponent<Grabbable>();
				var asset = Entity.AttachComponent<StaticBinaryAsset>();
				asset.url.Value = data;
				if (WasmScript) {
					var wasmScript = Entity.AttachComponent<WasmRunner>();
					wasmScript.TargetScript.Target = asset;
					wasmScript.CallCompile();
				}
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						using var stream = File.OpenRead(data); 
						var binraryUri = await Entity.World.CreateLocalAsset(stream, "application/octet-stream");
						await ImportAsync(binraryUri.ToString(), true, null);
					}
					else {
						RLog.Err("Binary Load uknown" + data);
					}
				}
				else {
					var binraryUri = await Entity.World.CreateLocalAsset(rawdata, "application/octet-stream");
					await ImportAsync(binraryUri.ToString(), true, null);
				}
			}
		}

	}
}
