using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Assimp;

using DataModel.Enums;

using NYoutubeDL.Options;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Components.UI;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer" })]
	public sealed class DevToolsProgram : Program
	{
		public RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Mouse;

		public string ProgramNameLocName => "Programs.DevTools.Name";

		public override RTexture2D ProgramIcon => Engine.MainEngine.staticResources.IconSheet.GetElement(IconFind);

		public override string ProgramName => Engine.MainEngine.localisationManager.GetLocalString(ProgramNameLocName);

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var tool = AddToolBar(IconFind);
			tool.Entity.AddChild().AttachComponent<ColorRect>().Color.Value = Colorf.RandomHue();
		}
	}
}
