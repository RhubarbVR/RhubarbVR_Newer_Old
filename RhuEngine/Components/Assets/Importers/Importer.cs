using System;
using System.IO;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	public abstract partial class Importer : Component
	{
		public readonly SyncRef<ImportProgram> ImporterProgram;

		public readonly SyncRef<Button> ImportButton;
		public virtual void BuildUI(Entity rootBox) {
			var importButton = ImportButton.Target = rootBox.AddChild("Button").AttachComponent<Button>();
			importButton.Text.Value = "Import";
			importButton.Alignment.Value = RButtonAlignment.Center;
			importButton.Pressed.Target = Import;
		}

		private bool _hasData = false;
		protected (string url_path, bool isUrl, Stream rawData, string ex) _importData;

		public virtual void LoadImportData(string url_path, bool isUrl, Stream rawData, string ex, ImportProgram importProgram) {
			ImporterProgram.Target = importProgram;
			_hasData = true;
			_importData = (url_path, isUrl, rawData, ex);
		}

		public abstract Task ImportAsset();


		[Exposed]
		public virtual void Import() {
			if (!_hasData) {
				return;
			}
			if (ImportButton.Target is null) {
				return;
			}
			if (ImportButton.Target.Disabled.Value) {
				return;
			}
			Task.Run(async () => {
				if (ImportButton.Target is not null) {
					ImportButton.Target.Disabled.Value = true;
				}
				try {
					await ImportAsset();
				}
				catch (Exception e) {
					RLog.Err("Failed to import asset Error:" + e.ToString());
				}
				_importData.rawData?.Dispose();
				if (ImportButton.Target is not null) {
					ImportButton.Target.Disabled.Value = false;
				}
				ImporterProgram.Target?.CloseProgram();
				Destroy();
			});
		}
	}
}
