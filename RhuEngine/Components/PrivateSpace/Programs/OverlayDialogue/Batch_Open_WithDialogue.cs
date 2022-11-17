using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues
{
	[PrivateSpaceOnly]
	public sealed class Batch_Open_WithDialogue : OverlayDialogue
	{
		public void LoadImport(string[] file, string mimeType, string ex) {
			var foundProgam = false;
			foreach (var data in ProgramOpenWithAttribute.GetAllPrograms(mimeType, Assembly.GetExecutingAssembly())) {
				foundProgam = true;
				var programInfo = data.GetProgramInfo();
				RLog.Info($"Found program to open mimeType:{mimeType} Program {programInfo.ProgramName}");
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
