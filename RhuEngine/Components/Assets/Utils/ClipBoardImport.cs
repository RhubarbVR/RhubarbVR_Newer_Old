using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using TextCopy;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets\\Utils" })]
	public class ClipBoardImport :Component, IUpdatingComponent
	{
		public override void Step() {
			if(Input.Key(Key.V).IsJustActive() && Input.Key(Key.Ctrl).IsActive()) {
				Log.Info("ClipBoard");
				World.ImportString(ClipboardService.GetText());
			}
		}
	}
}
