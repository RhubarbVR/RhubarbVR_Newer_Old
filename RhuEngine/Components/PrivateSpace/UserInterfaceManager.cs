using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.WorldObjects;
using RhuEngine.Components.PrivateSpace;
using System.Globalization;
using System.Collections;
using RhuEngine.Managers;
using DataModel.Enums;
using RhuEngine.Components.UI;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace RhuEngine.Components
{

	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class UserInterfaceManager : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public PrivateSpaceManager _PrivateSpaceManager;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement UserInterface;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement Windows;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public CanvasMesh StartVRElement;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement Start;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity VrElements;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UnlitMaterial UImaterial;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement TaskBarRoot;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement ToolBarHolder;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Button ToolBarCloseButton;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public BoxContainer ToolBarButtons;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement ToolBarRoot;

		public readonly List<PrivateSpaceTaskbarItem> privateSpaceTaskbarItems = new();

		public bool OpenCloseDash
		{
			get => Windows.Enabled.Value;
			set {
				Windows.Enabled.Value = value;
				VrElements.enabled.Value = value && Engine.IsInVR;
				_PrivateSpaceManager.VRViewPort.UpdateMode.Value = value && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
				if (value) {
					InputManager.screenInput.FreeMouse();
				}
				else {
					InputManager.screenInput.UnFreeMouse();
				}
#if DEBUG
				if (Windows.Enabled.Value) {
					RLog.Info("Opened Dash");
				}
				else {
					RLog.Info("Closed Dash");
				}
#endif
				TaskBarRoot.Enabled.Value = Windows.Enabled.Value;
				if (!Windows.Enabled.Value) {
					ToggleStart(false);
					_allProgramsButton.Entity.enabled.Value = true;
					_startscroll.Entity.enabled.Value = true;
					_profileElement.Entity.enabled.Value = false;
					_profileSideButton.ButtonPressed.Value = false;
				}
			}
		}

		public void ToggleDash() {
			OpenCloseDash = !OpenCloseDash;
		}

		[Exposed]
		public void ToggleStart(bool startState) {
#if DEBUG
			if (startState) {
				RLog.Info("Opened Start");
			}
			else {
				RLog.Info("Closed Start");
			}
#endif
			if (!startState && !_startListBox.Entity.enabled.Value) {
				_allProgramsButton.Pressed.Invoke();
			}
			StartVRElement.Entity.enabled.Value = startState;
			Start.Entity.enabled.Value = startState;
			_startButton.ButtonPressed.Value = startState;
		}

		private void EngineLink_VRChange(bool obj) {
			UserInterface.Entity.parent.Target = Engine.IsInVR ? _PrivateSpaceManager.VRViewPort.Entity : _PrivateSpaceManager.RootScreenElement.Entity;
			_PrivateSpaceManager.VRViewPort.UpdateMode.Value = OpenCloseDash && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
			VrElements.enabled.Value = OpenCloseDash && Engine.IsInVR;
			_PrivateSpaceManager.VRViewPort.Enabled.Value = Engine.IsInVR;
		}

		public Entity RootUIEntity => UserInterface.Entity;

		private bool _offlineStart = false;

		private static readonly Type[] _startTypes = new[] {
			typeof(SessionsFinderProgram),
			typeof(CreateWorldProgram),
			typeof(FileExplorerProgram),
			typeof(SettingsProgram),
		};
		private static readonly Type[] _taskBarTypes = new[] {
			typeof(SessionsFinderProgram),
			typeof(CreateWorldProgram),
		};

		private void LoadTaskBarAndStart() {
			if (Engine.netApiManager.Client.IsLogin) {
				ClearStart();
				ClearTaskBar();
				foreach (var item in _startTypes) {
					AddStartProgram(item);
				}
				foreach (var item in _taskBarTypes) {
					AddTaskBarItemProgram(item);
				}
				_offlineStart = false;
			}
			else if (!_offlineStart) {
				ClearStart();
				ClearTaskBar();
				foreach (var item in _startTypes) {
					AddStartProgram(item);
				}
				foreach (var item in _taskBarTypes) {
					AddTaskBarItemProgram(item);
				}
				_offlineStart = true;
			}
		}

		private readonly List<PrivateSpaceTaskbarItem> _taskBarItems = new();

		private void ClearTaskBar() {
			lock (_taskBarItems) {
				foreach (var privateSpaceTaskbarItem in _taskBarItems) {
					privateSpaceTaskbarItem.Entity.Destroy();
				}
				lock (privateSpaceTaskbarItems) {
					foreach (var item in privateSpaceTaskbarItems) {
						if (item.Entity.orderOffset.Value < 500) {
							item.Entity.orderOffset.Value = 0;
						}
					}
				}
				_taskBarItems.Clear();
			}
		}
		private void ClearStart() {
			_startListBox.Entity.DestroyChildren();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		private void AddStartFolder(IFolder folder) {

		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		private void AddStartItem(IFile file) {

		}


		private void BuildAllPrograms() {
			var e = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => !x.IsAbstract).Where(x => x.IsAssignableTo(typeof(Program))).Where(x => x.GetCustomAttribute<ProgramHideAttribute>(true) is null);
			foreach (var type in e) {
				var (ProgramName, icon) = type.GetProgramInfo();
				var button = _startListBoxAllPrograms.Entity.AddChild(type.Name).AttachComponent<Button>();
				button.MinSize.Value = new Vector2i(230, 50);
				button.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
				button.Alignment.Value = RButtonAlignment.Left;
				button.IconAlignment.Value = RButtonAlignment.Left;
				button.ExpandIcon.Value = true;
				button.Text.Value = ProgramName;
				button.ToolTipText.Value = Engine.localisationManager.GetLocalString("Common.Open") + " " + ProgramName;
				var asset = button.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
				button.Icon.Target = asset;
				asset.LoadAsset(icon);
				var delCaller = button.Entity.AttachComponent<DelegateCall>();
				delCaller.action = () => {
					ProgramManager.OpenProgram(type);
					ToggleStart(false);
				};
				button.Pressed.Target = delCaller.CallDelegate;
			}
		}

		private void AddStartProgram(Type type) {
			if (type is null) {
				return;
			}
			var (ProgramName, icon) = type.GetProgramInfo();
			var button = _startListBox.Entity.AddChild(type.Name).AttachComponent<Button>();
			button.MinSize.Value = new Vector2i(230, 50);
			button.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			button.Alignment.Value = RButtonAlignment.Left;
			button.IconAlignment.Value = RButtonAlignment.Left;
			button.ExpandIcon.Value = true;
			button.Text.Value = ProgramName;
			button.ToolTipText.Value = Engine.localisationManager.GetLocalString("Common.Open") + " " + ProgramName;
			var asset = button.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			button.Icon.Target = asset;
			asset.LoadAsset(icon);
			var delCaller = button.Entity.AttachComponent<DelegateCall>();
			delCaller.action = () => {
				ProgramManager.OpenProgram(type);
				ToggleStart(false);
			};
			button.Pressed.Target = delCaller.CallDelegate;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
		private void AddTaskBarItem(IFile file) {
			if (file is null) {
				return;
			}
			lock (_taskBarItems) {
				var newItem = PrivateSpaceManager.UserInterfaceManager._taskbarElementHolder?.Entity?.AddChild(file.Name)?.AttachComponent<PrivateSpaceTaskbarItem>();
				newItem.SetUpItemOpen(file, _taskBarItems.Count);
				_taskBarItems.Add(newItem);
			}
		}
		private void AddTaskBarItemProgram(Type type) {
			if (type is null) {
				return;
			}
			lock (_taskBarItems) {
				var newItem = PrivateSpaceManager.UserInterfaceManager._taskbarElementHolder?.Entity?.AddChild(type.Name)?.AttachComponent<PrivateSpaceTaskbarItem>();
				newItem.SetUpProgramOpen(type, _taskBarItems.Count);
				_taskBarItems.Add(newItem);
			}
		}


		internal void LoadInterface() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			Engine.worldManager.OnWorldUpdateTaskBar = WorldManager_WorldChanged;
			Engine.worldManager.WorldChanged += WorldManager_WorldChanged;
			Engine.netApiManager.Client.OnLogout += Client_OnLogout;
			Engine.netApiManager.Client.OnLogin += Client_OnLogin;
			Engine.netApiManager.Client.HasGoneOfline += Client_OnLogout;
			Engine.netApiManager.Client.StatusUpdate += Client_StatusUpdate;

			Windows = RootUIEntity.AddChild("Windows").AttachComponent<UIElement>();
			Windows.InputFilter.Value = RInputFilter.Pass;
			Windows.Enabled.Value = false;
			UImaterial = Entity.AttachComponent<UnlitMaterial>();
			UImaterial.DullSided.Value = true;
			UImaterial.Transparency.Value = Transparency.Blend;
			UImaterial.MainTexture.Target = _PrivateSpaceManager.VRViewPort;
			VrElements = Entity.AddChild("VrElements");
			var taskBarVRElement = VrElements.AddChild("TaskBarVR").AttachMesh<CanvasMesh>(UImaterial);
			var e = VrElements.AttachComponent<ValueCopy<Vector2i>>();
			e.Target.Target = taskBarVRElement.Resolution;
			e.Source.Target = _PrivateSpaceManager.VRViewPort.Size;
			taskBarVRElement.Max.Value = new Vector2f(1, 0);
			taskBarVRElement.MaxOffset.Value = new Vector2i(0, 100);
			taskBarVRElement.InputInterface.Target = _PrivateSpaceManager.VRViewPort;

			StartVRElement = VrElements.AddChild("StartVR").AttachMesh<CanvasMesh>(UImaterial);
			StartVRElement.TopOffset.Value = false;
			StartVRElement.FrontBindRadus.Value += 1f;
			StartVRElement.Scale.Value += new Vector3f(1, 0, 0);
			StartVRElement.Min.Value = new Vector2f(0, 0);
			StartVRElement.Max.Value = new Vector2f(0, 0);
			StartVRElement.MaxOffset.Value = new Vector2i(350, 544);
			StartVRElement.MinOffset.Value = new Vector2i(0, 100);

			var ee = VrElements.AttachComponent<ValueCopy<Vector2i>>();
			ee.Target.Target = StartVRElement.Resolution;
			ee.Source.Target = _PrivateSpaceManager.VRViewPort.Size;
			StartVRElement.InputInterface.Target = _PrivateSpaceManager.VRViewPort;

			VrElements.enabled.Value = false;
			Engine.EngineLink.VRChange += EngineLink_VRChange;

			TaskBarRoot = RootUIEntity.AddChild("TaskBar").AttachComponent<UIElement>();
			TaskBarRoot.Min.Value = new Vector2f(0, 1);
			TaskBarRoot.MinOffset.Value = new Vector2f(0, -100);

			TaskBarRoot.Entity.AddChild("Panel").AttachComponent<Panel>();
			var taskBardata = TaskBarRoot.Entity.AddChild("BoxContainer").AttachComponent<BoxContainer>();
			TaskBarRoot.Enabled.Value = false;
			var leftElement = taskBardata.Entity.AddChild("Left").AttachComponent<UIElement>();
			leftElement.MinSize.Value = new Vector2i(190, 0);
			var centerElement = taskBardata.Entity.AddChild("Center").AttachComponent<UIElement>();
			centerElement.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			var rightElement = taskBardata.Entity.AddChild("Right").AttachComponent<UIElement>();
			rightElement.MinSize.Value = new Vector2i(200, 0);
			BuildLeftTaskBar(leftElement);
			BuildCenterTaskBar(centerElement);
			BuildRightTaskBar(rightElement);
			EngineLink_VRChange(Engine.IsInVR);

			Start = RootUIEntity.AddChild("Start").AttachComponent<UIElement>();

			BuildStartMenu(Start);
			BuildToolBar();
			ToggleStart(false);
			Client_StatusUpdate();
			if (Engine.netApiManager.Client.IsLogin) {
				Client_OnLogin(Engine.netApiManager.Client.User);
			}
			else {
				Client_OnLogout();
			}
		}

		private void BuildToolBar() {
			ToolBarRoot = RootUIEntity.AddChild("ToolBarRoot").AttachComponent<UIElement>();
			ToolBarRoot.Enabled.Value = false;
			ToolBarRoot.Entity.AddChild("Color").AttachComponent<Panel>();
			ToolBarRoot.Max.Value = new Vector2f(1, 0);
			ToolBarRoot.MaxOffset.Value = new Vector2f(0, 45f);
			ToolBarRoot.InputFilter.Value = RInputFilter.Stop;
			var data = ToolBarRoot.Entity.AddChild("ToolBarRoot").AttachComponent<BoxContainer>();
			ToolBarButtons = data.Entity.AddChild("Buttons").AttachComponent<BoxContainer>();
			ToolBarHolder = data.Entity.AddChild("Holder").AttachComponent<UIElement>();
			ToolBarHolder.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			ToolBarCloseButton = data.Entity.AddChild("CloseTaskBarItem").AttachComponent<Button>();
			ToolBarCloseButton.IconAlignment.Value = RButtonAlignment.Center;
			ToolBarCloseButton.ExpandIcon.Value = true;
			var texture = ToolBarCloseButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			ToolBarCloseButton.Icon.Target = texture;
			texture.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Close));
			ToolBarCloseButton.MinSize.Value = new Vector2i(45);
		}

		private readonly List<PrivateSpaceTaskbarItem> _worldItems = new();

		private void WorldManager_WorldChanged() {
			WorldManager_WorldChanged(null);
		}

		private void WorldManager_WorldChanged(World obj) {
			lock (_worldItems) {
				foreach (var item in _worldItems) {
					item.Entity.Destroy();
				}
				_worldItems.Clear();
				foreach (var item in Engine.worldManager.worlds) {
					if (item.Focus is not World.FocusLevel.Background and not World.FocusLevel.Focused) {
						continue;
					}
					if (item.IsLoading || item.IsDisposed || item.IsRemoved) {
						continue;
					}
					var newItem = PrivateSpaceManager.UserInterfaceManager._taskbarElementHolder?.Entity?.AddChild(item.Name)?.AttachComponent<PrivateSpaceTaskbarItem>();
					//Todo remove or keep show focused world first
					newItem.Entity.orderOffset.Value = item.Focus == World.FocusLevel.Focused ? 502 : 501;
					newItem.SetUpWorld(item);
					_worldItems.Add(newItem);
				}
			}
		}

		private void Client_StatusUpdate() {
			UpdateStatus(Engine?.netApiManager?.Client?.Status?.Status ?? UserStatus.Offline);
			_ststusLineEdit.Text.Value = Engine?.netApiManager?.Client?.Status?.CustomStatusMsg;
		}

		private void Client_OnLogin(RhubarbCloudClient.Model.PrivateUser obj) {
			Client_StatusUpdate();
			_profileSideButton.Entity.enabled.Value = true;
			_usernameLabel.Text.Value = obj.UserName;
			LoadTaskBarAndStart();
			UpdateProfilePic();
		}

		private void UpdateProfilePic() {
			if ((Engine.netApiManager.Client.User?.ProfileIcon ?? Guid.Empty) == Guid.Empty) {
				_profileSideButton.Icon.Target = _profileSideButton.Entity.GetFirstComponent<RawAssetProvider<RTexture2D>>();
			}
			else {
				var data = _profileSideButton.Entity.GetFirstComponentOrAttach<StaticTexture>();
				data.url.Value = Engine.netApiManager.Client.User.ProfileURL;
				_profileSideButton.Icon.Target = data;
			}
		}

		[Exposed]
		public void ChangeStatusText() {
			if (Engine?.netApiManager?.Client?.Status is null) {
				return;
			}
			Engine.netApiManager.Client.Status.CustomStatusMsg = _ststusLineEdit.Text.Value;
			Task.Run(Engine.netApiManager.Client.UpdateStatus);
		}

		private void Client_OnLogout() {
			_profileElement.Entity.enabled.Value = false;
			_profileSideButton.ButtonPressed.Value = false;
			_profileSideButton.Entity.enabled.Value = false;
			LoadTaskBarAndStart();
		}
		Button _onlineButton;
		Button _idleButton;
		Button _doNotDisturbButton;
		Button _streamButton;
		Button _offlineButton;
		LineEdit _ststusLineEdit;
		private void UpdateStatus(UserStatus rhubarbIcons) {
			if (_onlineButton is null ||
			_idleButton is null ||
			_doNotDisturbButton is null ||
			_streamButton is null ||
			_offlineButton is null) {
				return;
			}
			switch (rhubarbIcons) {
				case UserStatus.Online:
					_onlineButton.ButtonPressed.Value = true;
					_idleButton.ButtonPressed.Value = false;
					_doNotDisturbButton.ButtonPressed.Value = false;
					_streamButton.ButtonPressed.Value = false;
					_offlineButton.ButtonPressed.Value = false;
					break;
				case UserStatus.Idle:
					_onlineButton.ButtonPressed.Value = false;
					_idleButton.ButtonPressed.Value = true;
					_doNotDisturbButton.ButtonPressed.Value = false;
					_streamButton.ButtonPressed.Value = false;
					_offlineButton.ButtonPressed.Value = false;
					break;
				case UserStatus.DoNotDisturb:
					_onlineButton.ButtonPressed.Value = false;
					_idleButton.ButtonPressed.Value = false;
					_doNotDisturbButton.ButtonPressed.Value = true;
					_streamButton.ButtonPressed.Value = false;
					_offlineButton.ButtonPressed.Value = false;
					break;
				case UserStatus.Streaming:
					_onlineButton.ButtonPressed.Value = false;
					_idleButton.ButtonPressed.Value = false;
					_doNotDisturbButton.ButtonPressed.Value = false;
					_streamButton.ButtonPressed.Value = true;
					_offlineButton.ButtonPressed.Value = false;
					break;
				case UserStatus.Invisible:
					_onlineButton.ButtonPressed.Value = false;
					_idleButton.ButtonPressed.Value = false;
					_doNotDisturbButton.ButtonPressed.Value = false;
					_streamButton.ButtonPressed.Value = false;
					_offlineButton.ButtonPressed.Value = true;
					break;
				case UserStatus.Offline:
					_onlineButton.ButtonPressed.Value = false;
					_idleButton.ButtonPressed.Value = false;
					_doNotDisturbButton.ButtonPressed.Value = false;
					_streamButton.ButtonPressed.Value = false;
					_offlineButton.ButtonPressed.Value = true;
					break;
				default:
					break;
			}

		}

		[Exposed]
		public void Logout() {
			Task.Run(Engine.netApiManager.Client.LogOut);
		}

		[Exposed]
		public void ChangeProfile(IAssetProvider<RTexture2D> assetProvider) {
			if (assetProvider is null) {
				return;
			}
			if (!assetProvider.Loaded) {
				return;
			}
			if (assetProvider.Value is null) {
				return;
			}
			if (!Engine.netApiManager.Client.IsLogin) {
				return;
			}
			RLog.Info("Changing profile pic");
			var img = assetProvider.Value.Image;
			Task.Run(async () => {
				var data = new MemoryStream(img.SaveWebp(false, 1));
				RLog.Info("Uploading profile pic");
				var uploadedData = await Engine.netApiManager.Client.UploadRecord(data, "image/webp", true, true);
				if (uploadedData.Error) {
					RLog.Err("Failed to upload profile Error:" + uploadedData.MSG);
					return;
				}
				await Engine.netApiManager.Client.ChangeProfile(uploadedData.Data.RecordID);
				Engine.netApiManager.Client.User.ProfileIcon = uploadedData.Data.RecordID;
				UpdateProfilePic();
			});
		}


		private Button _usernameLabel;

		private void BuildStartMenu(UIElement start) {
			start.Min.Value = new Vector2f(0, 1);
			start.Max.Value = new Vector2f(0, 1);
			start.MinOffset.Value = new Vector2f(350, -544);
			start.MaxOffset.Value = new Vector2f(0, -100);
			start.Entity.AddChild("BackGround").AttachComponent<Panel>();


			var main = start.Entity.AddChild("Main").AttachComponent<BoxContainer>();
			main.Vertical.Value = true;
			main.Min.Value = Vector2f.Zero;
			main.Max.Value = Vector2f.Zero;
			main.MinOffset.Value = new Vector2f(97, 8);
			main.MaxOffset.Value = new Vector2f(343, 443);


			_startscroll = main.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			_startscroll.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_startscroll.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			_startListBox = _startscroll.Entity.AddChild("Start").AttachComponent<BoxContainer>();
			_startListBox.Vertical.Value = true;
			_startListBox.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_startListBox.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			_startListBoxAllPrograms = _startscroll.Entity.AddChild("StartAll").AttachComponent<BoxContainer>();
			_startListBoxAllPrograms.Vertical.Value = true;
			_startListBoxAllPrograms.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_startListBoxAllPrograms.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_startListBoxAllPrograms.Entity.enabled.Value = false;
			BuildAllPrograms();

			_allProgramsButton = main.Entity.AddChild("AllProgramsButton").AttachComponent<Button>();
			_allProgramsButton.MinSize.Value = new Vector2i(230, 35);
			_allProgramsButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_allProgramsButton.ToggleMode.Value = true;
			_allProgramsButton.Alignment.Value = RButtonAlignment.Center;
			var Locale = _allProgramsButton.Entity.AttachComponent<StandardLocale>();
			Locale.TargetValue.Target = _allProgramsButton.Text;
			Locale.Key.Value = "Start.AllPrograms";
			_allProgramsButton.ExpandIcon.Value = true;
			var AllProgramsButtonactionProvider = _allProgramsButton.Entity.AttachComponent<DelegateCall>();
			AllProgramsButtonactionProvider.action = () => {
				_startListBoxAllPrograms.Entity.enabled.Value = _startListBox.Entity.enabled.Value;
				_startListBox.Entity.enabled.Value = !_startListBoxAllPrograms.Entity.enabled.Value;
				Locale.Key.Value = _startListBoxAllPrograms.Entity.enabled.Value ? "Common.Back" : "Start.AllPrograms";
			};
			_allProgramsButton.Pressed.Target = AllProgramsButtonactionProvider.CallDelegate;




			_profileElement = main.Entity.AddChild("Profile").AttachComponent<UIElement>();
			_profileElement.Entity.enabled.Value = false;
			_profileElement.MinSize.Value = new Vector2i(0, 443);
			_profileElement.Entity.AddChild("BackGround").AttachComponent<Panel>();
			var profileBox = _profileElement.Entity.AddChild("Data").AttachComponent<BoxContainer>();
			profileBox.Vertical.Value = true;
			profileBox.Alignment.Value = RBoxContainerAlignment.Center;
			_usernameLabel = profileBox.Entity.AddChild("UserName").AttachComponent<Button>();
			_usernameLabel.Text.Value = "Username";

			var StatusButtons = profileBox.Entity.AddChild("StatusButtons").AttachComponent<BoxContainer>();
			StatusButtons.Alignment.Value = RBoxContainerAlignment.Center;
			StatusButtons.Vertical.Value = true;
			Button AddStatusButton(string buttonName, RTexture2D rTexture2D, Action clickAction = null) {
				var statusButton = StatusButtons.Entity.AddChild(buttonName).AttachComponent<Button>();
				statusButton.MinSize.Value = new Vector2i(32, 32);
				statusButton.ToggleMode.Value = true;
				statusButton.IconAlignment.Value = RButtonAlignment.Left;
				var Locale = statusButton.Entity.AttachComponent<StandardLocale>();
				Locale.TargetValue.Target = statusButton.Text;
				Locale.Key.Value = buttonName;
				statusButton.ExpandIcon.Value = true;
				var textureRef = StatusButtons.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
				var actionProvider = StatusButtons.Entity.AttachComponent<DelegateCall>();
				actionProvider.action = clickAction;
				statusButton.Icon.Target = textureRef;
				textureRef.LoadAsset(rTexture2D);
				if (clickAction != null) {
					statusButton.Pressed.Target = actionProvider.CallDelegate;
				}
				return statusButton;
			}



			void ChangeStatus(DataModel.Enums.UserStatus userStatus) {
				if (Engine.netApiManager.Client.Status is null) {
					return;
				}
				Engine.netApiManager.Client.Status.Status = userStatus;
				Task.Run(Engine.netApiManager.Client.UpdateStatus);
				UpdateStatus(userStatus);
			}
			_onlineButton = AddStatusButton("Status.Online", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.OnlineStatus), () => ChangeStatus(DataModel.Enums.UserStatus.Online));
			_idleButton = AddStatusButton("Status.Idle", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.IdleStatus), () => ChangeStatus(DataModel.Enums.UserStatus.Idle));
			_doNotDisturbButton = AddStatusButton("Status.DoNotDisturb", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.DoNotDistrubeStatus), () => ChangeStatus(DataModel.Enums.UserStatus.DoNotDisturb));
			_streamButton = AddStatusButton("Status.Stream", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.StreamingStatus), () => ChangeStatus(DataModel.Enums.UserStatus.Streaming));
			_offlineButton = AddStatusButton("Status.Offline", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.OfflineStatus), () => ChangeStatus(DataModel.Enums.UserStatus.Offline));

			_ststusLineEdit = profileBox.Entity.AddChild("StatusField").AttachComponent<LineEdit>();
			var ststusLineEditlocale = _ststusLineEdit.Entity.AttachComponent<StandardLocale>();
			ststusLineEditlocale.TargetValue.Target = _ststusLineEdit.PlaceholderText;
			ststusLineEditlocale.Key.Value = "Common.CustomStatus";
			_ststusLineEdit.HorizontalFilling.Value = RFilling.ShrinkCenter;
			_ststusLineEdit.MinSize.Value = new Vector2i(230, 35);
			_ststusLineEdit.TextSubmitted.Target = ChangeStatusText;

			var logoutButton = profileBox.Entity.AddChild("logoutButton").AttachComponent<Button>();
			var locale = logoutButton.Entity.AttachComponent<StandardLocale>();
			locale.TargetValue.Target = logoutButton.Text;
			logoutButton.Pressed.Target = Logout;
			locale.Key.Value = "Common.Logout";
			var local = logoutButton.Entity.AttachComponent<StandardLocale>();
			local.TargetValue.Target = logoutButton.ToolTipText;
			local.Key.Value = "Common.Logout";

			logoutButton.HorizontalFilling.Value = RFilling.ShrinkCenter;
			logoutButton.MinSize.Value = new Vector2i(230, 35);
			var logouttextureRef = logoutButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			logoutButton.Icon.Target = logouttextureRef;
			logouttextureRef.LoadAsset(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.LogOut));
			logoutButton.ExpandIcon.Value = true;

			var sideBar = start.Entity.AddChild("SideBar").AttachComponent<BoxContainer>();
			sideBar.Vertical.Value = true;
			sideBar.Alignment.Value = RBoxContainerAlignment.End;
			sideBar.Max.Value = new Vector2f(0, 1);
			sideBar.Min.Value = new Vector2f(0, 0);
			sideBar.MaxOffset.Value = new Vector2f(8, 0);
			sideBar.MinOffset.Value = new Vector2f(93, 0);
			sideBar.GrowVertical.Value = RGrowVertical.Both;

			Button AddSideButton(string buttonName, RTexture2D rTexture2D, Action clickAction = null) {
				var sideBarButton = sideBar.Entity.AddChild(buttonName).AttachComponent<Button>();
				sideBarButton.MinSize.Value = new Vector2i(0, 84);
				sideBarButton.IconAlignment.Value = RButtonAlignment.Center;
				sideBarButton.ExpandIcon.Value = true;
				var textureRef = sideBarButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
				var actionProvider = sideBar.Entity.AttachComponent<DelegateCall>();
				actionProvider.action = clickAction;
				sideBarButton.Icon.Target = textureRef;
				textureRef.LoadAsset(rTexture2D);
				if (clickAction != null) {
					sideBarButton.Pressed.Target = actionProvider.CallDelegate;
				}
				return sideBarButton;
			}

			_profileSideButton = AddSideButton("Profile", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.User), () => {
				_profileElement.Entity.enabled.Value = !_profileElement.Entity.enabled.Value;
				_allProgramsButton.Entity.enabled.Value = !_profileElement.Entity.enabled.Value;
				_startscroll.Entity.enabled.Value = !_profileElement.Entity.enabled.Value;
			});
			var acsepter = _profileSideButton.Entity.AttachComponent<ReferenceAccepter<IAssetProvider<RTexture2D>>>();
			acsepter.Dropped.Target = ChangeProfile;

			if (Engine.EngineLink.LiveVRChange) {
				var vrswitch = AddSideButton("VRSwitch", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.VRHeadset), () => {
					Engine.EngineLink.ChangeVR(!Engine.IsInVR);
					ToggleStart(false);
				});
				var vrswitchlocal = vrswitch.Entity.AttachComponent<StandardLocale>();
				vrswitchlocal.TargetValue.Target = vrswitch.ToolTipText;
				var action = void (bool state) => vrswitchlocal.Key.Value = state ? "Actions.ChangeVR.Disable" : "Actions.ChangeVR.Enable";
				Engine.EngineLink.VRChange += action;
				action(Engine.EngineLink.InVR);
			}
			else {
				sideBar.Entity.AddChild("VRSwitch").AttachComponent<UIElement>().MinSize.Value = new Vector2i(0, 84);
			}
			var files = AddSideButton("Files", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Folder), () => {
				ProgramManager.PrivateOpenProgram<FileExplorerProgram>();
				ToggleStart(false);
			});
			var fileslocal = files.Entity.AttachComponent<StandardLocale>();
			fileslocal.TargetValue.Target = files.ToolTipText;
			fileslocal.Key.Value = "Programs.FileExplorer.Name";
			var settings = AddSideButton("Settings", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Settings), () => {
				ProgramManager.OpenOnePrivateOpenProgram<SettingsProgram>();
				ToggleStart(false);
			});
			var settingslocal = settings.Entity.AttachComponent<StandardLocale>();
			settingslocal.TargetValue.Target = settings.ToolTipText;
			settingslocal.Key.Value = "Programs.Settings.Name";
			var exit = AddSideButton("Exit", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Shutdown), () => {
				Engine.Close();
				ToggleStart(false);
			});
			var exitlocal = exit.Entity.AttachComponent<StandardLocale>();
			exitlocal.TargetValue.Target = exit.ToolTipText;
			exitlocal.Key.Value = "Common.ExitRhubarbVR";
		}


		private void BuildLeftTaskBar(UIElement left) {
			var holder = left.Entity.AddChild("Holder").AttachComponent<BoxContainer>();
			holder.MinOffset.Value = new Vector2f(-8, 8);
			holder.MaxOffset.Value = new Vector2f(8, -8);
			_startButton = holder.Entity.AddChild("StartButton").AttachComponent<Button>();
			_startButton.IconAlignment.Value = RButtonAlignment.Center;
			_startButton.ToggleMode.Value = true;
			_startButton.Toggled.Target = ToggleStart;
			_startButton.ExpandIcon.Value = true;
			var local = _startButton.Entity.AttachComponent<StandardLocale>();
			local.TargetValue.Target = _startButton.ToolTipText;
			local.Key.Value = "ToolTips.StartMenu";

			var starticon = _startButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			starticon.Load(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.RhubarbVR));
			_startButton.Icon.Target = starticon;
			_startButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			var audioButton = holder.Entity.AddChild("AudioButton").AttachComponent<Button>();
			var audioicon = audioButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			audioicon.Load(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.AudioNormal));
			audioButton.Icon.Target = audioicon;
			audioButton.IconAlignment.Value = RButtonAlignment.Center;
			audioButton.ExpandIcon.Value = true;
			audioButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

		}

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public BoxContainer _taskbarElementHolder;

		private void BuildCenterTaskBar(UIElement center) {
			var backGorund = center.Entity.AddChild("Panel").AttachComponent<ColorRect>();
			backGorund.MinOffset.Value = new Vector2f(0, 8);
			backGorund.MaxOffset.Value = new Vector2f(0, -8);
			backGorund.Color.Value = new Colorf(50, 50, 50, 150);

			var scrollCont = center.Entity.AddChild("Scroll").AttachComponent<ScrollContainer>();
			_taskbarElementHolder = scrollCont.Entity.AddChild("Box").AttachComponent<BoxContainer>();
			_taskbarElementHolder.VerticalFilling.Value = RFilling.Fill | RFilling.Expand;
			_taskbarElementHolder.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

			var e = _taskbarElementHolder.Entity.AddChild("Spacer").AttachComponent<Panel>();
			e.Entity.orderOffset.Value = 500;
			e.MinSize.Value = new Vector2i(8, 38);
			e.VerticalFilling.Value = RFilling.ShrinkCenter;
		}

		private TextLabel _textLabel;
		private Button _startButton;
		private Button _profileSideButton;
		private UIElement _profileElement;
		private BoxContainer _startListBox;
		private Button _allProgramsButton;
		private ScrollContainer _startscroll;
		private BoxContainer _startListBoxAllPrograms;

		private void BuildRightTaskBar(UIElement right) {
			var holder = right.Entity.AddChild("Holder").AttachComponent<BoxContainer>();
			holder.MinOffset.Value = new Vector2f(-8, 8);
			holder.MaxOffset.Value = new Vector2f(8, -8);
			var notificationButton = holder.Entity.AddChild("NotificationButton").AttachComponent<Button>();
			var notificationicon = notificationButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			notificationicon.Load(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Bell));
			notificationButton.Icon.Target = notificationicon;
			_textLabel = holder.Entity.AddChild("Label").AttachComponent<TextLabel>();
			_textLabel.Text.Value = "40:54 AM\n90/13/2022\nFPS:2000";
			_textLabel.TextSize.Value = 16;
			notificationButton.IconAlignment.Value = RButtonAlignment.Center;
			notificationButton.ExpandIcon.Value = true;
			notificationButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			_textLabel.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

		}




		protected override void Step() {
			base.Step();
			if (InputManager.OpenDash.JustActivated()) {
				ToggleDash();
			}
			if (_textLabel is null) {
				return;
			}
			var date = DateTime.Now;
			var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			var sysFormatTime = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
			_textLabel.Text.Value = $"{date.ToString(sysFormatTime, CultureInfo.InvariantCulture)}\n{date.ToString(sysFormat, CultureInfo.InvariantCulture)}\nFPS:{1 / RTime.Elapsed:f0}";
		}
	}
}
