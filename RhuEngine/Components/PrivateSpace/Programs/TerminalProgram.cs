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
	[PrivateSpaceOnly]
	public sealed partial class TerminalProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Terminal;

		public override string ProgramNameLocName => "Programs.Terminal.Name";
		private RichTextLabel _richText;
		private LineEdit _lineEdit;
		private ScrollContainer _scroll;
		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
			var Window = AddWindow();
			var box = Window.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			box.Vertical.Value = true;
			_scroll = box.Entity.AddChild("Top").AttachComponent<ScrollContainer>();
			_scroll.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_scroll.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_richText = _scroll.Entity.AddChild("Top").AttachComponent<RichTextLabel>();
			_richText.TextSelectionEnabled.Value = true;
			_richText.ContextMenuEnabled.Value = true;
			_richText.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_richText.FitContent.Value = true;
			_richText.ScrollActive.Value = false;
			_richText.AutoWrapMode.Value = RAutowrapMode.WordSmart;
			Engine.outputCapture.TextEdied += OutputCapture_TextEdied;
			OutputCapture_TextEdied();
			_lineEdit = box.Entity.AddChild("Bottom").AttachComponent<LineEdit>();
			_lineEdit.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_lineEdit.TextSubmitted.Target = TextSubmitted;
		}

		[Exposed]
		public void TextSubmitted() {
			Engine.commandManager.RunComand(_lineEdit.Text.Value);
			_lineEdit.Text.Value = "";
		}

		public override void Dispose() {
			if (Engine is not null) {
				Engine.outputCapture.TextEdied -= OutputCapture_TextEdied;
			}
			base.Dispose();
		}

		private void OutputCapture_TextEdied() {
			RenderThread.ExecuteOnEndOfFrame(this, () => {
				try {
					_richText.Text.Value = Engine.outputCapture.InGameConsole;
				}
				catch { }
			});
		}
	}
}
