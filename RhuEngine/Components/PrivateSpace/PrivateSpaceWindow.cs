using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public sealed class PrivateSpaceWindow : Component
	{
		public ProgramWindow Window { get; set; }

		public PrivateSpaceTaskbarItem PrivateSpaceTaskbarItem { get; set; }

		public void InitPrivateSpaceWindow(ProgramWindow window) {
			Window = window;
			Window.PrivateSpaceWindow = this;
			LoadTaskBarItem();
			LoadUI();
			window.OnClosedWindow += Window_OnClosedWindow;
		}

		private void Window_OnClosedWindow() {
			Entity.Destroy();
		}

		public override void Dispose() {
			base.Dispose();
			PrivateSpaceTaskbarItem?.WindowClosed();
		}

		private void LoadTaskBarItem() {
			PrivateSpaceTaskbarItem = PrivateSpaceManager.UserInterfaceManager._taskbarElementHolder?.Entity?.AddChild(Window.WindowTitle)?.AttachComponent<PrivateSpaceTaskbarItem>();
			PrivateSpaceTaskbarItem?.OpennedPorgram(this);
		}

		private void LoadUI() {

		}

	}
}
