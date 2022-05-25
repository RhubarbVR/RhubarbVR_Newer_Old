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
			uiBuilder.PushRect(new Vector2f(0.5f,0.6f), new Vector2f(0.5f, 0.6f),0);
			var windowSize = new Vector2f(2.5f, 3.25f);
			uiBuilder.SetOffsetMinMax(-windowSize, windowSize);
			uiBuilder.PushRect(new Vector2f(0, 0.75f), new Vector2f(1, 1),0);
			uiBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f));
			uiBuilder.AddSprit(new Vector2i(11, 0), new Vector2i(11, 0), window.IconMit.Target, window.IconSprite.Target);
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PushRect(new Vector2f(0, 0), new Vector2f(1, 0.75f));
			uiBuilder.PushRect(new Vector2f(0.01f), new Vector2f(0.99f));
			var list = uiBuilder.AttachChildRect<VerticalList>();
			list.Fit.Value = true;
			list.Depth.Value = 0f;

			uiBuilder.PushRect(null, null, 0);
			uiBuilder.PushRect(new Vector2f(0, 0), new Vector2f(0.5, 1), 0);
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0);
			uiBuilder.AddButton(false, new Colorf(0.1f, 0.9f));
			uiBuilder.AddText("Login");
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();

			uiBuilder.PushRect(new Vector2f(0.5, 0), new Vector2f(1, 1), 0);
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0);
			uiBuilder.AddButton(false,new Colorf(0.1f,0.9f));
			uiBuilder.AddText("Register");
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();

			uiBuilder.PushRect();
			uiBuilder.AddText("Password");
			uiBuilder.PopRect();

			uiBuilder.PushRect();
			uiBuilder.AddText("UserName");
			uiBuilder.PopRect();
		}
	}
}
