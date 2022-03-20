using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Managers;

using SharedModels;
using SharedModels.Session;

using StereoKit;

namespace RhuEngine.Components.PrivateSpace.Windows
{
	public class LoginWindow : Window
	{
		public bool CreateAccountScreen = false;

		public Random random = new();

		public string username = "Username";
		public string email = "Email@server.com";
		public string password = "Password";

		public string error = "";

		public string sessionID = " ";

		public SessionInfo[] sessions;

		public override bool? OnLogin => false;

		public override string Name => "Login";

		public override void OnOpen() {
			base.OnOpen();
			error = "";
		}

		public override void Update() {

#if DEBUG
			UI.WindowBegin($"    Account UI: MilkSnake Version:{Engine.version}", ref windowPose, new Vec2(0.4f, 0), UIWin.Normal, UIMove.FaceUser);
#else
			UI.WindowBegin($"Account UI: Version:{Engine.version}", ref windowPose, UIWin.Normal, UIMove.FaceUser);
#endif
			CloseDraw();
			if (!Engine.netApiManager.IsLoggedIn) {
				if (CreateAccountScreen) {
					if (error != "") {
						UI.Text(error);
					}
					UI.Label("Username");
					UI.SameLine();
					UI.Input("UsernameField", ref username);
					UI.Label("Email");
					UI.SameLine();
					UI.Input("EmailField", ref email);
					UI.Label("Password");
					UI.SameLine();
					UI.Input("PasswordField", ref password);
					if (UI.Button("Create Account")) {
						Task.Run(async () => {
							error = "Creating Account...";
							try {
								var data = await Engine.netApiManager.SignUp(username, email, password, DateTime.Today);
								if (data is null) {
									error = "Unknown Error";
								}
								else {
									if (data.Error) {
										error = data.Message;
										error += data.ErrorDetails;
									}
									else {
										error = data.Message;
									}
								}
							}
							catch (Exception ex) {
								error = ex is ConnectToServerError ? "Server Is Down" : ex.ToString();
							}
						});
					}

				}
				else {
					if (error != "") {
						UI.Text(error);
					}
					UI.Label("Email");
					UI.SameLine();
					UI.Input("EmailField", ref email);
					UI.Label("Password");
					UI.SameLine();
					UI.Input("PasswordField", ref password);
					if (UI.Button("Login")) {
						Task.Run(async () => {
							error = "Logging In...";
							try {
								var data = await Engine.netApiManager.Login(email, password);
								error = data is null ? "Unknown Error" : !data.Login ? data.Message : data.Message;
								if (data?.Login??false) {
									sessions = (await Engine.netApiManager.GetSessions()).ToArray();
								}
							}
							catch (Exception ex) {
								error = ex is ConnectToServerError ? "Server Is Down" : ex.ToString();
							}
						});
					}
				}
				var LastCreateAccountScreen = CreateAccountScreen;
				UI.Toggle("Create Account or Login", ref CreateAccountScreen);
				if (CreateAccountScreen != LastCreateAccountScreen) {
					error = "";
				}
			}
			UI.WindowEnd();
		}
		public LoginWindow(Engine engine, WorldManager worldManager, WorldObjects.World world) :base(engine,worldManager,world) {
#if DEBUG
			var number = (int)(random.NextDouble() * 100);
			username = $"DebugUser{number}";
			email = $"DebugEmail{number}@real.serv";
			password = "Password1";
#endif
		}
	}
}
