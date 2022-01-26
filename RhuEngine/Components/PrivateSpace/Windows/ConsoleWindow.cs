using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Managers;

using StereoKit;

namespace RhuEngine.Components.PrivateSpace.Windows
{
	public class ConsoleWindow : Window
	{
		public override bool? OnLogin => null;

		public override string Name => "Console";

		public override void Update() {
			Hierarchy.Push(Matrix.S(0.5f));
			UI.WindowBegin("===---===   Console Window   ===---===", ref windowPose);
			UI.Text(Engine.outputCapture.singleString);
			UI.WindowEnd();
			Hierarchy.Pop();
		}
		public ConsoleWindow(Engine engine, WorldManager worldManager):base(engine,worldManager) {
		}
	}
}
