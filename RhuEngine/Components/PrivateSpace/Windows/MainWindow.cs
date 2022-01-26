using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Managers;

using SharedModels;

using StereoKit;

namespace RhuEngine.Components.PrivateSpace.Windows
{
	public class MainWindow : Window
	{
		public bool CreateAccountScreen = false;

		public Random random = new();

		public string username = "Username";
		public string email = "Email@server.com";
		public string password = "Password";

		public string error = "";

		public string sessionID = " ";

		public SessionInfo[] sessions;

		public override bool? OnLogin => null;

		public override string Name => "Main";

		public override void Update() {

#if DEBUG
			UI.WindowBegin($"Private UI: MilkSnake Version:{Engine.version}", ref windowPose, UIWin.Normal, UIMove.FaceUser);
#else
			UI.WindowBegin($"Private UI Version:{Engine.version}", ref privatePose, UIWin.Normal, UIMove.FaceUser);
#endif
			if (Engine.netApiManager.IsLoggedIn) {
				UI.SameLine();
				UI.Label("Hello " + Engine.netApiManager.User?.UserName ?? "null");
				UI.Input("SessionID Field", ref sessionID, new Vec2(0.25f, 0.03f));
				UI.SameLine();
				if (UI.Button("Join Session")) {
					Task.Run(async () => { WorldManager.JoinNewWorld(sessionID, WorldObjects.World.FocusLevel.Focused); Thread.Sleep(300); sessions = (await Engine.netApiManager.GetSessions()).ToArray(); });
				}
				if (UI.Button("Start New Session")) {
					Task.Run(async () => { WorldManager.CreateNewWorld(WorldObjects.World.FocusLevel.Focused, false, $"{Engine.netApiManager.User?.UserName ?? "null"} Session"); Thread.Sleep(500); sessions = (await Engine.netApiManager.GetSessions()).ToArray(); });
				}
				UI.SameLine();
				if (UI.Button("Refresh List")) {
					Task.Run(async () => sessions = (await Engine.netApiManager.GetSessions()).ToArray());
				}
				var count = 1;
				for (var i = 0; i < (sessions?.Length ?? 0); i++) {
					var item = sessions[i];
					if (count % 3 != 1) {
						UI.SameLine();
					}
					UI.PushId(count);
					if (UI.Button(" " + item.SessionName)) {
						try {
							WorldManager.JoinNewWorld(item.SessionId, WorldObjects.World.FocusLevel.Focused, item.SessionName);
						}
						catch { }
					}
					UI.PopId();
					count++;
				}
				if ((sessions?.Length ?? 0) == 0) {
					UI.Text("No Sessions Found");
				}
				UI.Text("World switcher");
				if (WorldManager.FocusedWorld is not null) {
					UI.PushEnabled(false);
					var e = true;
					UI.Toggle(" " + WorldManager.FocusedWorld.SessionName.Value, ref e);
					UI.PopEnabled();
					if (WorldManager.LocalWorld != WorldManager.FocusedWorld) {
						UI.PushTint(new Color(0.8f, 0, 0));
						UI.SameLine();
						UI.Space(-Engine.UISettings.padding);
						if (UI.Button("X")) {
							try {
								WorldManager.FocusedWorld.Dispose();
							}
							catch { }
							Task.Run(async () => { Thread.Sleep(300); sessions = (await Engine.netApiManager.GetSessions()).ToArray(); });
						}
						UI.PopTint();
					}
				}
				count = 2;
				for (var i = 0; i < WorldManager.worlds.Count; i++) {
					var item = WorldManager.worlds[i];
					if (item.Focus == WorldObjects.World.FocusLevel.Background) {
						if (count % 3 != 1) {
							UI.SameLine();
						}
						UI.PushId(count);
						if (UI.Button(" " + item.SessionName.Value)) {
							item.Focus = WorldObjects.World.FocusLevel.Focused;
						}
						if (WorldManager.LocalWorld != item) {
							UI.PushTint(new Color(0.8f, 0, 0));
							UI.SameLine();
							UI.Space(-Engine.UISettings.padding);
							if (UI.Button("X")) {
								try {
									item.Dispose();
								}
								catch { }
								Task.Run(async () => { Thread.Sleep(300); sessions = (await Engine.netApiManager.GetSessions()).ToArray(); });
							}
							UI.PopTint();
						}
						UI.PopId();
						count++;
					}
				}
				if (UI.Button("Logout")) {
					Engine.netApiManager.Logout();
				}
			}
			else {
				if (CreateAccountScreen) {
					if (error != "") {
						UI.SameLine();
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
								error = ex.ToString();
							}
						});
					}

				}
				else {
					if (error != "") {
						UI.SameLine();
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
								if (data.Login) {
									sessions = (await Engine.netApiManager.GetSessions()).ToArray();
								}
							}
							catch (Exception ex) {
								error = ex.ToString();
							}
						});
					}
				}
				UI.Toggle("Create Account or Login", ref CreateAccountScreen);
			}
			UI.WindowEnd();
		}
		public MainWindow(Engine engine, WorldManager worldManager):base(engine,worldManager) {
#if DEBUG
			var number = (int)(random.NextDouble() * 100);
			username = $"DebugUser{number}";
			email = $"DebugEmail{number}@real.serv";
			password = "Password1";
#endif
		}
	}
}
