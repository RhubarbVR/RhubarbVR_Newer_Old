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
using System.Reflection;

namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public class TaskBar : Component
	{
		public enum OpenPart
		{
			None,
			Start,
			Audio,
			Notifications
		}

		private OpenPart CurrentState { get; set; } = OpenPart.None;

		private OpenPart _part = OpenPart.None;

		public OpenPart OpenLevel
		{
			get => _part;
			set {
				_part = value;
				CurrentState = CurrentState switch {
					OpenPart.None => value,
					_ => OpenPart.None,
				};
			}
		}
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UICanvas uICanvas;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UICanvas startCanvas;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public DynamicMaterial iconMit;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public DynamicMaterial mit;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public SpriteProvder sprite;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity ProgramsHolder;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity TaskBarItems;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public ScrollUIRect scrollRect;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public ScrollUIRect scrollRectTwo;
		public List<ITaskBarItem> taskBarItems = new();

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity StartEntity;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity AudioEntiy;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity NotificationEntiy;

		public bool _open = true;

		public float _openTarget;

		public OpenPart LastState = OpenPart.None;

		public bool Open
		{
			get => _open;
			set {
				_open = value;
				_openTarget = value ? 0f : 1f;
				if (!_open) {
					LastState = OpenLevel;
					OpenLevel = OpenPart.None;
				}
				else {
					OpenLevel = LastState;
				}
			}
		}

		public void AddTaskBarItemToList(ITaskBarItem taskBarItem) {
			var remove = new List<ITaskBarItem>();
			for (var i = 0; i < taskBarItems.Count; i++) {
				if (taskBarItem.ID == taskBarItems[i].ID) {
					remove.Add(taskBarItems[i]);
				}
			}
			foreach (var item in remove) {
				taskBarItems.Remove(item);
			}
			taskBarItems.Add(taskBarItem);
			RegTaskBarItemsUpdate();
		}

		public void RemoveTaskBarItemToList(ITaskBarItem taskBarItem) {
			taskBarItems.Remove(taskBarItem);
			RegTaskBarItemsUpdate();
		}

		public List<Program> programs = new();

		public const float PADDING = 0f;

		public readonly Linker<string> TimeText;

		public (Entity, UIRect) AddButton(Entity were, Vector2i iconindex, Action<ButtonEvent> action, float paddingoffset = 0, float yoffset = 0) {
			var child = were.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
			rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
			var img = child.AttachComponent<UIRectangle>();
			var colorassign = child.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.5f;
			colorassign.ColorShif.Value = 0.5f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
			img.AddRoundingSettings();
			var icon = child.AddChild("Icon");
			var iconrect = icon.AttachComponent<UIRect>();
			iconrect.AnchorMin.Value = new Vector2f(PADDING + paddingoffset, PADDING + paddingoffset + yoffset);
			iconrect.AnchorMax.Value = new Vector2f(1 - (PADDING + paddingoffset), 1 - (PADDING + paddingoffset) + yoffset);
			var spriterender = icon.AttachComponent<UISprite>();
			if (!(iconindex == new Vector2i(16, 0) || iconindex == new Vector2i(7, 6))) {
				colorassign = NotificationEntiy.AttachComponent<UIColorAssign>();
				colorassign.ColorShif.Value = 1.9f;
				colorassign.TargetColor.Target = spriterender.Tint;
			}
			spriterender.Sprite.Target = sprite;
			spriterender.Material.Target = iconMit;
			spriterender.PosMin.Value = iconindex;
			spriterender.PosMax.Value = iconindex;
			if (action != null) {
				var buttoninter = child.AttachComponent<UIButtonInteraction>();
				buttoninter.ButtonEvent.Target = action;
				buttoninter.AllowOtherZones.Value = false;
			}
			return (child, iconrect);
		}
		public (Entity, UIRect) AddButton(Entity were, RTexture2D textue, Action<ButtonEvent> action, float paddingoffset = 0, float yoffset = 0) {
			var child = were.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
			rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
			var img = child.AttachComponent<UIRectangle>();
			var colorassign = child.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.5f;
			colorassign.ColorShif.Value = 0.5f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
			img.AddRoundingSettings();
			var icon = child.AddChild("Icon");
			var iconrect = icon.AttachComponent<UIRect>();
			iconrect.AnchorMin.Value = new Vector2f(PADDING + paddingoffset, PADDING + paddingoffset + yoffset);
			iconrect.AnchorMax.Value = new Vector2f(1 - (PADDING + paddingoffset), 1 - (PADDING + paddingoffset) + yoffset);
			var spriterender = icon.AttachComponent<UIImage>();
			var assetProvider = child.AttachComponent<RawAssetProvider<RTexture2D>>();
			assetProvider.LoadAsset(textue);
			var mmit = child.AttachComponent<DynamicMaterial>();
			mmit.shader.Target = World.RootEntity.GetFirstComponentOrAttach<UnlitClipShader>();
			mmit.Transparency = Transparency.Blend;
			mit.MainTexture = assetProvider;
			spriterender.Material.Target = mmit;
			if (action != null) {
				child.AttachComponent<UIButtonInteraction>().ButtonEvent.Target = action;
			}
			return (icon, iconrect);
		}

		public void RegTaskBarItemsUpdate() {
			RWorld.ExecuteOnEndOfFrame(this, TaskBarItemsUpdate);
		}

		public void TaskBarItemsUpdate() {
			TaskBarItems.children.Clear();
			foreach (var item in Engine.worldManager.worlds) {
				if (item.Focus is World.FocusLevel.Background or World.FocusLevel.Focused) {
					AddTaskBarItem(new WorldTaskBarItem(item), "");
				}
			}
			foreach (var item in taskBarItems) {
				AddTaskBarItem(item, item.ExtraText);
			}
		}


		public void AddTaskBarItem(ITaskBarItem taskBarItem, string appenedText) {
			var element = TaskBarItems.AddChild("listElementHolder");
			var rect = element.AttachComponent<UIRect>();
			rect.AnchorMin.Value = Vector2f.Zero;
			rect.AnchorMax.Value = new Vector2f(0.15f, 1);
			var delegatecall = element.AttachComponent<DelegateCall>();
			var buttonevent = element.AttachComponent<ButtonEventManager>();
			buttonevent.Click.Target = delegatecall.CallDelegate;
			delegatecall.action = taskBarItem.Clicked;
			var padding = 0.2f;
			var (child, taskbarrect) = taskBarItem.Texture is null
				? AddButton(element, taskBarItem.Icon ?? new Vector2i(7, 5), buttonevent.Call, padding, padding)
				: AddButton(element, taskBarItem.Texture, buttonevent.Call, padding, padding);
			if (taskBarItem.CanCloses) {
				var eventComp = child.parent.Target.AttachComponent<UIHoverInteraction>();
				var onhover = element.AttachComponent<DelegateCall>();
				var unHover = element.AttachComponent<DelegateCall>();
				var closeeButtpon = element.AttachComponent<DelegateCall>();
				var LastClick = DateTime.UtcNow;
				closeeButtpon.action = () => {
					if((DateTime.UtcNow - LastClick).TotalSeconds < 0.5f) {
						taskBarItem.Close();
					}
					LastClick = DateTime.UtcNow;
				};
				var closeeButtponEvent = element.AttachComponent<ButtonEventManager>();
				closeeButtponEvent.Click.Target = closeeButtpon.CallDelegate;
				eventComp.OnHover.Target += onhover.CallDelegate;
				eventComp.OnUnHover.Target += unHover.CallDelegate;
				var closebutton = child.parent.Target.AddChild("CloseButton");
				var closeRect = closebutton.AttachComponent<UIRect>();
				closeRect.AnchorMax.Value = new Vector2f(1f);
				closeRect.AnchorMin.Value = new Vector2f(0.65f);
				closebutton.enabled.Value = false;
				AddButton(closebutton, new Vector2i(20,0), closeeButtponEvent.Call);
				onhover.action = () => {
					closebutton.enabled.Value = true;
				};
				unHover.action = () => {
					closebutton.enabled.Value = false;
				};
			}
			var text = child.AddChild("Text");
			var textrect = text.AttachComponent<UIRect>();
			textrect.AnchorMin.Value = new Vector2f(0.1f, 0f);
			textrect.AnchorMax.Value = new Vector2f(0.9f, 0.5f);
			textrect.Depth.Value += 0.02f;

			var uitext = text.AttachComponent<UIText>();
			uitext.Text.Value = taskBarItem.Name;
			var colorassign = text.AttachComponent<UIColorAssign>();
			colorassign.ColorShif.Value = 1.9f;
			colorassign.TargetColor.Target = uitext.StartingColor;

			if (taskBarItem.LocalName) {
				var local = text.AttachComponent<StandardLocale>();
				local.Append.Value = appenedText;
				local.TargetValue.Target = uitext.Text;
				local.Key.Value = taskBarItem.Name;
			}
			uitext.VerticalAlien.Value = EVerticalAlien.Center;
			uitext.HorizontalAlien.Value = EHorizontalAlien.Middle;
			if (taskBarItem.ShowOpenFlag) {
				var openoverlay = child.AddChild("IsOpenOverlay");
				var openoverlayrect = openoverlay.AttachComponent<UIRect>();
				openoverlayrect.Depth.Value += 0.01f;
				openoverlayrect.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
				openoverlayrect.AnchorMax.Value = new Vector2f(0.9f, 0.15f);
				var img = openoverlay.AttachComponent<UIRectangle>();
				colorassign = openoverlay.AttachComponent<UIColorAssign>();
				colorassign.Alpha.Value = 0.5f;
				colorassign.ColorShif.Value = 0.7f;
				colorassign.TargetColor.Target = img.Tint;
				img.Material.Target = mit;
			}
		}

		[Exposed]
		public void OpenStart(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				RLog.Info("OpenStart");
				OpenLevel = OpenLevel != OpenPart.Start ? OpenPart.Start : OpenPart.None;
			}
		}
		[Exposed]
		public void OpenSoundOptions(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				RLog.Info("OpenSoundOptions");
				OpenLevel = OpenLevel != OpenPart.Audio ? OpenPart.Audio : OpenPart.None;
			}
		}

		[Exposed]
		public void OpenNotifications(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				RLog.Info("OpenNotifications");
				OpenLevel = OpenLevel != OpenPart.Notifications ? OpenPart.Notifications : OpenPart.None;
			}
		}

		private void LoadStart(Entity mainentity) {
			StartEntity = mainentity.AddChild("Start");
			var startrect = StartEntity.AttachComponent<UIRect>();
			startrect.AnchorMax.Value = new Vector2f(0.3f, 1f);
			var min = StartEntity.AttachComponent<StartMenu>();
			min.BuildStart(this, startrect, mit, iconMit, sprite);
			var img = StartEntity.AttachComponent<UIRectangle>();
			var colorassign = StartEntity.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.9f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
			img.FullBox.Value = true;
		}

		private void LoadAudio(Entity mainentity) {
			AudioEntiy = mainentity.AddChild("Audio");
			var startrect = AudioEntiy.AttachComponent<UIRect>();
			startrect.AnchorMax.Value = new Vector2f(0.4f, 0.45f);
			startrect.AnchorMin.Value = new Vector2f(0.1f, 0f);
			var img = AudioEntiy.AttachComponent<UIRectangle>();
			var colorassign = AudioEntiy.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.9f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
			img.FullBox.Value = true;
		}
		private void LoadNotification(Entity mainentity) {
			NotificationEntiy = mainentity.AddChild("Notify");
			var startrect = NotificationEntiy.AttachComponent<UIRect>();
			startrect.AnchorMin.Value = new Vector2f(0.7f, 0f);

			var img = NotificationEntiy.AttachComponent<UIRectangle>();
			var colorassign = NotificationEntiy.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.9f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
			img.FullBox.Value = true;
		}
		public void AddStartAndNotification() {
			startCanvas = Entity.AddChild("TopOver").AttachComponent<UICanvas>();
			startCanvas.scale.Value = new Vector3f(16, 6.5f, 1);
			var rectTwo = startCanvas.Entity.AttachComponent<CuttingUIRect>();
			rectTwo.AnchorMin.Value = Vector2f.Zero;
			rectTwo.AnchorMax.Value = Vector2f.One;

			var mainentity = startCanvas.Entity.AddChild("scroll");
			scrollRectTwo = mainentity.AttachComponent<ScrollUIRect>();
			LoadAudio(mainentity);
			LoadStart(mainentity);
			LoadNotification(mainentity);
		}


		public override void OnAttach() {
			ProgramsHolder = World.RootEntity.AddChild("Programms");
			uICanvas = Entity.AddChild("Canvas").AttachComponent<UICanvas>();
			Engine.SettingsUpdate += Engine_SettingsUpdate;
			uICanvas.scale.Value = new Vector3f(16, 1.25f, 1);
			var shader = World.RootEntity.GetFirstComponentOrAttach<UnlitClipShader>();
			mit = Entity.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
			mit.Transparency = Transparency.Blend;
			iconMit = Entity.AttachComponent<DynamicMaterial>();
			iconMit.shader.Target = shader;
			var icons = World.RootEntity.GetFirstComponentOrAttach<IconsTex>();
			iconMit.MainTexture = icons;
			iconMit.Transparency = Transparency.Blend;
			sprite = Entity.AttachComponent<SpriteProvder>();
			sprite.Texture.Target = icons;
			sprite.GridSize.Value = new Vector2i(26, 7);
			mit.Transparency = Transparency.Blend;
			AddStartAndNotification();
			Engine_SettingsUpdate();

			//BackGround
			var rectTwo = uICanvas.Entity.AttachComponent<CuttingUIRect>();
			rectTwo.AnchorMin.Value = Vector2f.Zero;
			rectTwo.AnchorMax.Value = Vector2f.One;

			var mainentity = uICanvas.Entity.AddChild("scroll");
			scrollRect = mainentity.AttachComponent<ScrollUIRect>();
			mainentity = mainentity.AddChild("scroll");
			mainentity.AttachComponent<UIRect>();
			var img = mainentity.AttachComponent<UIRectangle>();
			var colorassign = mainentity.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.9f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;

			var leftSide = mainentity.AddChild("leftSide");
			var leftSideList = leftSide.AttachComponent<HorizontalList>();
			leftSideList.AnchorMin.Value = Vector2f.Zero;
			leftSideList.AnchorMax.Value = new Vector2f(0.20f, 1);
			leftSideList.Depth.Value = 0;
			leftSideList.Fit.Value = true;
#if DEBUG
			AddButton(leftSide, new Vector2i(7, 6), OpenStart);
#else
			AddButton(leftSide, new Vector2i(16, 0), OpenStart);
#endif
			AddButton(leftSide, new Vector2i(8, 3), OpenSoundOptions);
			var listentitHolder = mainentity.AddChild("listentitHolder");
			var listentitHolderrect = listentitHolder.AttachComponent<CuttingUIRect>();
			listentitHolderrect.AnchorMin.Value = new Vector2f(0.20f, 0.1f);
			listentitHolderrect.AnchorMax.Value = new Vector2f(0.8f, 0.9f);

			var listentit = listentitHolder.AddChild("list");
			var list = listentit.AttachComponent<HorizontalList>();
			var interaction = listentit.AttachComponent<UIScrollInteraction>();
			interaction.AllowOtherZones.Value = true;
			interaction.OnScroll.Target += list.Scroll;
			var img4 = listentit.AttachComponent<UIRectangle>();
			var colorassign2 = listentit.AttachComponent<UIColorAssign>();
			colorassign2.Alpha.Value = 0.5f;
			colorassign2.ColorShif.Value = 0.3f;
			colorassign2.TargetColor.Target = img4.Tint;
			img4.Material.Target = mit;
			list.Fit.Value = false;
			TaskBarItems = listentit;

			var RightSide = mainentity.AddChild("RightSide");
			var RightSideList = RightSide.AttachComponent<UIRect>();
			RightSideList.AnchorMin.Value = new Vector2f(0.81f, 0.1f);
			RightSideList.AnchorMax.Value = new Vector2f(0.99f, 0.9f);

			var child = RightSide.AddChild("childEliment");
			var rectTwo2 = child.AttachComponent<UIRect>();
			rectTwo2.AnchorMin.Value = new Vector2f(0f);
			rectTwo2.AnchorMax.Value = new Vector2f(0.4f, 1f);
			img = child.AttachComponent<UIRectangle>();
			colorassign = child.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.5f;
			colorassign.ColorShif.Value = 0.3f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
			img.AddRoundingSettings();
			var icon = child.AddChild("Icon");
			var iconrect = icon.AttachComponent<UIRect>();
			iconrect.AnchorMin.Value = new Vector2f(PADDING, PADDING);
			iconrect.AnchorMax.Value = new Vector2f(1 - PADDING, 1 - PADDING);
			var spriterender = icon.AttachComponent<UISprite>();
			spriterender.Sprite.Target = sprite;
			spriterender.Material.Target = iconMit;
			var iconindex = new Vector2i(2, 0);
			colorassign = NotificationEntiy.AttachComponent<UIColorAssign>();
			colorassign.ColorShif.Value = 1.9f;
			colorassign.TargetColor.Target = spriterender.Tint;
			spriterender.PosMin.Value = iconindex;
			spriterender.PosMax.Value = iconindex;
			child.AttachComponent<UIButtonInteraction>().ButtonEvent.Target = OpenNotifications;


			var child3 = RightSide.AddChild("childEliment");
			rectTwo2 = child3.AttachComponent<UIRect>();
			rectTwo2.AnchorMin.Value = new Vector2f(0.5f, 0);
			var rectTwom = child3.AttachComponent<UIRect>();
			var text = child3.AttachComponent<UIText>();
			TimeText.SetLinkerTarget(text.Text);
			colorassign = NotificationEntiy.AttachComponent<UIColorAssign>();
			colorassign.ColorShif.Value = 1.9f;
			colorassign.TargetColor.Target = text.StartingColor;
			if (!Engine.netApiManager.IsLoggedIn) {
				AddTaskBarItemToList(new ProgramTaskBarItem(this, typeof(LoginProgram)));
			}
			WorldManager.OnWorldUpdateTaskBar += RegTaskBarItemsUpdate;
		}
		public Program HasProgramOpen(Type ProGramType) {
			foreach (var item in programs) {
				if (item.GetType() == ProGramType) {
					return item;
				}
			}
			return null;
		}

		public T HasProgramOpen<T>() where T : Program {
			foreach (var item in programs) {
				if (item.GetType() == typeof(T)) {
					return (T)item;
				}
			}
			return null;
		}

		public T OpenProgram<T>(bool forceOpen = false) where T : Program {
			forceOpen |= typeof(T).GetCustomAttribute<OpenManyAttribute>() != null;
			if (!forceOpen) {
				var lastProgram = HasProgramOpen<T>();
				if (lastProgram is not null) {
					return lastProgram;
				}
			}
			return (T)OpenProgramForced(typeof(T).GetFormattedName(), typeof(T));
		}

		public Program OpenProgram(Type ProgramType, bool forceOpen = false) {
			forceOpen |= ProgramType.GetCustomAttribute<OpenManyAttribute>() != null;
			if (!forceOpen) {
				var lastProgram = HasProgramOpen(ProgramType);
				if (lastProgram is not null) {
					return lastProgram;
				}
			}
			return OpenProgramForced(ProgramType.GetFormattedName(), ProgramType);
		}

		public int HowManyProgramsOpen(Type programType) {
			var count = 0;
			foreach (var item in programs) {
				if (item.GetType() == programType) {
					count++;
				}
			}
			return count;
		}

		public Program OpenProgramForced(string name, Type programType) {
			RLog.Info($"Open program {name} Type: {programType.GetFormattedName()}");
			var programentity = ProgramsHolder.AddChild(name);
			var programcomp = programentity.AttachComponent<Program>(programType);
			programcomp.taskBar = this;
			programcomp.taskBarItem = new ProgramTaskBarItem(this, programcomp);
			programcomp.IntProgram();
			var amount = HowManyProgramsOpen(programType);
			programcomp.taskBarItem.ID += $".{amount}";
			if (amount != 0) {
				programcomp.taskBarItem.ExtraText = amount.ToString();
			}
			programs.Add(programcomp);
			AddTaskBarItemToList(programcomp.taskBarItem);
			return programcomp;
		}

		public void ProgramClose(Program program) {
			programs.Remove(program);
			RemoveTaskBarItemToList(program.taskBarItem);
		}

		private void Engine_SettingsUpdate() {

			//Ui 
			uICanvas.FrontBindSegments.Value = Engine.MainSettings.UISettings.DashRoundingSteps;
			uICanvas.TopOffset.Value = Engine.MainSettings.UISettings.TopOffset != 0;
			uICanvas.TopOffsetValue.Value = Engine.MainSettings.UISettings.TopOffset;
			uICanvas.FrontBind.Value = Engine.MainSettings.UISettings.FrontBindAngle > 0;
			uICanvas.FrontBindAngle.Value = Engine.MainSettings.UISettings.FrontBindAngle;
			uICanvas.FrontBindRadus.Value = Engine.MainSettings.UISettings.FrontBindRadus;

			startCanvas.FrontBindSegments.Value = Engine.MainSettings.UISettings.DashRoundingSteps;
			startCanvas.FrontBind.Value = Engine.MainSettings.UISettings.FrontBindAngle + Engine.MainSettings.UISettings.TopOffset > 0;
			startCanvas.FrontBindAngle.Value = Engine.MainSettings.UISettings.FrontBindAngle;
			startCanvas.FrontBindRadus.Value = Engine.MainSettings.UISettings.FrontBindRadus + Engine.MainSettings.UISettings.TopOffset;
			Entity.position.Value = new Vector3f(-0.7f, 0.1f, -0.1f);
			startCanvas.Entity.position.Value = new Vector3f((-Engine.MainSettings.UISettings.TopOffset) / 10, (uICanvas.scale.Value.y / 10) + 0.01f, 0);
			//uICanvas.TopOffset.Value = false;
			//uICanvas.FrontBind.Value = false;
			//Entity.position.Value = new Vector3f(-0.7f, 0.1f, -1f);

			Entity.rotation.Value = Quaternionf.CreateFromEuler(-22.5f, 0, 0);

			Entity.parent.Target.position.Value = new Vector3f(0, -0.4f - Engine.MainSettings.UISettings.DashOffsetDown, -(Engine.MainSettings.UISettings.FrontBindRadus / 100) - Engine.MainSettings.UISettings.DashOffsetForward);

		}
		private float _newvaluetwo = 0;

		private float _newvalue = 0;
		public override void Step() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (RInput.Key(Key.Ctrl).IsActive() && RInput.Key(Key.Space).IsJustActive()) {
				Open = !Open;
			}
			_newvalue = MathUtil.Lerp(_newvalue, _openTarget, RTime.Elapsedf * 5);
			if (_newvalue is > 0.01f and < 0.95f) {
				if (!uICanvas.Entity.enabled.Value) {
					uICanvas.Entity.enabled.Value = true;
				}
				scrollRect.ScrollPos.Value = new Vector2f(0, _newvalue);
			}
			else if (_newvalue > 0.95f) {
				if (scrollRect.ScrollPos.Value.x != 1) {
					scrollRect.ScrollPos.Value = new Vector2f(0, 1);
				}
				if (uICanvas.Entity.enabled.Value) {
					uICanvas.Entity.enabled.Value = false;
				}
			}
			else if (_newvalue <= 0.01f && scrollRect.ScrollPos.Value.x != 0) {
				scrollRect.ScrollPos.Value = new Vector2f(0);
			}
			_newvaluetwo = MathUtil.Lerp(_newvaluetwo, (CurrentState != OpenPart.None) ? 0f : 1f, RTime.Elapsedf * ((CurrentState != _part) ? 10f : 5f));
			if (_newvaluetwo <= 0.01f && scrollRectTwo.ScrollPos.Value.x != 0) {
				scrollRectTwo.ScrollPos.Value = new Vector2f(0);
			}
			if (_newvaluetwo >= 0.97f && scrollRectTwo.ScrollPos.Value.x != -1) {
				scrollRectTwo.ScrollPos.Value = new Vector2f(0, -1);
			}
			if (_newvaluetwo is > 0.01f and < 0.97f) {
				if (!startCanvas.Entity.enabled.Value) {
					StartEntity.enabled.Value = false;
					NotificationEntiy.enabled.Value = false;
					AudioEntiy.enabled.Value = false;
					switch (CurrentState) {
						case OpenPart.Start:
							StartEntity.enabled.Value = true;
							break;
						case OpenPart.Audio:
							AudioEntiy.enabled.Value = true;
							break;
						case OpenPart.Notifications:
							NotificationEntiy.enabled.Value = true;
							break;
						default:
							break;
					}
					startCanvas.Entity.enabled.Value = true;
				}
				scrollRectTwo.ScrollPos.Value = new Vector2f(0, -_newvaluetwo);
			}
			else if (_newvaluetwo > 0.97f) {
				if (CurrentState == OpenPart.None && _part != CurrentState) {
					CurrentState = _part;
				}
				if (startCanvas.Entity.enabled.Value) {
					startCanvas.Entity.enabled.Value = false;
				}
			}
			//MathUtil.Lerp()
			//System Time
			var date = DateTime.Now;
			var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			var sysFormatTime = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
			var newTimeText = $"<size13>{date.ToString(sysFormatTime, CultureInfo.InvariantCulture)} \n<size10>{date.ToString(sysFormat, CultureInfo.InvariantCulture)}";
			if (TimeText.Linked) {
				if (TimeText.LinkedValue != newTimeText) {
					TimeText.LinkedValue = newTimeText;
				}
			}
		}

	}
}
