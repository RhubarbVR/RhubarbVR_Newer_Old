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
	public sealed partial class PrivateSpaceTaskbarItem : Component
	{
		private RawAssetProvider<RTexture2D> _iconProvider;

		private Panel _isOpen;

		private Button _closeButton;

		private Button _mainButton;


		protected override void OnLoaded() {
			base.OnLoaded();
			lock (PrivateSpaceManager?.UserInterfaceManager.privateSpaceTaskbarItems) {
				PrivateSpaceManager.UserInterfaceManager.privateSpaceTaskbarItems.Add(this);
			}
		}

		private bool _overBackGorund;
		private bool _overClose;

		[Exposed]
		public void InputOverBackGround() {
			_overBackGorund = true;
			InputUpdate();
		}

		[Exposed]
		public void InputOverClose() {
			_overClose = true;
			InputUpdate();
		}
		private bool _overRideClose = false;
		private void InputUpdate() {
			if(_closeButton is null) {
				return;
			}
			_closeButton.Entity.enabled.Value = ((_privateSpaceWindow?.Window?.CanClose ?? false) || _overRideClose) && (_overBackGorund || _overClose);
		}

		[Exposed]
		public void InputLeaveBackGround() {
			_overBackGorund = false;
			InputUpdate();
		}

		[Exposed]
		public void InputLeaveClose() {
			_overClose = false;
			InputUpdate();
		}

		private void UpdatePanel() {
			if(_isOpen is null) {
				return;
			}
			if (_privateSpaceWindow?.Minimized ?? false) {
				_isOpen.MinOffset.Value = new Vector2f(-25, -10);
				_isOpen.MaxOffset.Value = new Vector2f(25, -5);
			}
			else {
				_isOpen.MinOffset.Value = new Vector2f(-15, -10);
				_isOpen.MaxOffset.Value = new Vector2f(15, -5);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var root = Entity.AttachComponent<UIElement>();
			root.VerticalFilling.Value = RFilling.ShrinkCenter;
			root.MinSize.Value = new Vector2i(80);

			_mainButton = Entity.AddChild("MainButton").AttachComponent<Button>();
			_mainButton.Flat.Value = true;
			_mainButton.Pressed.Target = MainButtonClick;

			var inputEvents = _mainButton.Entity.AttachComponent<UIInputEvents>();
			inputEvents.InputEntered.Target = InputOverBackGround;
			inputEvents.InputExited.Target = InputLeaveBackGround;


			_mainButton.IconAlignment.Value = RButtonAlignment.Center;
			_mainButton.ExpandIcon.Value = true;
			_mainButton.Icon.Target = _iconProvider = Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			_iconProvider.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.MissingFile));

			_isOpen = Entity.AddChild("IsOpen").AttachComponent<Panel>();
			_isOpen.Entity.enabled.Value = false;
			_isOpen.Min.Value = new Vector2f(0, 1);
			_isOpen.MinOffset.Value = new Vector2f(-15, -10);
			_isOpen.MaxOffset.Value = new Vector2f(15, -5);
			_isOpen.GrowHorizontal.Value = RGrowHorizontal.Both;
			_isOpen.GrowVertical.Value = RGrowVertical.Top;

			_closeButton = Entity.AddChild("CloseButton").AttachComponent<Button>();
			_closeButton.Entity.enabled.Value = false;
			var inputEventse = _closeButton.Entity.AttachComponent<UIInputEvents>();
			inputEventse.InputEntered.Target = InputOverClose;
			inputEventse.InputExited.Target = InputLeaveClose;

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
			tex.ExpandedMode.Value = RExpandedMode.IgnoreSize;
			tex.StrechMode.Value = RStrechMode.KeepAspectCenter;
			tex.MinOffset.Value = new Vector2f(-3);
			tex.MaxOffset.Value = new Vector2f(3);
		}

		private DateTimeOffset _lastClick;
		private DateTimeOffset _lastLastClick;

		private Action _fallBackOpenAction;

		[Exposed]
		public void MainButtonClick() {
			if (_privateSpaceWindow is null) {
				_fallBackOpenAction?.Invoke();
				return;
			}
			var newTime = DateTimeOffset.UtcNow;
			if ((newTime - _lastLastClick).TotalSeconds <= 0.5f) {
				_privateSpaceWindow.NotMinimized = !_privateSpaceWindow.NotMinimized;
			}
			else if ((newTime - _lastClick).TotalSeconds <= 0.5f) {
				_privateSpaceWindow.Window?.CenterWindowIntoView();
			}
			else if (!_privateSpaceWindow.NotMinimized) {
				_privateSpaceWindow.NotMinimized = !_privateSpaceWindow.NotMinimized;
			}
			_lastLastClick = _lastClick;
			_lastClick = DateTimeOffset.UtcNow;
		}

		private Action _fallBackCloseAction;

		[Exposed]
		public void OnClose() {
			if (_privateSpaceWindow is null) {
				_fallBackCloseAction?.Invoke();
				return;
			}
			_privateSpaceWindow?.Window.Close();
		}

		public override void Dispose() {
			IsDestroying= true;
			OpennedPorgram(null);
			if (PrivateSpaceManager?.UserInterfaceManager?.privateSpaceTaskbarItems is not null) {
				lock (PrivateSpaceManager?.UserInterfaceManager?.privateSpaceTaskbarItems) {
					PrivateSpaceManager?.UserInterfaceManager?.privateSpaceTaskbarItems.Remove(this);
				}
			}
			ProgramsUpdate -= UpdateHide;
			base.Dispose();
		}

		private static event Action ProgramsUpdate;

		private PrivateSpaceWindow _privateSpaceWindow;
		public void OpennedPorgram(PrivateSpaceWindow privateSpaceWindow) {
			if (_privateSpaceWindow?.Window is not null) {
				_privateSpaceWindow.Window.OnUpdatedData -= Window_UpdateData;
				_privateSpaceWindow.OnMinimize -= UpdatePanel;
			}
			_privateSpaceWindow = privateSpaceWindow;
			if (_privateSpaceWindow?.Window is not null) {
				_privateSpaceWindow.Window.OnUpdatedData += Window_UpdateData;
				_privateSpaceWindow.OnMinimize += UpdatePanel;
			}
			if (!IsDestroying) {
				if (_isOpen is not null) {
					_isOpen.Entity.enabled.Value = privateSpaceWindow is not null;
				}
				Window_UpdateData();
				UpdatePanel();
			}
			ProgramsUpdate?.Invoke();
		}

		private Type _innnerType;

		public void SetUpProgramOpen(Type type, int localtion) {
			Entity.orderOffset.Value = localtion + 1;
			_innnerType = type;
			ProgramsUpdate += UpdateHide;
			var (ProgramName, icon) = type.GetProgramInfo();
			if (_mainButton is not null) {
				_mainButton.ToolTipText.Value = ProgramName;
			}
			if (_closeButton is not null) {
				_closeButton.ToolTipText.Value = Engine.localisationManager.GetLocalString("Common.CloseThing", ProgramName);
			}
			_iconProvider.LoadAsset(icon ?? Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.MissingFile));
			_fallBackOpenAction = () => PrivateSpaceManager.ProgramManager.OpenProgram(type);
			UpdateHide();
		}

		private void UpdateHide() {
			if (IsDestroying | IsRemoved) {
				return;
			}
			var foundProgram = false;
			lock (PrivateSpaceManager.UserInterfaceManager.privateSpaceTaskbarItems) {
				foreach (var item in PrivateSpaceManager.UserInterfaceManager.privateSpaceTaskbarItems) {
					if (item._privateSpaceWindow?.Window?.Program is null) {
						continue;
					}
					if (item._privateSpaceWindow.Window.Program.GetType() == _innnerType) {
						item.Entity.orderOffset.Value = Entity.orderOffset.Value;
						Entity.enabled.Value = false;
						foundProgram = true;
						break;
					}
				}
			}
			Entity.enabled.Value = !foundProgram;
		}

		private void Window_UpdateData() {
			if(IsDestroying | IsRemoved) {
				return;
			}
			InputUpdate();
			if (_mainButton is not null) {
				_mainButton.ToolTipText.Value = _privateSpaceWindow?.Window?.WindowTitle ?? string.Empty;
			}
			if (_closeButton is not null) {
				_closeButton.ToolTipText.Value = Engine.localisationManager.GetLocalString("Common.CloseThing", _privateSpaceWindow?.Window?.WindowTitle ?? string.Empty);
			}
			_iconProvider?.LoadAsset(_privateSpaceWindow?.Window?.Icon ?? Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.MissingFile));
			ProgramsUpdate?.Invoke();
		}

		public void WindowClosed() {
			ProgramsUpdate?.Invoke();
			Entity.Destroy();
		}

		public void SetUpItemOpen(IFile file, int count) {
			Entity.orderOffset.Value = count + 1;
			_iconProvider.LoadAsset(file.Texture ?? Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.MissingFile));
			_fallBackOpenAction = () => file.Open();
		}

		internal void SetUpWorld(World item) {
			_iconProvider.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Location));
			_fallBackOpenAction = () => item.Focus = World.FocusLevel.Focused;
			_fallBackCloseAction = () => Engine.worldManager.RemoveWorld(item);
			_overRideClose = item != Engine.worldManager.LocalWorld;
			if (_mainButton is not null) {
				_mainButton.ToolTipText.Value = item.SessionName.Value;
			}
			if (_closeButton is not null) {
				_closeButton.ToolTipText.Value = Engine.localisationManager.GetLocalString("Common.CloseThing", item.SessionName.Value);
			}
			var e = _mainButton.Entity.AddChild("SessionName").AttachComponent<TextLabel>();
			e.Text.Value = item.SessionName.Value;
			e.TextSize.Value = 15;
			e.OverrunBehavior.Value = ROverrunBehavior.TrimEllipsis;
			e.MinOffset.Value = new Vector2f(0, 30);
			e.MaxOffset.Value = new Vector2f(0, 30);
			e.InputFilter.Value = RInputFilter.Ignore;
		}
	}
}
