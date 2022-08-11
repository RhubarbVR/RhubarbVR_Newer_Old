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
	[RemoveFromProgramList]
	public class LoginProgram : Program
	{
		public override string ProgramID => "LoginScreen";

		public override Vector2i? Icon => new Vector2i(10,0);

		public override RTexture2D Texture => null;

		public override string ProgramName => "Programs.Login.Name";

		public override bool LocalName => true;

		[Exposed]
		public void Click() {
		}

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIText ErrorText;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity RegEntiyOne;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity RegEntiyTwo;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity RegEntiyThree;

		public void Error(string error) {
			ErrorText.Text.Value = $"<colorred>{error}";
		}
		public void Normal(string n) {
			ErrorText.Text.Value = $"{n}";
		}
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIText Password;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIText ConfPassword;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIText UserName;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIText Email;


		[Exposed]
		public void MainButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			if (RegScreen) {
				if(Password.Text.Value != ConfPassword.Text.Value) {
					Error("Passwords Don't Match");
					return;
				}

				Task.Run(async () => {
					var info = await Engine.netApiManager.SignUp(UserName.Text, Email.Text, Password.Text, new DateTime(1980, 9, 19));
					if (info.Error || info.ErrorDetails == "Normal Error") {
						Error(info.Message);
					}
					else {
						Normal(info.Message);
						SwitchReg();
					}
				});
			}
			else {
				Task.Run(async () => {
					var info = await Engine.netApiManager.Login(Email.Text, Password.Text);
					if (!info.Login) {
						Error(info.Message);
					}
					else {
						Close();
					}
				});
			}
		}

		public bool RegScreen = false;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public StandardLocale ButtonOneText;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public StandardLocale ButtonTwoText;

		
		[Exposed]
		public void ToggleButton(ButtonEvent buttonEvent) {
			if (!buttonEvent.IsClicked) {
				return;
			}
			SwitchReg();
		}

		public void SwitchReg() {
			RegScreen = !RegScreen;
			RegEntiyOne.enabled.Value = RegScreen;
			RegEntiyTwo.enabled.Value = RegScreen;
			ButtonOneText.Key.Value = RegScreen ? "Programs.Login.Register" : "Programs.Login.Login";
			ButtonTwoText.Key.Value = RegScreen ? "Programs.Login.Login" : "Programs.Login.Register";
		}
		
		public override void LoadUI(Entity uiRoot) {
			window.CloseButton.Value = false;
			var ma = uiRoot.AttachComponent<UIRect>();
			var mit = window.MainMit.Target;
			var uiBuilder = new UIBuilder(uiRoot, mit, ma,true);
			uiBuilder.PushRect(new Vector2f(0.5f,0.6f), new Vector2f(0.5f, 0.6f),0);
			var windowSize = new Vector2f(2.5f, 3.25f);
			uiBuilder.SetOffsetMinMax(-windowSize, windowSize);
			uiBuilder.PushRect(new Vector2f(0, 0.85f), new Vector2f(1, 1),0);
			uiBuilder.PushRect(new Vector2f(0.1f), new Vector2f(0.9f),0);
			uiBuilder.AddSprit(new Vector2i(11, 0), new Vector2i(11, 0), window.IconMit.Target, window.IconSprite.Target);
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PushRect(new Vector2f(0, 0), new Vector2f(1, 0.85f),0);
			uiBuilder.PushRect(new Vector2f(0.01f), new Vector2f(0.99f),0);
			var list = uiBuilder.AttachChildRect<VerticalList>();
			list.Fit.Value = true;
			list.Depth.Value = 0f;

			uiBuilder.PushRect(null, null, 0);
			uiBuilder.PushRect(new Vector2f(0, 0), new Vector2f(0.5, 1), 0);
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0);
			uiBuilder.AddButton(false, 0.2f).ButtonEvent.Target = MainButton;
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0.05f);
			ButtonOneText = uiBuilder.AddTextWithLocal("Programs.Login.LoginButton", 1.9f);
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();

			uiBuilder.PushRect(new Vector2f(0.5, 0), new Vector2f(1, 1), 0);
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0);
			uiBuilder.AddButton(false, 0.2f).ButtonEvent.Target = ToggleButton;
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0.05f);
			ButtonTwoText = uiBuilder.AddTextWithLocal("Programs.Login.Register",1.9f);
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();
			uiBuilder.PopRect();

			var minclamp = new Vector2f(1,float.MinValue);
			uiBuilder.PushRect();
			uiBuilder.PopRect();


			uiBuilder.PushRect();
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0f);
			var Confirmpassword = uiBuilder.AddTextEditor("Programs.Login.ConfirmPassword", 0.2f, 1, "", 0.1f, null, 1.9f);
			Confirmpassword.Item1.Password.Value = true;
			ConfPassword = Confirmpassword.Item1;
			uiBuilder.PopRect();
			RegEntiyOne = uiBuilder.CurretRectEntity;
			RegEntiyOne.enabled.Value = false;
			ConfPassword.HorizontalAlien.Value = EHorizontalAlien.Left;
			ConfPassword.MinClamp.Value = minclamp;
			uiBuilder.PopRect();

			uiBuilder.PushRect();
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0f);
			var password = uiBuilder.AddTextEditor("Programs.Login.Password", 0.2f, 1, "", 0.1f, null, 1.9f);
			password.Item1.Password.Value = true;
			Password = password.Item1;
			password.Item1.HorizontalAlien.Value = EHorizontalAlien.Left;
			password.Item1.MinClamp.Value = minclamp;
			uiBuilder.PopRect();
			uiBuilder.PopRect();


			uiBuilder.PushRect();
			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0f);
			Email = uiBuilder.AddTextEditor("Programs.Login.Email", 0.2f,1,"",0.1f,null,1.9f).Item1;
			Email.HorizontalAlien.Value = EHorizontalAlien.Left;
			Email.MinClamp.Value = minclamp;
			uiBuilder.PopRect();
			uiBuilder.PopRect();

			uiBuilder.PushRect();

			uiBuilder.PushRect(new Vector2f(0.1f, 0.1f), new Vector2f(0.9f, 0.9f), 0f);
			UserName = uiBuilder.AddTextEditor("Programs.Login.Username", 0.2f, 1, "", 0.1f, null, 1.9f).Item1;
			RegEntiyTwo = uiBuilder.CurretRectEntity;
			RegEntiyTwo.enabled.Value = false;
			UserName.HorizontalAlien.Value = EHorizontalAlien.Left;
			UserName.MinClamp.Value = minclamp;
			uiBuilder.PopRect();

			uiBuilder.PopRect();

			//uiBuilder.PushRect();
			////DateOfBirth
			//uiBuilder.PopRect();

			uiBuilder.PushRect();
			//Error
			ErrorText = uiBuilder.AddText("<colorred>",null,0,1,null,true);
			uiBuilder.PopRect();
		}
	}
}
