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
	public sealed class LoginProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Login;

		public override string ProgramNameLocName => "Programs.Login.Name";

		public override void StartProgram(object[] args = null, Stream file = null, string mimetype = null, string ex = null) {
			AddWindow(null, null, false, false);
			RebuildUI();
		}

		public void RebuildUI() {
			if (programWindows.Count <= 0) {
				return;
			}
			if (programWindows[0].Target is null) {
				return;
			}
			var window = programWindows[0].Target;
			window.Entity.DestroyChildren();
			var button = window.Entity.AddChild("Button").AttachComponent<Button>();
			button.Text.Value = "Button";
			button.Min.Value = new Vector2f(0.25f);
			button.Max.Value = new Vector2f(0.75f);
			//if (Engine.netApiManager.Client.IsLogin) {
			//	window.Entity.AddChild("Color").AttachComponent<ColorRect>().Color.Value = Colorf.Purple;
			//}
			//else if (Engine.netApiManager.Client.IsOnline) {
			//	window.Entity.AddChild("Color").AttachComponent<ColorRect>().Color.Value = Colorf.Green;
			//}
			//else {
			//	window.Entity.AddChild("Color").AttachComponent<ColorRect>().Color.Value = Colorf.Red;
			//}
		}
	}
}
