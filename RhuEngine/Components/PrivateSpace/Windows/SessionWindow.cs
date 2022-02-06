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
	public class SessionWindow : Window
	{
		public string sessionID = " ";

		public SessionInfo[] sessions;

		public override bool? OnLogin => true;

		public override string Name => "Session Joiner";

		public override void Update() {

#if DEBUG
			UI.WindowBegin($"    Session Joiner: MilkSnake Version:{Engine.version}", ref windowPose, new Vec2(0.4f, 0), UIWin.Normal, UIMove.FaceUser);
#else
			UI.WindowBegin($"Session Joiner: Version:{Engine.version}", ref windowPose, UIWin.Normal, UIMove.FaceUser);
#endif
			CloseDraw();
			if (Engine.netApiManager.IsLoggedIn) {
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
			}
			UI.WindowEnd();
		}
		public SessionWindow(Engine engine, WorldManager worldManager,WorldObjects.World world):base(engine,worldManager,world) {
		}
	}
}
