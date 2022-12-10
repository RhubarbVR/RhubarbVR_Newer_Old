using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

using Esprima.Ast;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class PrivateSpaceToolBar : Component
	{
		public ProgramToolBar Tool { get; set; }

		public Button ToggleButton { get; set; }
		public ViewportConnector ConnectedViewPort { get; set; }

		public void InitPrivateSpaceToolBar(ProgramToolBar toolBar) {
			Tool = toolBar;
			Tool.PrivateSpaceToolBar = this;
			LoadUI();
			Tool.OnClosedToolBar += Tool_OnClosedToolBar;
			Tool.OnUpdatedData += Tool_OnUpdatedData;
			Tool.OnViewportUpdate += Tool_OnViewportUpdate;
			Tool_OnViewportUpdate();
			RenderThread.ExecuteOnStartOfFrame(() => ButtonUP());
		}

		private void Tool_OnViewportUpdate() {
			if(ConnectedViewPort is null) {
				return;
			}
			ConnectedViewPort.Target.Target = Tool.TargetViewport;
		}

		private void Tool_OnUpdatedData() {
			ToggleButton.ToolTipText.Value = Tool.Title;
			var icon = ToggleButton.Entity.GetFirstComponentOrAttach<RawAssetProvider<RTexture2D>>();
			icon.LoadAsset(Tool.Icon);
			ToggleButton.Icon.Target = icon;
		}

		private void UpdateSelected() {
			for (var i = 0; i < PrivateSpaceManager.UserInterfaceManager.ToolBarHolder.Entity.children.Count; i++) {
				var item = PrivateSpaceManager.UserInterfaceManager.ToolBarHolder.Entity.children[i];
				var ui = item.GetFirstComponent<ViewportConnector>();
				if (ui is null) {
					continue;
				}
				item.enabled.Value = ui == ConnectedViewPort;
			}
			for (var i = 0; i < PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children.Count; i++) {
				var item = PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children[i];
				var ui = item.GetFirstComponent<Button>();
				if (ui is null) {
					continue;
				}
				if (ui != ToggleButton) {
					ui.ButtonPressed.Value = false;
				}
			}
		}

		[Exposed]
		public void ButtonUP() {
			ToggleButton.ButtonPressed.Value = true;
			PrivateSpaceManager.UserInterfaceManager.ToolBarCloseButton.Enabled.Value = Tool.CanClose;
			PrivateSpaceManager.UserInterfaceManager.ToolBarCloseButton.Pressed.Target = CloseTaskBar;
			var sizeOfView = Engine.windowManager.MainWindow.Width;
			if (Engine.IsInVR) {
				sizeOfView = PrivateSpaceManager.VRViewPort.Size.Value.x;
			}
			if (Tool.CanClose) {
				sizeOfView -= 48;
			}
			sizeOfView -= 48 * PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children.Count;
			Tool.SizePixels = new Vector2i(sizeOfView, 45);
			UpdateSelected();
		}

		[Exposed]
		public void CloseTaskBar() {
			Tool.Close();
			for (var i = 0; i < PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children.Count; i++) {
				var item = PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children[i];
				var ui = item.GetFirstComponent<Button>();
				if (ui is null) {
					continue;
				}
				if (ui != ToggleButton) {
					RenderThread.ExecuteOnStartOfFrame(() => ui.ButtonUp.Target?.Invoke());
					break;
				}
			}
		}

		private void LoadUI() {
			if (PrivateSpaceManager?.UserInterfaceManager?.ToolBarButtons?.Entity is null) {
				return;
			}
			ToggleButton = PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.AddChild(Tool.Title).AttachComponent<Button>();
			ToggleButton.IconAlignment.Value = RButtonAlignment.Center;
			ToggleButton.FocusMode.Value = RFocusMode.None;
			ToggleButton.ExpandIcon.Value = true;
			ToggleButton.MinSize.Value = new Vector2i(45);
			ToggleButton.ToggleMode.Value = true;
			ToggleButton.ButtonUp.Target = ButtonUP;
			ConnectedViewPort = PrivateSpaceManager.UserInterfaceManager.ToolBarHolder.Entity.AddChild(Tool.Title).AttachComponent<ViewportConnector>();
			ConnectedViewPort.Target.AllowCrossWorld();

			PrivateSpaceManager.UserInterfaceManager.ToolBarRoot.Enabled.Value = PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children.Count > 0;
		}

		private void Tool_OnClosedToolBar() {
			ConnectedViewPort?.Entity?.Destroy();
			ToggleButton?.Entity?.Dispose();
			if (PrivateSpaceManager?.UserInterfaceManager?.ToolBarButtons?.Entity is not null) {
				PrivateSpaceManager.UserInterfaceManager.ToolBarRoot.Enabled.Value = PrivateSpaceManager.UserInterfaceManager.ToolBarButtons.Entity.children.Count > 0;
			}
			Entity.Destroy();
		}
	}
}
