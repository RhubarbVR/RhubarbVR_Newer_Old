using System;
using System.Collections.Generic;
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
	[PrivateSpaceOnly]
	[ProgramOpenWith("text/uri-list", "text/x-uuencode", "text/plain", "nofile/uri")]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class OpenBrowserProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.MissingFile;

		public override string ProgramNameLocName => "Program.OpenLink.Name";

		public Uri targetURI;

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			if ((args?.Length ?? 0) >= 1) {
				if (args[0] is Uri target) {
					targetURI = target;
				}
				if (args[0] is string targetSting) {
					Uri.TryCreate(targetSting, UriKind.RelativeOrAbsolute, out targetURI);
				}
			}
			if (file is not null) {
				using var reader = new StreamReader(file, true);
				Uri.TryCreate(reader.ReadToEnd(), UriKind.RelativeOrAbsolute, out targetURI);
			}

			if (targetURI is null) {
				CloseProgram();
			}
			else {
				BuildUI(AddWindow());
			}
		}

		private void BuildUI(ViewPortProgramWindow programWindow) {


		}


	}
}
