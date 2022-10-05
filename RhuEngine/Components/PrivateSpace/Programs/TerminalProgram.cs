using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	public sealed class TerminalProgram : Program
	{
		public override string ProgramID => "Terminal";

		public override Vector2i? Icon => new Vector2i(4,6);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.Terminal.Name";

		public override bool LocalName => true;

		private bool _waitingForPassword = false;

		private string _passwordValue = string.Empty;

		[Exposed]
		public void DoneEdit() {
			if (_waitingForPassword) {
				_passwordValue = _editedValue.Value;
				_editorUiText.Password.Value = false;
				_editedValue.Value = "";
				_waitingForPassword = false;
				return;
			}
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
		private UI3DText _text;
		[NoLoad]
		[NoSave]
		[NoSync]
		[NoSyncUpdate]
		private UI3DText _editorUiText;

		public override void LoadUI(Entity uiRoot) {
			var ma = uiRoot.AttachComponent<UI3DRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UI3DBuilder(uiRoot, mit, ma,true);
			uiBuilder.PushRect(null,null,0);
			uiBuilder.PushRect(null, null, 0);
			uiBuilder.SetOffsetMinMax(new Vector2f(0.2f, 1), null);
			_text = uiBuilder.AddText("Failed To Load console data", null, 1.8f, 1, null, true);
			_text.MinClamp.Value = new Vector2f(5, float.MinValue);
			Engine.outputCapture.TextEdied += OutputCapture_TextEdied;
			Engine.commandManager.PasswordEvent = PasswordInput;
			OutputCapture_TextEdied();
			uiBuilder.PopRect();
			uiBuilder.PushRect(null,new Vector2f(1,0), 0);
			var editor = uiBuilder.AddTextEditor("", 0.2f, 0.9f, "", 0.1f, null, 1.9f);
			editor.Item2.OnDoneEditing.Target = DoneEdit;
			_editorUiText = editor.Item1;
			_editedValue = editor.Item3;
			uiBuilder.SetOffsetMinMax(null, new Vector2f(0, 1));
		}

		public string PasswordInput() {
			_editorUiText.Password.Value = true;
			_waitingForPassword = true;
			_editedValue.Value = "";
			while (_waitingForPassword) {
				Thread.Sleep(30);
			}
			return _passwordValue;
		}

		private void OutputCapture_TextEdied() {
			if (Engine.IsCloseing) {
				return;
			}
			RUpdateManager.ExecuteOnEndOfFrame(this, () => _text.Text.Value = Engine.outputCapture.InGameConsole);
		}

		public override void Dispose() {
			Engine.outputCapture.TextEdied -= OutputCapture_TextEdied;
			base.Dispose();
		}
	}
}
