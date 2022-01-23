using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Rendering)]
	public class PrivateSpaceManager : RenderingComponent
	{
		public Pose privatePose;
		public Pose debugPose;
		public Pose consolePose;

		public bool Console = false;
		public bool Debug = false;
		public bool CreateAccountScreen = false;

		public Random random = new();

		public string username = "Username";
		public string email = "Email@ef.com";
		public string password = "Password32";

		public string error = "";

		public string sessionID = " ";

		public SessionInfo[] sessions;

		private void RenderDebugWindow() {
			UI.WindowBegin("===---===   Debug Window   ===---===", ref debugPose);
			UI.Text(@$"

=====---- Focused World ----=====
LastFocusChange {WorldManager.FocusedWorld?.LastFocusChange}
IsLoading {WorldManager.FocusedWorld?.IsLoading}
IsLoadingNet {WorldManager.FocusedWorld?.IsLoadingNet}
WaitingForState {WorldManager.FocusedWorld?.WaitingForWorldStartState}
IsDeserializing {WorldManager.FocusedWorld?.IsDeserializing}

UserID {WorldManager.FocusedWorld?.LocalUserID}
UserCount {WorldManager.FocusedWorld?.Users.Count}

Updating Entities {WorldManager.FocusedWorld?.UpdatingEntityCount}
Entities {WorldManager.FocusedWorld?.EntityCount}
Networkeds {WorldManager.FocusedWorld?.NetworkedObjectsCount}
WorldObjects {WorldManager.FocusedWorld?.WorldObjectsCount}
RenderComponents {WorldManager.FocusedWorld?.RenderingComponentsCount}
GlobalStepables {WorldManager.FocusedWorld?.GlobalStepableCount}
stepTime {(WorldManager.FocusedWorld?.stepTime * 1000f).Value:f3}ms

=====---- EngineStatistics ----=====
worldManager stepTime {WorldManager.TotalStepTime * 1000f:f3}ms
FPS {1 / Time.Elapsedf:f3}
RunningTime {Time.Totalf:f3}
Worlds Open {WorldManager.worlds.Count()}

=====FocusedWorld NetStatistics=====
{((WorldManager.FocusedWorld?.NetStatistics is not null)?
$@"BytesReceived {WorldManager.FocusedWorld?.NetStatistics?.BytesReceived.ToString()}
BytesSent {WorldManager.FocusedWorld?.NetStatistics?.BytesSent}
PacketLoss {WorldManager.FocusedWorld?.NetStatistics?.PacketLoss}
PacketLossPercent {WorldManager.FocusedWorld?.NetStatistics?.PacketLossPercent}
PacketsReceived {WorldManager.FocusedWorld?.NetStatistics?.PacketsReceived}
PacketsSent {WorldManager.FocusedWorld?.NetStatistics?.PacketsSent}": "No NetStatistics for this world")}
=====-----------------------=====
");
			var serverindex = 0;
			foreach (var item in WorldManager.FocusedWorld?.relayServers) {
				UI.Text($"RelayServer{serverindex} Connections{item.peers.Count}");
				serverindex++;
			}
			UI.WindowEnd();
		}

		private void RenderConsole() {
			UI.WindowBegin("===---===   Console Window   ===---===", ref consolePose);
			UI.Text(Engine.outputCapture.singleString);
			UI.WindowEnd();
		}

		private void RenderPrivateWindow() {
#if DEBUG
			UI.WindowBegin($"Private UI: MilkSnake Version:{Engine.version}", ref privatePose, UIWin.Normal, UIMove.FaceUser);
#else
			UI.WindowBegin($"Private UI Version:{Engine.version}", ref privatePose, UIWin.Normal, UIMove.FaceUser);
#endif
			UI.Toggle("Console", ref Console);
			UI.SameLine();
			UI.Toggle("Debug", ref Debug);
			if (Engine.netApiManager.IsLoggedIn) {
				UI.SameLine();
				UI.Label("Hello " + Engine.netApiManager.User?.UserName ?? "null");
				UI.Input("SessionID Field", ref sessionID, new Vec2(0.25f, 0.03f));
				UI.SameLine();
				if (UI.Button("Join Session")) {
					Task.Run(async() => {WorldManager.JoinNewWorld(sessionID, WorldObjects.World.FocusLevel.Focused); Thread.Sleep(300); sessions = (await Engine.netApiManager.GetSessions()).ToArray(); });
				}
				if (UI.Button("Start New Session")) {
					Task.Run(async () => { WorldManager.CreateNewWorld(WorldObjects.World.FocusLevel.Focused, false, $"{Engine.netApiManager.User?.UserName ?? "null"} Session"); Thread.Sleep(500); sessions = (await Engine.netApiManager.GetSessions()).ToArray(); });
				}
				UI.SameLine();
				if (UI.Button("Refresh List")) {
					Task.Run(async () => sessions = (await Engine.netApiManager.GetSessions()).ToArray());
				}
				var count = 1;
				for (int i = 0; i < sessions.Length; i++) {
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
				if ((sessions?.Count()??0) == 0) {
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
						if (UI.Button(" "+item.SessionName.Value)) {
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


		public override void OnLoaded() {
			base.OnLoaded();
			username = $"Username{(int)(random.NextDouble()*100)}";
			email = $"Email{(int)(random.NextDouble() * 100)}@de.de";
			privatePose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));
			debugPose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));
			consolePose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));
		}

		readonly bool _uIOpen = true;

		public override void Render() {
			Hierarchy.Push(Renderer.CameraRoot);
			Hierarchy.Push(Matrix.S(0.75f));
			if (_uIOpen) {
				RenderPrivateWindow();
			}
			Hierarchy.Pop();
			Hierarchy.Push(Matrix.S(0.5f));
			if (Debug) {
				RenderDebugWindow();
			}
			if (Console) {
				RenderConsole();
			}
			Hierarchy.Pop();
			Hierarchy.Pop();
		}
	}
}
