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
	[PrivateSpaceOnly]
	public sealed class PrivateSpaceTaskbarItem : Component
	{
		private RawAssetProvider<RTexture2D> _iconProvider;

		private Panel _isOpen;

		private Button _closeButton;


		protected override void OnLoaded() {
			base.OnLoaded();
			PrivateSpaceManager.UserInterfaceManager.privateSpaceTaskbarItems.Add(this);
		}

		protected override void OnAttach() {
			base.OnAttach();
			var root = Entity.AttachComponent<UIElement>();
			root.VerticalFilling.Value = RFilling.ShrinkCenter;
			root.MinSize.Value = new Vector2i(80);

			var mainButton = Entity.AddChild("MainButton").AttachComponent<Button>();
			mainButton.Flat.Value = true;

			mainButton.IconAlignment.Value = RButtonAlignment.Center;
			mainButton.ExpandIcon.Value = true;
			mainButton.Icon.Target = _iconProvider = Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			_iconProvider.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.MissingFile));

			_isOpen = Entity.AddChild("IsOpen").AttachComponent<Panel>();
			_isOpen.Entity.enabled.Value = false;
			_isOpen.Min.Value = new Vector2f(0, 1);
			_isOpen.MinOffset.Value = new Vector2f(-15, -10);
			_isOpen.MaxOffset.Value = new Vector2f(15, -5);
			_isOpen.GrowHorizontal.Value = RGrowHorizontal.Both;
			_isOpen.GrowVertical.Value = RGrowVertical.Top;

			_closeButton = Entity.AddChild("CloseButton").AttachComponent<Button>();
			_closeButton.IconAlignment.Value = RButtonAlignment.Center;
			_closeButton.ExpandIcon.Value = true;
			_closeButton.Min.Value = new Vector2f(1, 0);
			_closeButton.Max.Value = new Vector2f(1, 0);
			_closeButton.MinOffset.Value = new Vector2f(0, 5);
			_closeButton.MaxOffset.Value = new Vector2f(-30, 35);
			_closeButton.GrowHorizontal.Value = RGrowHorizontal.Left;

			_closeButton.Pressed.Target = OnClose;
			var tex = _closeButton.Entity.AddChild("Textuer").AttachComponent<TextureRect>();
			var closeIcon = tex.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			tex.Texture.Target = closeIcon;
			tex.InputFilter.Value = RInputFilter.Ignore;
			closeIcon.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Close));
			tex.IgnoreTextureSize.Value = true;
			tex.StrechMode.Value = RStrechMode.KeepAspectCenter;
			tex.MinOffset.Value = new Vector2f(-3);
			tex.MaxOffset.Value = new Vector2f(3);
		}
		[Exposed]
		public void OnClose() {
			_privateSpaceWindow?.Window.Close();
		}

		public override void Dispose() {
			base.Dispose();
			PrivateSpaceManager?.UserInterfaceManager.privateSpaceTaskbarItems.Remove(this);
		}


		private PrivateSpaceWindow _privateSpaceWindow;
		public void OpennedPorgram(PrivateSpaceWindow privateSpaceWindow) {
			if (_privateSpaceWindow is not null) {
				_privateSpaceWindow.Window.OnUpdatedData -= Window_UpdateData;
			}
			privateSpaceWindow.Window.OnUpdatedData += Window_UpdateData;
			_privateSpaceWindow = privateSpaceWindow;
			_isOpen.Entity.enabled.Value = privateSpaceWindow is not null;
			Window_UpdateData();
		}


		private void Window_UpdateData() {
			_closeButton.Entity.enabled.Value = _privateSpaceWindow?.Window?.CanClose ?? false;
			_iconProvider.LoadAsset(_privateSpaceWindow?.Window?.Icon ?? Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.MissingFile));
		}

		public void WindowClosed() {
			Entity.Destroy();
		}
	}
}
