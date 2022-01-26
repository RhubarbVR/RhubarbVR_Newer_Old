using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine.Components.PrivateSpace.Windows
{
	public class AssetTasksWindow : Window
	{
		public override bool? OnLogin => null;

		public override string Name => "Asset Tasks";

		public override void Update() {
			Hierarchy.Push(Matrix.S(0.5f));
			UI.WindowBegin("    ===---===   Asset Tasks   ===---===", ref windowPose, new Vec2(0.4f, 0));
			CloseDraw();
			if (World.assetSession.Manager.tasks.Count == 0) {
				UI.Text("No Running asset tasks");
			}
			else {
				var num = 0;
				foreach (var item in World.assetSession.Manager.tasks) {
					UI.Text($"Task{num} TaskID:{item.runningTask.Id} Uri{item.assetUri} Status:{item.runningTask.Status}");
					UI.SameLine();
					UI.PushId(num);
					if(UI.Button("Stop Task")) {
						item.Stop();
					}
					UI.PopId();
					num++;
				}
			}
			UI.WindowEnd();
			Hierarchy.Pop();
		}
		public AssetTasksWindow(Engine engine, WorldManager worldManager, WorldObjects.World world) :base(engine,worldManager,world) {
		}
	}
}
