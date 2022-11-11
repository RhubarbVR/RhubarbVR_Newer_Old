using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class LoginProgram : PrivateSpaceProgram
	{
		public override RhubarbAtlasSheet.RhubarbIcons IconFind => RhubarbAtlasSheet.RhubarbIcons.Login;

		public override string ProgramNameLocName => "Programs.Login.Name";

		public override void StartProgram(object[] args = null, Stream file = null, string mimetype = null, string ex = null) {
			AddWindow(null, null, false, false);
			RebuildUI();
			Engine.netApiManager.Client.OnLogin += Client_OnLogin;
			Engine.netApiManager.Client.OnLogout += Client_OnLogout;
			Engine.netApiManager.Client.HasGoneOnline += Client_HasGoneOnline;
		}

		private void Client_HasGoneOnline() {
			RebuildUI();
		}

		private void Client_OnLogout() {
			RebuildUI();
		}

		private void Client_OnLogin(RhubarbCloudClient.Model.PrivateUser obj) {
			RebuildUI();
		}

		private TextLabel _loadingText;

		private float _lastUpdate;

		protected override void Step() {
			base.Step();
			if (_lastUpdate > 5) {
				if (_loadingText is not null) {
					_loadingText.Text.Value = RhubarbLoadingFacts.GetRandomFact(Engine.localisationManager).Replace("<br />", "\n").Replace("<i class=\"bi bi-train-front\"></i>","");
				}
				_lastUpdate -= 5;
			}
			_lastUpdate += RTime.Elapsedf;
		}

		public void RebuildUI() {
			if (programWindows.Count <= 0) {
				return;
			}
			if (programWindows[0].Target is null) {
				return;
			}
			if (Engine.netApiManager.Client.IsLogin) {
				CloseProgram();
				return;
			}
			var window = programWindows[0].Target;
			window.Entity.DestroyChildren();
			_loadingText = null;
			var mainBox = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>().Entity.AddChild("Box").AttachComponent<BoxContainer>();
			mainBox.Vertical.Value = true;
			mainBox.Alignment.Value = RBoxContainerAlignment.Center;
			mainBox.HorizontalFilling.Value = mainBox.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			var boxRoot = mainBox.Entity;
			if (Engine.netApiManager.Client.IsOnline) {
				var loadingImg = boxRoot.AddChild("Img").AttachComponent<TextureRect>();
				loadingImg.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				loadingImg.Texture.Target = loadingImg.Entity.AttachComponent<RhubarbLogo>();
				loadingImg.IgnoreTextureSize.Value = true;
				loadingImg.StrechMode.Value = RStrechMode.KeepAspectCenter;
				loadingImg.MinSize.Value = new Vector2i(100);
				_lastUpdate = 6;

				var rloadingText = boxRoot.AddChild("TopText").AttachComponent<TextLabel>();
				rloadingText.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rloadingText.VerticalAlignment.Value = RVerticalAlignment.Center;
				rloadingText.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				rloadingText.AutowrapMode.Value = RAutowrapMode.WordSmart;
				rloadingText.TextSize.Value = 36;
				rloadingText.MinSize.Value = new Vector2i(350, 0);
				var TopText = rloadingText.Entity.AttachComponent<StandardLocale>();
				TopText.TargetValue.Target = rloadingText.Text;
				TopText.Key.Value = "Programs.Login.Login";

				var email = boxRoot.AddChild("email").AttachComponent<LineEdit>();
				email.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var emailLocal = email.Entity.AttachComponent<StandardLocale>();
				emailLocal.TargetValue.Target = email.PlaceholderText;
				emailLocal.Key.Value = "Programs.Login.Email";
				email.MinSize.Value = new Vector2i(350, 0);

				var password = boxRoot.AddChild("password").AttachComponent<LineEdit>();
				password.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var passwordLocal = password.Entity.AttachComponent<StandardLocale>();
				passwordLocal.TargetValue.Target = password.PlaceholderText;
				passwordLocal.Key.Value = "Programs.Login.Password";
				password.Secret.Value = true;
				password.MinSize.Value = new Vector2i(350, 0);

				var forgotPassword = boxRoot.AddChild("ForgotPassword").AttachComponent<LinkButton>();
				forgotPassword.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				forgotPassword.MinSize.Value = new Vector2i(350, 0);
				var forgotPasswordLocal = forgotPassword.Entity.AttachComponent<StandardLocale>();
				forgotPasswordLocal.TargetValue.Target = forgotPassword.Text;
				forgotPasswordLocal.Key.Value = "Programs.Login.Forgot password";


				var rbuttonLogin = boxRoot.AddChild("Login").AttachComponent<Button>();
				rbuttonLogin.Pressed.Target = GoOnline;
				rbuttonLogin.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rbuttonLogin.MinSize.Value = new Vector2i(100, 0);
				rbuttonLogin.Alignment.Value = RButtonAlignment.Center;
				var loginLoc = rbuttonLogin.Entity.AttachComponent<StandardLocale>();
				loginLoc.TargetValue.Target = rbuttonLogin.Text;
				loginLoc.Key.Value = "Programs.Login.LoginButton";

				var RegBox = boxRoot.AddChild("Regester").AttachComponent<BoxContainer>();
				RegBox.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				RegBox.Alignment.Value = RBoxContainerAlignment.Center;
				var labal = RegBox.Entity.AddChild("Text").AttachComponent<TextLabel>();
				labal.TextSize.Value = 17;
				labal.VerticalAlignment.Value = RVerticalAlignment.Top;
				var labalLoc = rbuttonLogin.Entity.AttachComponent<StandardLocale>();
				labalLoc.TargetValue.Target = labal.Text;
				labalLoc.Key.Value = "Programs.Login.RegisterText";

				var button = RegBox.Entity.AddChild("RegButton").AttachComponent<LinkButton>();
				var buttonLoc = rbuttonLogin.Entity.AttachComponent<StandardLocale>();
				buttonLoc.TargetValue.Target = button.Text;
				buttonLoc.Key.Value = "Programs.Login.RegisterButton";

			}
			else {
				var loadingImg = boxRoot.AddChild("Img").AttachComponent<TextureRect>();
				loadingImg.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				loadingImg.Texture.Target = loadingImg.Entity.AttachComponent<RhubarbLogo>();
				loadingImg.IgnoreTextureSize.Value = true;
				loadingImg.StrechMode.Value = RStrechMode.KeepAspectCenter;
				loadingImg.MinSize.Value = new Vector2i(100);
				_lastUpdate = 6;
				var rloadingText = boxRoot.AddChild("Offline").AttachComponent<TextLabel>();
				rloadingText.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rloadingText.VerticalAlignment.Value = RVerticalAlignment.Center;
				rloadingText.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				rloadingText.AutowrapMode.Value = RAutowrapMode.WordSmart;
				rloadingText.TextSize.Value = 36;
				rloadingText.MinSize.Value = new Vector2i(350, 0);
				var ofline = rloadingText.Entity.AttachComponent<StandardLocale>();
				ofline.TargetValue.Target = rloadingText.Text;
				ofline.Key.Value = "Programs.Offline.Name";
				_loadingText = boxRoot.AddChild("Offline").AttachComponent<TextLabel>();
				_loadingText.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				_loadingText.VerticalAlignment.Value = RVerticalAlignment.Center;
				_loadingText.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				_loadingText.AutowrapMode.Value = RAutowrapMode.WordSmart;
				_loadingText.TextSize.Value = 20;
				_loadingText.MinSize.Value = new Vector2i(350, 0);

				var rbutton = boxRoot.AddChild("Offline").AttachComponent<Button>();
				rbutton.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rbutton.Pressed.Target = GoOnline;
				rbutton.MinSize.Value = new Vector2i(350, 0);
				var oflinee = rbutton.Entity.AttachComponent<StandardLocale>();
				oflinee.TargetValue.Target = rbutton.Text;
				oflinee.Key.Value = "Programs.Offline.GoOnline";

			}
		}
		[Exposed]
		public void GoOnline() {
			Task.Run(Engine.netApiManager.Client.CheckForInternetConnectionLoop);
		} 


	}
}
