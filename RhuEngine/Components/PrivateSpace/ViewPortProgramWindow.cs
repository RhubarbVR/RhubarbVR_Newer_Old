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
	public sealed class ViewPortProgramWindow : ProgramWindow
	{
		[Default("ViewPort Program Window")]
		[OnChanged(nameof(UpdateData))]
		public readonly Sync<string> Title;
		[OnChanged(nameof(ViewportUpdate))]
		public readonly SyncRef<Viewport> Viewport;

		public void ViewportUpdate() {
			OnViewportUpdate?.Invoke();
		}

		[OnAssetLoaded(nameof(UpdateData))]
		public readonly AssetRef<RTexture2D> IconTexture;
		[OnChanged(nameof(SizePosUpdate))]
		public readonly Linker<Vector2i> ViewPortSizeLink;
		[OnChanged(nameof(SizePosUpdate))]
		public readonly Sync<Vector2i> Size;
		public readonly Sync<Vector2i> WindowMinSize;

		[OnChanged(nameof(SizePosUpdate))]
		public readonly Sync<Vector2f> WindowPos;
		[OnChanged(nameof(UpdateData))]
		public readonly Sync<bool> WindowCanClose;

		public override event Action OnUpdatedData;
		public override event Action OnUpdatePosAndScale;
		public override event Action OnViewportUpdate;

		public override RTexture2D Texture => Viewport.Target?.Value;

		public override string WindowTitle => Title.Value;

		public override RTexture2D Icon => IconTexture.Asset;

		public override Vector2f Pos { get => WindowPos.Value; set => WindowPos.Value = value; }
		public override Vector2i SizePixels { get => Size.Value; set => Size.Value = value; }

		public override bool CanClose => WindowCanClose.Value;

		public override Viewport TargetViewport => Viewport.Target;

		public override Vector2i MinSize => WindowMinSize.Value;

		public void UpdateData() {
			OnUpdatedData?.Invoke();
		}

		public void SizePosUpdate() {
			var clampSize = MathUtil.Max(MinSize, Size.Value);
			if(clampSize != Size.Value) {
				Size.Value = clampSize;
				return;
			}
			OnUpdatePosAndScale?.Invoke();
			if (ViewPortSizeLink.Linked) {
				ViewPortSizeLink.LinkedValue = Size.Value;
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var viewPort = Viewport.Target = Entity.AttachComponent<Viewport>();
			ViewPortSizeLink.Target = viewPort.Size;
			Size.Value = new Vector2i(512);
			WindowMinSize.Value = new Vector2i(235,150);
		}

		public void AddRawTexture(RTexture2D rTexture2D) {
			var texture = Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			texture.LoadAsset(rTexture2D);
			IconTexture.Target = texture;
		}
	}
}
