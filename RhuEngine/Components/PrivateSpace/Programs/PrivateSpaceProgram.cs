using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public abstract class PrivateSpaceProgram : Program
	{
		public abstract RhubarbAtlasSheet.RhubarbIcons IconFind { get; }

		public override RTexture2D ProgramIcon => Engine.staticResources.IconSheet.GetElement(IconFind);

		public abstract string ProgramNameLocName { get; }

		public override string ProgramName => Engine.localisationManager.GetLocalString(ProgramNameLocName);
	}
}
