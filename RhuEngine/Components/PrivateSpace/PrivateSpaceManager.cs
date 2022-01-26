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
				UI.Toggle(item.Name, ref item.IsOpen);
				UI.SameLine();
			}
			UI.WindowEnd();
		}


		public override void OnLoaded() {
			base.OnLoaded();
			windows = new Window[] { new DebugWindow(Engine,WorldManager),new ConsoleWindow(Engine,WorldManager),new MainWindow(Engine,WorldManager) };
			privatePose = new(-.2f, 0.2f, -0.2f, Quat.LookDir(1, 0, 1));
		}

		readonly bool _uIOpen = true;

		public override void Render() {
			Hierarchy.Push(Renderer.CameraRoot);
			Hierarchy.Push(Matrix.S(0.75f));
			if (_uIOpen) {
				RenderPrivateWindow();
			}
			Hierarchy.Pop();
			foreach (var item in windows) {
				if (item.IsOpen) {
					item.Update();
				}
			}
			Hierarchy.Pop();
		}
	}
}
