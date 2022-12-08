using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	public abstract class ProgramToolBar : Component
	{
		public abstract Viewport TargetViewport { get; }

		public abstract RTexture2D Texture { get; }

		public abstract event Action OnUpdatedData;
		public abstract event Action OnUpdateScale;
		public abstract RTexture2D Icon { get; }

		public abstract string Title { get; }

		public abstract Vector2i SizePixels { get; set; }

		public PrivateSpaceToolBar PrivateSpaceToolBar { get; internal set; }

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Program Program { get; set; }
		 
		public abstract bool CanClose { get; }

		public event Action OnClosedToolBar;
		public abstract event Action OnViewportUpdate;

		public void Close() {
			if (!CanClose) {
				return;
			}
			Entity.Destroy();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			if (Pointer.GetOwnerID() == World.LocalUserID) {
				ProgramManager.LoadProgramToolBar(this);
			}
		}

		public override void Dispose() {
			if (Pointer.GetOwnerID() == World.LocalUserID) {
				OnClosedToolBar?.Invoke();
				ProgramManager.UnLoadProgramToolBar(this);
			}
			base.Dispose();
		}
	}
}
