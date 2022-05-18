using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.WorldObjects;
using RhuEngine.Components.PrivateSpace;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	public abstract class Program : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public TaskBar taskBar;


		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Window window;

		public ProgramTaskBarItem taskBarItem;

		public abstract string ProgramID { get; }

		public abstract Vector2i? Icon { get; }

		public abstract RTexture2D Texture { get; }

		public abstract string ProgramName { get; }

		public void IntProgram() {
			World.DrawDebugText(taskBar.Entity.GlobalTrans, new Vector3f(0,1,-1), Vector3f.One, Colorf.Green,"Program Loaded", 5);
			window = Entity.AttachComponent<Window>();
			window.OnMinimize.Target = Minimize;
			window.OnClose.Target = Close;
			window.NameValue.Value = ProgramName;
			LoadUI(window.PannelRoot.Target);
		}

		public abstract void LoadUI(Entity uiRoot);

		public bool Minimized = false;
		[Exsposed]
		public void Minimize() {
			window.Entity.enabled.Value = Minimized;
			Minimized = !Minimized;
		}

		[Exsposed]
		public void Close() {
			Entity.Destroy();
		}

		public override void Dispose() {
			taskBar.ProgramClose(this);
			base.Dispose();
		}

		public void ClickedButton() {
			if (Minimized) {
				Minimize();
			}
		}
	}
}
