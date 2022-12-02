using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	public abstract class Program : Component
	{
		public readonly SyncObjList<SyncRef<ProgramWindow>> programWindows;

		public ProgramWindow this[int c] => programWindows[c].Target;


		public ProgramWindow MainProgramWindow => programWindows.Count >= 1 ? this[0] : null;
		public ViewPortProgramWindow AddWindowWithIcon(RhubarbAtlasSheet.RhubarbIcons icon, string name = null, bool closeProgramOnWindowClose = true, bool canClose = true) {
			var window = AddWindow(name, null, closeProgramOnWindowClose, canClose);
			var nicon = window.Entity.AttachComponent<SingleIconTex>();
			nicon.Icon.Value = icon;
			window.IconTexture.Target = nicon;
			return window;
		}
		public ViewPortProgramWindow AddWindowWithTexture(IAssetProvider<RTexture2D> icon, string name = null, bool closeProgramOnWindowClose = true, bool canClose = true) {
			var window = AddWindow(name, null, closeProgramOnWindowClose, canClose);
			window.IconTexture.Target = icon;
			return window;
		}


		public ViewPortProgramWindow AddWindow(string name = null, RTexture2D icon = null, bool closeProgramOnWindowClose = true, bool canClose = true) {
			var window = Entity.AddChild(name ?? ProgramName).AttachComponent<ViewPortProgramWindow>();
			window.Program = this;
			window.WindowCanClose.Value = canClose;
			if (canClose) {
				if (closeProgramOnWindowClose) {
					window.OnClosedWindow += () => CloseProgram();
				}
			}
			window.Title.Value = name ?? ProgramName;
			if (icon is not null) {
				window.AddRawTexture(icon);
			}
			else if (ProgramIcon is not null) {
				window.AddRawTexture(ProgramIcon);
			}
			programWindows.Add().Target = window;
			window.CenterWindowIntoView();
			return window;
		}

		public abstract RTexture2D ProgramIcon { get; }

		public abstract string ProgramName { get; }

		public abstract void StartProgram(Stream file = null, string mimetype = null, string ex = null, params object[] args);

		public virtual void CloseProgram() {
			Entity.Destroy();
		}

		public void ForceClose() {
			Entity.Destroy();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			if (Pointer.GetOwnerID() == World.LocalUserID) {
				ProgramManager.LoadProgram(this);
			}
		}

		public override void Dispose() {
			base.Dispose();
			if (Pointer.GetOwnerID() == World.LocalUserID) {
				if (programWindows is not null) {
					foreach (SyncRef<ProgramWindow> item in programWindows) {
						item.Target?.Close();
					}
				}
				ProgramManager?.UnLoadProgram(this);
			}
		}
	}
}
