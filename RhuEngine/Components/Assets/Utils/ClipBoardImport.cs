using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using TextCopy;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets\\Utils" })]
	public class ClipBoardImport :Component, IUpdatingComponent
	{
		public override void Step() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if(RInput.Key(Key.V).IsJustActive() && RInput.Key(Key.Ctrl).IsActive()) {
				RLog.Info("ClipBoard");
				World.ImportString(ClipboardService.GetText());
			}
		}
	}
}
