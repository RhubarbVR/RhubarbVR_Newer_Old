using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Components
{
	[Category("UI/Events")]
	public sealed partial class UILinkOpen : ElementEvent<ButtonBase>
	{
		public readonly Sync<Uri> TargetUri;

		protected override void LoadCanvasItem(ButtonBase data) {
			if (data is null) {
				return;
			}
			data.PressedAction += Data_PressedAction;
		}

		private void Data_PressedAction() {
			Engine.worldManager?.PrivateSpaceManager?.ProgramManager?.OverlayProgram?.OpenURI(TargetUri.Value);
		}

		protected override void UnLoadCanvasItem(ButtonBase data) {
			if (data is null) {
				return;
			}
			data.PressedAction -= Data_PressedAction;
		}
	}
}
