using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using TextCopy;
using RhuEngine.Linker;
using RhuEngine.Managers;

namespace RhuEngine.Components
{
	[UpdatingComponent]
	[Category(new string[] { "Assets/Utils" })]
	public sealed class ClipBoardImport :Component
	{
		protected override void OnLoaded() {
			base.OnLoaded();
			Engine.DragAndDrop += Engine_DragAndDrop;
		}

		private void Engine_DragAndDrop(System.Collections.Generic.List<string> obj) {
			if (World.Focus != World.FocusLevel.Focused) {
				return;
			}
			foreach (var item in obj) {
				World.ImportString(item);
			}
		}

		protected override void Step() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (Engine.HasKeyboard) {
				return;
			}
			if(InputManager.KeyboardSystem.IsKeyJustDown(Key.V) && InputManager.KeyboardSystem.IsKeyDown(Key.Ctrl)) {
				//ToDO inprove to have imgs and also have render bindings
				var data = ClipboardService.GetText();
				RLog.Info($"ClipBoard {data}");
				World.ImportString(data);
			}
		}
	}
}
