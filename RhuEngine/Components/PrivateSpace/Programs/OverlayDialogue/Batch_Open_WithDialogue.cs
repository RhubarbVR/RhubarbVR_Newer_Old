using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues
{
	[PrivateSpaceOnly]
	public sealed partial class Batch_Open_WithDialogue : OverlayDialogue
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public void LoadImport(string[] file, string mimeType, string ex) {
			var foundProgam = false;
			foreach (var data in ProgramOpenWithAttribute.GetAllPrograms(mimeType, Assembly.GetExecutingAssembly())) {
				foundProgam = true;
				var (ProgramName, icon) = data.GetProgramInfo();
				RLog.Info($"Found program to open mimeType:{mimeType} Program {ProgramName}");
			}
			if (!foundProgam) {
				RLog.Info($"Found no program to open mimeType:{mimeType}");
			}
		}


		public override void Close() {

		}

		public override void Opened() {

		}
	}
}
