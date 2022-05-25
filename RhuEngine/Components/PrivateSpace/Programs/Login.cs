using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components.PrivateSpace
{
	public class Login : Program
	{
		public override string ProgramID => "LoginScreen";

		public override Vector2i? Icon => new Vector2i(10,0);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Login";

		[Exsposed]
		public void Click() {
		}

		public override void LoadUI(Entity uiRoot) {
			window.CloseButton.Value = false;
			var ma = uiRoot.AttachComponent<UIRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UIBuilder(uiRoot, mit, ma);
			uiBuilder.PushRect(new Vector2f(0.5f,0.6f), new Vector2f(0.5f, 0.6f));
			var windowSize = new Vector2f(2.5f, 3f);
			uiBuilder.SetOffsetMinMax(-windowSize, windowSize);
			uiBuilder.AddRectangle(Colorf.Blue);
		}
	}
}
