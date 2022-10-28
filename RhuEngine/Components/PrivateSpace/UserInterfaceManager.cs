﻿using System;
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
		public PrivateSpaceManager PrivateSpaceManager;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement UserInterface;

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
		public bool OpenCloseDash
		{
			get => UserInterface.Enabled.Value;
			set {
				UserInterface.Enabled.Value = value;
				VrElements.enabled.Value = value && Engine.IsInVR;
				PrivateSpaceManager.VRViewPort.UpdateMode.Value = value && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
				if (value) {
					InputManager.screenInput.FreeMouse();
				}
				else {
					InputManager.screenInput.UnFreeMouse();
				}
#if DEBUG
				if (UserInterface.Enabled.Value) {
					RLog.Info("Opened Dash");
				}
				else {
					RLog.Info("Closed Dash");
				}
#endif
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
			StartVRElement.Entity.enabled.Value = startState;
			Start.Entity.enabled.Value = startState;
		}

		private void EngineLink_VRChange(bool obj) {
			UserInterface.Entity.parent.Target = Engine.IsInVR ? PrivateSpaceManager.VRViewPort.Entity : PrivateSpaceManager.RootScreenElement.Entity;
			PrivateSpaceManager.VRViewPort.UpdateMode.Value = OpenCloseDash && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
			VrElements.enabled.Value = OpenCloseDash && Engine.IsInVR;
			PrivateSpaceManager.VRViewPort.Enabled.Value = Engine.IsInVR;
		}

		public Entity RootUIEntity => UserInterface.Entity;

		internal void LoadInterface() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			UImaterial = Entity.AttachComponent<UnlitMaterial>();
			UImaterial.DullSided.Value = true;
			UImaterial.Transparency.Value = Transparency.Blend;
			UImaterial.MainTexture.Target = PrivateSpaceManager.VRViewPort;
			VrElements = Entity.AddChild("VrElements");
			var taskBarVRElement = VrElements.AddChild("TaskBarVR").AttachMesh<CanvasMesh>(UImaterial);
			taskBarVRElement.Resolution.Value = PrivateSpaceManager.VRViewPort.Size.Value;
			taskBarVRElement.Max.Value = new Vector2f(1, 0);
			taskBarVRElement.MaxOffset.Value = new Vector2i(0, 100);
			taskBarVRElement.InputInterface.Target = PrivateSpaceManager.VRViewPort;

			StartVRElement = VrElements.AddChild("StartVR").AttachMesh<CanvasMesh>(UImaterial);
			StartVRElement.TopOffset.Value = false;
			StartVRElement.FrontBindRadus.Value += 1f;
			StartVRElement.Scale.Value += new Vector3f(1, 0, 0);
			StartVRElement.Min.Value = new Vector2f(0, 0);
			StartVRElement.Max.Value = new Vector2f(0, 0);
			StartVRElement.MaxOffset.Value = new Vector2i(350, 544);
			StartVRElement.MinOffset.Value = new Vector2i(0, 100);

			StartVRElement.Resolution.Value = PrivateSpaceManager.VRViewPort.Size.Value;
			StartVRElement.InputInterface.Target = PrivateSpaceManager.VRViewPort;

			VrElements.enabled.Value = false;
			Engine.EngineLink.VRChange += EngineLink_VRChange;

			var TaskBar = RootUIEntity.AddChild("TaskBar").AttachComponent<UIElement>();
			TaskBar.Min.Value = new Vector2f(0, 1);
			TaskBar.MinOffset.Value = new Vector2f(0, -100);

			TaskBar.Entity.AddChild("Panel").AttachComponent<Panel>();
			var taskBardata = TaskBar.Entity.AddChild("BoxContainer").AttachComponent<BoxContainer>();

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
			ToggleStart(false);
		}

		private void BuildStartMenu(UIElement start) {
			start.Min.Value = new Vector2f(0, 1);
			start.Max.Value = new Vector2f(0, 1);
			start.MinOffset.Value = new Vector2f(350, -544);
			start.MaxOffset.Value = new Vector2f(0, -100);
			start.Entity.AddChild("BackGround").AttachComponent<Panel>();

			var sideBar = start.Entity.AddChild("SideBar").AttachComponent<BoxContainer>();
			sideBar.Vertical.Value = true;
			sideBar.Alignment.Value = RBoxContainerAlignment.End;
			sideBar.Max.Value = new Vector2f(0, 1);
			sideBar.Min.Value = new Vector2f(0, 0);
			sideBar.MaxOffset.Value = new Vector2f(8, 0);
			sideBar.MinOffset.Value = new Vector2f(93, 0);
			sideBar.GrowVertical.Value = RGrowVertical.Both;

			void AddSideButton(string buttonName, RTexture2D rTexture2D, Action clickAction = null) {
				var sideBarButton = sideBar.Entity.AddChild(buttonName).AttachComponent<Button>();
				sideBarButton.MinSize.Value = new Vector2i(0, 84);
				sideBarButton.IconAlignment.Value = RButtonAlignment.Center;
				sideBarButton.ExpandIcon.Value = true;
				var textureRef = sideBar.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
				var actionProvider = sideBar.Entity.AttachComponent<DelegateCall>();
				actionProvider.action = clickAction;
				sideBarButton.Icon.Target = textureRef;
				textureRef.LoadAsset(rTexture2D);
				if (clickAction != null) {
					sideBarButton.Pressed.Target = actionProvider.CallDelegate;
				}
			}

			AddSideButton("Profile", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.User), () => { 
				//Show Profile Settings
			});
			if (Engine.EngineLink.LiveVRChange) {
				AddSideButton("VRSwitch", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.VRHeadset), () => Engine.EngineLink.ChangeVR(!Engine.IsInVR));
			}
			else {
				sideBar.Entity.AddChild("VRSwitch").AttachComponent<UIElement>().MinSize.Value = new Vector2i(0, 84);
			}
			AddSideButton("Files", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Folder), () => { });
			AddSideButton("Settings", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Settings), () => { });
			AddSideButton("Exit", Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Shutdown), () => Engine.Close());
		}


		private void BuildLeftTaskBar(UIElement left) {
			var holder = left.Entity.AddChild("Holder").AttachComponent<BoxContainer>();
			holder.MinOffset.Value = new Vector2f(-8, 8);
			holder.MaxOffset.Value = new Vector2f(8, -8);
			var startButton = holder.Entity.AddChild("StartButton").AttachComponent<Button>();
			startButton.IconAlignment.Value = RButtonAlignment.Center;
			startButton.ToggleMode.Value = true;
			startButton.Toggled.Target = ToggleStart;
			startButton.ExpandIcon.Value = true;
			var starticon = startButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			starticon.Load(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.RhubarbVR));
			startButton.Icon.Target = starticon;
			startButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;
			var audioButton = holder.Entity.AddChild("AudioButton").AttachComponent<Button>();
			var audioicon = audioButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			audioicon.Load(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.AudioNormal));
			audioButton.Icon.Target = audioicon;
			audioButton.IconAlignment.Value = RButtonAlignment.Center;
			audioButton.ExpandIcon.Value = true;
			audioButton.HorizontalFilling.Value = RFilling.Fill | RFilling.Expand;

		}
		private void BuildCenterTaskBar(UIElement center) {
			var backGorund = center.Entity.AddChild("Panel").AttachComponent<ColorRect>();
			backGorund.MinOffset.Value = new Vector2f(0, 8);
			backGorund.MaxOffset.Value = new Vector2f(0, -8);
			backGorund.Color.Value = new Colorf(50, 50, 50, 150);

		}
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		private TextLabel _textLabel;

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
			_textLabel.Text.Value = $"{date.ToString(sysFormatTime, CultureInfo.InvariantCulture)}\n{date.ToString(sysFormat, CultureInfo.InvariantCulture)}\nFPS:{1 / RTime.Elapsedf:f0}";
		}
	}
}
