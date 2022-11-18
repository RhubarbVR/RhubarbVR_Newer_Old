using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	public abstract class ProgramWindow : Component
	{
		public abstract Viewport TargetViewport { get; }

		public abstract RTexture2D Texture { get; }

		public abstract string WindowTitle { get; }

		public abstract RTexture2D Icon { get; }

		public abstract event Action OnUpdatedData;
		public abstract event Action OnUpdatePosAndScale;

		public abstract Vector2f Pos { get; set; }

		public abstract Vector2i SizePixels { get; set; }
		public abstract Vector2i MinSize { get; }

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public PrivateSpaceWindow PrivateSpaceWindow { get; set; }

		public abstract bool CanClose { get; }

		public event Action OnClosedWindow;
		public abstract event Action OnViewportUpdate;

		public void CenterWindowIntoView() {
			var sizeofWindow = Engine.IsInVR ? PrivateSpaceManager.VRViewPort.Size.Value : Engine.windowManager.MainWindow.Size;
			sizeofWindow -= new Vector2i(0, 150);
			SizePixels = new Vector2i(Math.Min(sizeofWindow.x, SizePixels.x), Math.Min(sizeofWindow.y, SizePixels.y));

			Pos = (Vector2f)((sizeofWindow / new Vector2i(2)) - (SizePixels / new Vector2i(2)));
		}

		public void Close() {
			if (!CanClose) {
				return;
			}
			Entity.Destroy();
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			ProgramManager.LoadProgramWindow(this);
		}

		public override void Dispose() {
			base.Dispose();
			OnClosedWindow?.Invoke();
			ProgramManager.UnLoadProgramWindow(this);
		}

		public void Maximize() {
			if(PrivateSpaceWindow is null) {
				return;
			}
			PrivateSpaceWindow.Minimized = false;
		}
	}
}
