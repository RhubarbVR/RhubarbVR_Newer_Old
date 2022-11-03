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
	[Category(new string[] { "Executable" })]
	public sealed class ViewPortProgramWindow : ProgramWindow
	{
		[Default("ViewPort Program Window")]
		[OnChanged(nameof(UpdateData))]
		public readonly Sync<string> Title;

		public readonly SyncRef<Viewport> ViewPort;

		[OnAssetLoaded(nameof(UpdateData))]
		public readonly AssetRef<RTexture2D> IconTexture;
		[OnChanged(nameof(SizePosUpdate))]
		public readonly Linker<Vector2i> ViewPortSizeLink;
		[OnChanged(nameof(SizePosUpdate))]
		public readonly Sync<Vector2i> Size;
		[OnChanged(nameof(SizePosUpdate))]
		public readonly Sync<Vector2f> WindowPos;
		[OnChanged(nameof(UpdateData))]
		public readonly Sync<bool> WindowCanClose;

		public override event Action OnUpdatedData;
		public override event Action OnUpdatePosAndScale;

		public override RTexture2D Texture => ViewPort.Target?.Value;

		public override string WindowTitle => Title.Value;

		public override RTexture2D Icon => IconTexture.Asset;

		public override Vector2f Pos { get => WindowPos.Value; set => WindowPos.Value = value; }
		public override Vector2i SizePixels { get => Size.Value; set => Size.Value = value; }

		public override bool CanClose => WindowCanClose.Value;

		public void UpdateData() {
			OnUpdatedData?.Invoke();
		}

		public void SizePosUpdate() {
			OnUpdatePosAndScale?.Invoke();
			if (ViewPortSizeLink.Linked) {
				ViewPortSizeLink.LinkedValue = Size.Value;
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			Size.Value = new Vector2i(512);
			var viewPort = ViewPort.Target = Entity.AttachComponent<Viewport>();
			ViewPortSizeLink.Target = viewPort.Size;
		}

		public void AddRawTexture(RTexture2D rTexture2D) {
			var texture = Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			texture.LoadAsset(rTexture2D);
			IconTexture.Target = texture;
		}
	}
}
