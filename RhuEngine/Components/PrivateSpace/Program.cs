﻿using System;
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

		public ViewPortProgramWindow AddWindow(string name = null, RTexture2D icon = null,bool closeProgramOnWindowClose = true,bool canClose = true) {
			var window = Entity.AddChild(name ?? ProgramName).AttachComponent<ViewPortProgramWindow>();
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

		public abstract void StartProgram(object[] args = null, Stream file = null, string mimetype = null, string ex = null);

		public virtual void CloseProgram() {
			Entity.Destroy();
		}

		public void ForceClose() {
			Entity.Destroy();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			ProgramManager.LoadProgram(this);
		}

		public override void Dispose() {
			base.Dispose();
			if (programWindows is not null) {
				foreach (SyncRef<ProgramWindow> item in programWindows) {
					item.Target?.Close();
				}
			}
			ProgramManager?.UnLoadProgram(this);
		}
	}
}
