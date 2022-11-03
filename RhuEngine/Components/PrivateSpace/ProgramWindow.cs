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
		public abstract RTexture2D Texture { get; }

		public abstract string WindowTitle { get; }

		public abstract RTexture2D Icon { get; }

		public abstract event Action OnUpdatedData;
		public abstract event Action OnUpdatePosAndScale;

		public abstract Vector2f Pos { get; set; }

		public abstract Vector2i SizePixels { get; set; }

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public PrivateSpaceWindow PrivateSpaceWindow { get; set; }

		public abstract bool CanClose { get; }

		public event Action OnClosedWindow;

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
	}
}
