using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine.Components.PrivateSpace.Windows
{
	public abstract class Window
	{
		public abstract bool? OnLogin { get; }
		public Engine Engine { get; }
		public WorldManager WorldManager { get; }
		public abstract string Name { get; }
		public Window(Engine engine, WorldManager worldManager) {
			Engine = engine;
			WorldManager = worldManager;
		}

		public Pose windowPose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));

		public bool IsOpen;

		public virtual void Update() {

		}

		public void CloseDraw() {
			UI.PushTint(new Color(1f, 0f, 0f));
			if (UI.ButtonAt("X",new Vec3(0.2f, Engine.UISettings.padding * 3, 0), new Vec2(Engine.UISettings.padding * 3))) {
				IsOpen = false;
			}
			UI.PopTint();
		}

	}
}
