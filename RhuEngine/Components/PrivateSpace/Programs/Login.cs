using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
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

		public override void LoadUI(Entity uiRoot) {
			window.CloseButton.Value = false;
			var ma = uiRoot.AttachComponent<UIRect>();
			var test = uiRoot.AddChild("Test");
			var buttonint = test.AttachComponent<UIButtonInteraction>();
			var buttonpramremove = test.AttachComponent<ButtonEventManager>();
			var texteditor = test.AttachComponent<UITextEditorInteraction>();
			var textcurrsor = test.AttachComponent<UITextCurrsor>();
			buttonpramremove.Click.Target = texteditor.EditingClick;
			buttonint.ButtonEvent.Target = buttonpramremove.Call;
			var trains = test.AttachComponent<UIRect>();
			trains.AnchorMax.Value = new Vector2f(0.75f);
			trains.AnchorMin.Value = new Vector2f(0.25f);
			var textEntity = test.AddChild("Trains");
			textEntity.AttachComponent<UIRect>();
			var text = textEntity.AttachComponent<UIText>();
			texteditor.Value.SetLinkerTarget(text.Text);
			var rectTwo = test.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
			rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
			var img = test.AttachComponent<UIRectangle>();
			img.Tint.Value = new Colorf(0.1f, 0.1f, 0.1f, 0.5f);
			var mittrains = test.AttachComponent<DynamicMaterial>();
			mittrains.shader.Target = test.AttachComponent<UnlitShader>();
			img.Material.Target = mittrains;
			textcurrsor.Material.Target = mittrains;
			textcurrsor.TextComp.Target = text;
			textcurrsor.TextCurrsor.Target = texteditor;
		}
	}
}
