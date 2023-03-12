using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Local" })]
	public sealed partial class ViewPortProgramToolBar : ProgramToolBar
	{
		[OnChanged(nameof(ViewportUpdate))]
		public readonly SyncRef<Viewport> Viewport;

		public void ViewportUpdate() {
			OnViewportUpdate?.Invoke();
		}


		[OnChanged(nameof(SizePosUpdate))]
		public readonly Linker<Vector2i> ViewPortSizeLink;
		[OnChanged(nameof(SizePosUpdate))]
		public readonly Sync<Vector2i> Size;

		[OnChanged(nameof(UpdateData))]
		public readonly Sync<bool> ToolBarCanClose;

		[OnChanged(nameof(UpdateData))]
		[Default("ProgramName")]
		public readonly Sync<string> ToolbarTitle;

		[OnAssetLoaded(nameof(UpdateData))]
		public readonly AssetRef<RTexture2D> IconTexture;

		private void UpdateData(RTexture2D _) {
			UpdateData();
		}

		public override RTexture2D Icon => IconTexture.Asset;

		public override event Action OnUpdateScale;
		public override event Action OnUpdatedData;
		public override event Action OnViewportUpdate;

		public override string Title => ToolbarTitle.Value;

		public override RTexture2D Texture => Viewport.Target?.Value;

		public override Vector2i SizePixels { get => Size.Value; set => Size.Value = value; }

		public override bool CanClose => ToolBarCanClose.Value;

		public override Viewport TargetViewport => Viewport.Target;

		public void UpdateData() {
			OnUpdatedData?.Invoke();
		}

		public void SizePosUpdate() {
			OnUpdateScale?.Invoke();
			if (ViewPortSizeLink.Linked) {
				ViewPortSizeLink.LinkedValue = Size.Value;
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var viewPort = Viewport.Target = Entity.AttachComponent<Viewport>();
			viewPort.TransparentBG.Value = true;
			ViewPortSizeLink.Target = viewPort.Size;
			Size.Value = new Vector2i(512, 55);
		}
	}
}
