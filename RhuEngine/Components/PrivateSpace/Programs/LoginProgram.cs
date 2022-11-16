using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
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

		public override void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args) {
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
					_loadingText.Text.Value = RhubarbLoadingFacts.GetRandomFact(Engine.localisationManager).Replace("<br />", "\n").Replace("<i class=\"bi bi-train-front\"></i>", "");
				}
				_lastUpdate -= 5;
			}
			_lastUpdate += RTime.Elapsedf;
		}
		private bool _loginUI;

		private TextureRect _loadingImg;
		private StandardLocale _labalLoc;
		private StandardLocale _topText;
		private StandardLocale _infoTopText;
		private LineEdit _username;
		private LineEdit _email;
		private LineEdit _password;
		private LineEdit _twoFA;
		private LineEdit _confpassword;
		private StandardLocale _regbuttonLoc;

		private StandardLocale _loginLoc;
		private BoxContainer _loganRegBox;
		private BoxContainer _regBox;
		private LinkButton _forgotPassword;
		private bool _login;
		[Exposed]
		public void LoginReg() {
			if (_login) {
				Task.Run(async () => {
					var res = await Engine.netApiManager.Client.Login(_email.Text.Value, _password.Text.Value, _twoFA.Text.Value);
					Error(res.MSG);
				});
			}
			else {
				if (_password.Text.Value != _confpassword.Text.Value) {
					Error("Programs.Login.MatchPassword");
					return;
				}
				Task.Run(async () => {
					var res = await Engine.netApiManager.Client.RegisterAccount(_username.Text.Value, _password.Text.Value, _email.Text.Value);
					if (res.Data == "Programs.Login.Code") {
						TwoFaLoad();
						Error("");
						return;
					}
					if (res.IsDataGood) {
						Error(res.Data);
					}
					else {
						Error(res.Data);
					}
				});
			}
		}

		public void Error(string msg) {
			_infoTopText.Key.Value = msg;
		}

		private void TwoFaLoad() {
			RenderThread.ExecuteOnStartOfFrame(() => {
				_twoFA.Entity.enabled.Value = true;
				_username.Entity.enabled.Value = false;
				_email.Entity.enabled.Value = false;
				_password.Entity.enabled.Value = false;
				_confpassword.Entity.enabled.Value = false;
				_forgotPassword.Entity.enabled.Value = false;
				_regBox.Entity.enabled.Value = false;
				_loganRegBox.Entity.enabled.Value = false;
			});
		}

		[Exposed]
		public void ForgotPassword() {
			Task.Run(async () => Error(await Engine.netApiManager.Client.SendForgotPassword(_email.Text.Value)));
		}

		[Exposed]
		public void ToggleLoginScreen() {
			_login = !_login;
			if (_login) {
				_loadingImg.Entity.enabled.Value = true;
				_loginLoc.Key.Value = "Programs.Login.LoginButton";
				_forgotPassword.Entity.enabled.Value = true;
				_username.Entity.enabled.Value = false;
				_confpassword.Entity.enabled.Value = false;
				_topText.Key.Value = "Programs.Login.Login";
				_labalLoc.Key.Value = "Programs.Login.RegisterText";
				_regbuttonLoc.Key.Value = "Programs.Login.RegisterButton";
				_loganRegBox.Entity.enabled.Value = false;
			}
			else {
				_loadingImg.Entity.enabled.Value = false;
				_loginLoc.Key.Value = "Programs.Login.RegisterButton";
				_forgotPassword.Entity.enabled.Value = false;
				_username.Entity.enabled.Value = true;
				_confpassword.Entity.enabled.Value = true;
				_topText.Key.Value = "Programs.Login.Register";
				_labalLoc.Key.Value = "Programs.Login.LoginText";
				_regbuttonLoc.Key.Value = "Programs.Login.LoginButton";
				_loganRegBox.Entity.enabled.Value = true;
			}
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
			if (_loginUI && Engine.netApiManager.Client.IsOnline) {
				return;
			}
			_loginUI = false;
			var window = programWindows[0].Target;
			window.Entity.DestroyChildren();
			_loadingText = null;
			var mainBox = window.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>().Entity.AddChild("Box").AttachComponent<BoxContainer>();
			mainBox.Vertical.Value = true;
			mainBox.Alignment.Value = RBoxContainerAlignment.Center;
			mainBox.HorizontalFilling.Value = mainBox.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			var boxRoot = mainBox.Entity;
			if (Engine.netApiManager.Client.IsOnline) {
				_loadingImg = boxRoot.AddChild("Img").AttachComponent<TextureRect>();
				_loadingImg.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				_loadingImg.Texture.Target = _loadingImg.Entity.AttachComponent<RhubarbLogo>();
				_loadingImg.IgnoreTextureSize.Value = true;
				_loadingImg.StrechMode.Value = RStrechMode.KeepAspectCenter;
				_loadingImg.MinSize.Value = new Vector2i(100);
				_lastUpdate = 6;

				var rloadingText = boxRoot.AddChild("TopText").AttachComponent<TextLabel>();
				rloadingText.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rloadingText.VerticalAlignment.Value = RVerticalAlignment.Center;
				rloadingText.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				rloadingText.AutowrapMode.Value = RAutowrapMode.WordSmart;
				rloadingText.TextSize.Value = 36;
				rloadingText.MinSize.Value = new Vector2i(350, 0);
				_topText = rloadingText.Entity.AttachComponent<StandardLocale>();
				_topText.TargetValue.Target = rloadingText.Text;
				_topText.Key.Value = "Programs.Login.Login";

				var rIndfoText = boxRoot.AddChild("InfoTopText").AttachComponent<TextLabel>();
				rIndfoText.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rIndfoText.VerticalAlignment.Value = RVerticalAlignment.Center;
				rIndfoText.HorizontalAlignment.Value = RHorizontalAlignment.Center;
				rIndfoText.AutowrapMode.Value = RAutowrapMode.WordSmart;
				rIndfoText.TextSize.Value = 25;
				rIndfoText.MinSize.Value = new Vector2i(350, 0);
				_infoTopText = rIndfoText.Entity.AttachComponent<StandardLocale>();
				_infoTopText.TargetValue.Target = rIndfoText.Text;
				_infoTopText.Key.Value = " ";

				_twoFA = boxRoot.AddChild("twoFA").AttachComponent<LineEdit>();
				_twoFA.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var twoFALocal = _twoFA.Entity.AttachComponent<StandardLocale>();
				twoFALocal.TargetValue.Target = _twoFA.PlaceholderText;
				twoFALocal.Key.Value = "Programs.Login.Code";
				_twoFA.MinSize.Value = new Vector2i(350, 0);
				_twoFA.Entity.enabled.Value = false;

				_username = boxRoot.AddChild("username").AttachComponent<LineEdit>();
				_username.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var usernameLocal = _username.Entity.AttachComponent<StandardLocale>();
				usernameLocal.TargetValue.Target = _username.PlaceholderText;
				usernameLocal.Key.Value = "Programs.Login.Username";
				_username.MinSize.Value = new Vector2i(350, 0);


				_email = boxRoot.AddChild("email").AttachComponent<LineEdit>();
				_email.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var emailLocal = _email.Entity.AttachComponent<StandardLocale>();
				emailLocal.TargetValue.Target = _email.PlaceholderText;
				emailLocal.Key.Value = "Programs.Login.Email";
				_email.MinSize.Value = new Vector2i(350, 0);

				_password = boxRoot.AddChild("password").AttachComponent<LineEdit>();
				_password.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var passwordLocal = _password.Entity.AttachComponent<StandardLocale>();
				passwordLocal.TargetValue.Target = _password.PlaceholderText;
				passwordLocal.Key.Value = "Programs.Login.Password";
				_password.Secret.Value = true;
				_password.MinSize.Value = new Vector2i(350, 0);



				_confpassword = boxRoot.AddChild("confpassword").AttachComponent<LineEdit>();
				_confpassword.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				var confpasswordLocal = _confpassword.Entity.AttachComponent<StandardLocale>();
				confpasswordLocal.TargetValue.Target = _confpassword.PlaceholderText;
				confpasswordLocal.Key.Value = "Programs.Login.ConfirmPassword";
				_confpassword.Secret.Value = true;
				_confpassword.MinSize.Value = new Vector2i(350, 0);

				_forgotPassword = boxRoot.AddChild("ForgotPassword").AttachComponent<LinkButton>();
				_forgotPassword.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				_forgotPassword.MinSize.Value = new Vector2i(350, 0);
				_forgotPassword.Pressed.Target = ForgotPassword;
				var forgotPasswordLocal = _forgotPassword.Entity.AttachComponent<StandardLocale>();
				forgotPasswordLocal.TargetValue.Target = _forgotPassword.Text;
				forgotPasswordLocal.Key.Value = "Programs.Login.Forgot password";


				var rbuttonLogin = boxRoot.AddChild("Login").AttachComponent<Button>();
				rbuttonLogin.Pressed.Target = LoginReg;
				rbuttonLogin.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				rbuttonLogin.MinSize.Value = new Vector2i(100, 0);
				rbuttonLogin.Alignment.Value = RButtonAlignment.Center;
				_loginLoc = rbuttonLogin.Entity.AttachComponent<StandardLocale>();
				_loginLoc.TargetValue.Target = rbuttonLogin.Text;
				_loginLoc.Key.Value = "Programs.Login.LoginButton";

				_loganRegBox = boxRoot.AddChild("Login").AttachComponent<BoxContainer>();
				_loganRegBox.Vertical.Value = true;
				_loganRegBox.HorizontalFilling.Value = RFilling.ShrinkCenter;
				_loganRegBox.Alignment.Value = RBoxContainerAlignment.Center;

				var labalBYRef = _loganRegBox.Entity.AddChild("Text").AttachComponent<TextLabel>();
				labalBYRef.TextSize.Value = 17;
				labalBYRef.VerticalAlignment.Value = RVerticalAlignment.Top;
				labalBYRef.Text.Value = "By registering, you agree to RhubarbVR's";
				var eLoganRegBox = _loganRegBox.Entity.AddChild("Login").AttachComponent<BoxContainer>();
				eLoganRegBox.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				eLoganRegBox.Alignment.Value = RBoxContainerAlignment.Center;
				var buttonTerms = eLoganRegBox.Entity.AddChild("RegButton").AttachComponent<LinkButton>();
				buttonTerms.Text.Value = "Terms of service";
				buttonTerms.Entity.AttachComponent<UILinkOpen>().TargetUri.Value = new Uri("https://rhubarbvr.net/terms");
				var laband = eLoganRegBox.Entity.AddChild("Text").AttachComponent<TextLabel>();
				laband.TextSize.Value = 17;
				laband.VerticalAlignment.Value = RVerticalAlignment.Top;
				laband.Text.Value = "and";
				var buttonPrivacy = eLoganRegBox.Entity.AddChild("RegButton").AttachComponent<LinkButton>();
				buttonPrivacy.Text.Value = "Privacy Policy";
				buttonPrivacy.Entity.AttachComponent<UILinkOpen>().TargetUri.Value = new Uri("https://rhubarbvr.net/privacy");

				_regBox = boxRoot.AddChild("Regester").AttachComponent<BoxContainer>();
				_regBox.HorizontalFilling.Value = RFilling.ShrinkCenter | RFilling.Expand;
				_regBox.Alignment.Value = RBoxContainerAlignment.Center;
				var labal = _regBox.Entity.AddChild("Text").AttachComponent<TextLabel>();
				labal.TextSize.Value = 17;
				labal.VerticalAlignment.Value = RVerticalAlignment.Top;
				_labalLoc = rbuttonLogin.Entity.AttachComponent<StandardLocale>();
				_labalLoc.TargetValue.Target = labal.Text;
				_labalLoc.Key.Value = "Programs.Login.RegisterText";

				var button = _regBox.Entity.AddChild("RegButton").AttachComponent<LinkButton>();
				_regbuttonLoc = rbuttonLogin.Entity.AttachComponent<StandardLocale>();
				_regbuttonLoc.TargetValue.Target = button.Text;
				_regbuttonLoc.Key.Value = "Programs.Login.RegisterButton";
				button.Pressed.Target = ToggleLoginScreen;
				ToggleLoginScreen();
				_loginUI = true;
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
