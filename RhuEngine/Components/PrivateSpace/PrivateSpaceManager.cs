using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.Components.PrivateSpace.Windows;
using RhuEngine.WorldObjects.ECS;

using SharedModels;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Rendering)]
	public class PrivateSpaceManager : RenderingComponent
	{
		public Pose privatePose;

		public Window[] windows;

		private void RenderPrivateWindow() {
			UI.WindowBegin("PriveWindow", ref privatePose,UIWin.Body);
			foreach (var item in windows) {

				if (item.OnLogin is null) {
					UI.Toggle(item.Name, ref item.IsOpen);
					UI.SameLine();
				}else if(item.OnLogin.Value == Engine.netApiManager.IsLoggedIn) {
					UI.Toggle(item.Name, ref item.IsOpen);
					UI.SameLine();
				}
				else if(item.IsOpen) {
					item.IsOpen = false;
				}
			}
			if (Engine.netApiManager.IsLoggedIn) {
				UI.Label("Hello " + Engine.netApiManager.User?.UserName ?? "null");
				UI.SameLine();
				if (UI.Button("Logout")) {
					Engine.netApiManager.Logout();
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
						}
						UI.PopTint();
					}
				}
				var count = 2;
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
							}
							UI.PopTint();
						}
						UI.PopId();
						count++;
					}
				}
			}
			UI.WindowEnd();
		}


		public override void OnLoaded() {
			base.OnLoaded();
			windows = new Window[] { new DebugWindow(Engine,WorldManager),new ConsoleWindow(Engine,WorldManager), new SessionWindow(Engine, WorldManager), new LoginWindow(Engine,WorldManager) };
			privatePose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));
		}

		readonly bool _uIOpen = true;

		public override void Render() {
			Hierarchy.Push(Renderer.CameraRoot);
			foreach (var item in windows) {
				if (item.IsOpen) {
					item.Update();
				}
			}
			Hierarchy.Push(Matrix.S(0.75f));
			if (_uIOpen) {
				RenderPrivateWindow();
			}
			Hierarchy.Pop();
			Hierarchy.Pop();
		}
	}
}
