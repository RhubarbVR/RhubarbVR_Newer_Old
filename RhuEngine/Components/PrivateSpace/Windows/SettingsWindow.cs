using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine.Components.PrivateSpace.Windows
{
	public class SettingsWindow : Window
	{
		public override bool? OnLogin => null;

		public override string Name => "Settings";

		public override void Update() {
			Hierarchy.Push(Matrix.S(0.5f));
			UI.WindowBegin("    ===---===   Settings Window   ===---===", ref windowPose, new Vec2(0.4f, 0));
			CloseDraw();
			UI.Text($"Not done can save then edit the settings.json saved to {Engine.SettingsFile}");
			if(UI.Button("Save Settings")) {
				Engine.SaveSettings();
			}
			UI.SameLine();
			if (UI.Button("Load Settings")) {
				Engine.ReloadSettings();
			}
			UI.WindowEnd();
			Hierarchy.Pop();
		}
		public SettingsWindow(Engine engine, WorldManager worldManager, WorldObjects.World world) :base(engine,worldManager,world) {
		}
	}
}
