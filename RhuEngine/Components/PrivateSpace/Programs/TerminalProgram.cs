using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	public class TerminalProgram : Program
	{
		public override string ProgramID => "Terminal";

		public override Vector2i? Icon => new Vector2i(4,6);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.Terminal.Name";

		public override bool LocalName => true;

		[Exposed]
		public void DoneEdit() {
			Engine.commandManager.RunComand(_editedValue.Value);
			_editedValue.Value = "";
		}

		[NoLoad]
		[NoSave]
		[NoSync]
		[NoSyncUpdate]
		private Sync<string> _editedValue;

		[NoLoad]
		[NoSave]
		[NoSync]
		[NoSyncUpdate]
		private UIText _text;


		public override void LoadUI(Entity uiRoot) {
			var ma = uiRoot.AttachComponent<UIRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UIBuilder(uiRoot, mit, ma,true);
			uiBuilder.PushRect(null,null);
			uiBuilder.PushRect(null, null, 0);
			uiBuilder.SetOffsetMinMax(new Vector2f(0.2f, 1), null);
			_text = uiBuilder.AddText("Failed To Load console data", null, 1.8f, 1, null, true);
			_text.HorizontalAlien.Value = EHorizontalAlien.Left;
			_text.VerticalAlien.Value = EVerticalAlien.Bottom;
			_text.MiddleLines.Value = false;
			_text.MinClamp.Value = new Vector2f(5, float.MinValue);
			Engine.outputCapture.TextEdied += OutputCapture_TextEdied;
			OutputCapture_TextEdied();
			uiBuilder.PopRect();
			uiBuilder.PushRect(null,new Vector2f(1,0), 0);
			var editor = uiBuilder.AddTextEditor("", 0.2f, 0.9f, "", 0.1f, null, 1.9f);
			editor.Item1.HorizontalAlien.Value = EHorizontalAlien.Left;
			editor.Item1.VerticalAlien.Value = EVerticalAlien.Center;
			editor.Item1.MiddleLines.Value = false;
			editor.Item2.OnDoneEditing.Target = DoneEdit;
			_editedValue = editor.Item4;
			uiBuilder.SetOffsetMinMax(null, new Vector2f(0, 1));
		}

		private void OutputCapture_TextEdied() {
			if (Engine.IsCloseing) {
				return;
			}
			_text.Text.Value = string.Join("\n", Engine.outputCapture.InGameConsole.ToString());
		}

		public override void Dispose() {
			Engine.outputCapture.TextEdied -= OutputCapture_TextEdied;
			base.Dispose();
		}
	}
}
