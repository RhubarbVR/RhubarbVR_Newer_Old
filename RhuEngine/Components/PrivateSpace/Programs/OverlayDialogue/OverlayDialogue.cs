using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues
{
	[PrivateSpaceOnly]
	public abstract partial class OverlayDialogue: Component
	{
		[NoSave]
		[NoShow]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public ViewPortProgramWindow programWindow;

		public abstract void Opened();
		public abstract void Close();

		public void CloseWindow() {
			programWindow?.Close();
			programWindow = null;
			Entity.Destroy();
		}
	}
}
