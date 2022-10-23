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

		private void EngineLink_VRChange(bool obj) {
			UserInterface.Entity.parent.Target = Engine.IsInVR ? PrivateSpaceManager.VRViewPort.Entity : PrivateSpaceManager.RootScreenElement.Entity;
			PrivateSpaceManager.VRViewPort.UpdateMode.Value = OpenCloseDash && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
			VrElements.enabled.Value = OpenCloseDash && Engine.IsInVR;
			PrivateSpaceManager.VRViewPort.Enabled.Value = Engine.IsInVR;
		}

		public Entity RootUIEntity => UserInterface.Entity;

		internal void LoadInterface() {
			UImaterial = Entity.AttachComponent<UnlitMaterial>();
			UImaterial.DullSided.Value = true;
			UImaterial.Transparency.Value = Transparency.Blend;
			UImaterial.MainTexture.Target = PrivateSpaceManager.VRViewPort;
			VrElements = Entity.AddChild("VrElements");
			var mest = VrElements.AddChild("Stuff").AttachMesh<TrivialBox3Mesh>(UImaterial);
			mest.Entity.scale.Value = Vector3f.One / 3;
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
			UserInterface.Enabled.Value = false;
			EngineLink_VRChange(Engine.IsInVR);
		}

		private void BuildLeftTaskBar(UIElement left) {
			var holder = left.Entity.AddChild("Holder").AttachComponent<BoxContainer>();
			holder.MinOffset.Value = new Vector2f(8, 8);
			holder.MaxOffset.Value = new Vector2f(-8, -8);
			var startButton = holder.Entity.AddChild("StartButton").AttachComponent<Button>();
			startButton.IconAlignment.Value = RButtonAlignment.Center;
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
			holder.MinOffset.Value = new Vector2f(8, 8);
			holder.MaxOffset.Value = new Vector2f(-8, -8);
			var notificationButton = holder.Entity.AddChild("NotificationButton").AttachComponent<Button>();
			var notificationicon = notificationButton.Entity.AttachComponent<RawAssetProvider<RTexture2D>>();
			notificationicon.Load(Engine.staticResources.IconSheet.GetElement(RhubarbAtlasSheet.RhubarbIcons.Bell));
			notificationButton.Icon.Target = notificationicon;
			_textLabel = holder.Entity.AddChild("Label").AttachComponent<TextLabel>();
			_textLabel.Text.Value = "40:54 AM\r\n90/13/2022\r\nFPS:2000";
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
			if(_textLabel is null) {
				return;
			}
			var date = DateTime.Now;
			var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			var sysFormatTime = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
			_textLabel.Text.Value = $"{date.ToString(sysFormatTime, CultureInfo.InvariantCulture)}\r\n{date.ToString(sysFormat, CultureInfo.InvariantCulture)}\r\nFPS:{1 / RTime.Elapsedf:f0}";
		}
	}
}
