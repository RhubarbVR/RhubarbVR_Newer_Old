using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues
{
	[PrivateSpaceOnly]
	public sealed class Open_WithDialogue : OverlayDialogue
	{
		public void LoadImport(string file, Stream stream, string mimeType, string ex, object[] args = null) {
			var foundProgam = false;
			foreach (var data in ProgramOpenWithAttribute.GetAllPrograms(mimeType, Assembly.GetExecutingAssembly())) {
				foundProgam = true;
				RLog.Info($"Found program to open mimeType:{mimeType} Program {data.Name}");
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
