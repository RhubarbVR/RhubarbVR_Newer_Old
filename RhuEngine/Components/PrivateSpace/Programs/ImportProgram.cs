using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using TextCopy;

namespace RhuEngine.Components
{
	[ProgramOpenWith("*")]
	[Category(new string[] { "Assets/Importers" })]
	public sealed class ImportProgram : Program
	{

		public RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.AddFile;

		public string ProgramNameLocName => "Common.ImportFile";

		public override RTexture2D ProgramIcon => Engine.MainEngine.staticResources.IconSheet.GetElement(IconFind);

		public override string ProgramName => Engine.MainEngine.localisationManager.GetLocalString(ProgramNameLocName);

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var window = AddWindowWithIcon(IconFind);
			window.SizePixels = new Vector2i(320, 350);
			window.CenterWindowIntoView();
			var path = ex;
			if (args is not null && args.Length == 1 && args[0] is string @string) {
				path = @string;
			}
			var gettype = ImportStatics.GetFileTypes(path, mimetype);




		}

	}
}
