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
	public class TaskBar : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UICanvas uICanvas;
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

		public List<ITaskBarItem> taskBarItems = new();

		public void AddTaskBarItemToList(ITaskBarItem taskBarItem) {
			var remove = new List<ITaskBarItem>();
			for (var i = 0; i < taskBarItems.Count; i++) {
				if(taskBarItem.ProgramID == taskBarItems[i].ProgramID) {
					remove.Add(taskBarItems[i]);
				}
			}
			foreach (var item in remove) {
				taskBarItems.Remove(item);
			}
			taskBarItems.Add(taskBarItem);
			TaskBarItemsUpdate();
		}


		public List<Program> programs = new();

		public const float PADDING = 0f;

		public readonly Linker<string> TimeText;

		public Entity AddButton(Entity were, Vector2i iconindex, Action<ButtonEvent> action, float paddingoffset = 0, float yoffset = 0) {
			var child = were.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
			rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
			var img = child.AttachComponent<UIRectangle>();
			img.Tint.Value = new Colorf(0.1f, 0.1f, 0.1f, 0.5f);
			img.Material.Target = mit;
			img.AddRoundingSettings();
			var icon = child.AddChild("Icon");
			var iconrect = icon.AttachComponent<UIRect>();
			iconrect.AnchorMin.Value = new Vector2f(PADDING + paddingoffset, PADDING + paddingoffset + yoffset);
			iconrect.AnchorMax.Value = new Vector2f(1 - (PADDING + paddingoffset), 1 - (PADDING + paddingoffset) + yoffset);
			var spriterender = icon.AttachComponent<UISprite>();
			spriterender.Sprite.Target = sprite;
			spriterender.Material.Target = iconMit;
			spriterender.PosMin.Value = iconindex;
			spriterender.PosMax.Value = iconindex;
			if (action != null) {
				child.AttachComponent<UIButtonInteraction>().ButtonEvent.Target = action;
			}
			return child;
		}
		public Entity AddButton(Entity were, RTexture2D textue, Action<ButtonEvent> action, float paddingoffset = 0, float yoffset = 0) {
			var child = were.AddChild("childEliment");
			var rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
			rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
			var img = child.AttachComponent<UIRectangle>();
			img.Tint.Value = new Colorf(0.1f, 0.1f, 0.1f, 0.5f);
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
			mmit.transparency.Value = Transparency.Blend;
			mmit.SetPram("diffuse", assetProvider);
			spriterender.Material.Target = mmit;
			if (action != null) {
				child.AttachComponent<UIButtonInteraction>().ButtonEvent.Target = action;
			}
			return icon;
		}

		public void TaskBarItemsUpdate() {
			TaskBarItems.children.Clear();
			foreach (var item in taskBarItems) {
				AddTaskBarItem(item);
			}
		}


		public void AddTaskBarItem(ITaskBarItem taskBarItem) {
			var element = TaskBarItems.AddChild("listElementHolder");
			var rect = element.AttachComponent<UIRect>();
			rect.AnchorMin.Value = Vector2f.Zero;
			rect.AnchorMax.Value = new Vector2f(0.15f, 1);
			var delegatecall = element.AttachComponent<DelegateCall>();
			var buttonevent = element.AttachComponent<ButtonEventManager>();
			buttonevent.Click.Target = delegatecall.CallDelegate;
			delegatecall.action = taskBarItem.Clicked;
			var padding = 0.2f;
			var child = taskBarItem.Texture is null
				? AddButton(element, taskBarItem.Icon ?? new Vector2i(0, 3), buttonevent.Call, padding, padding)
				: AddButton(element, taskBarItem.Texture, buttonevent.Call, padding, padding);

			var text = child.AddChild("Text");
			var textrect = text.AttachComponent<UIRect>();
			textrect.AnchorMin.Value = new Vector2f(0f, 0f);
			textrect.AnchorMax.Value = new Vector2f(1f, 0.8f);
			textrect.Depth.Value += 0.02f;
			
			var uitext = text.AttachComponent<UIText>();
			uitext.Text.Value = $" <size5>{taskBarItem.Name}<size10> ";
			uitext.VerticalAlien.Value = EVerticalAlien.Top;
			uitext.HorizontalAlien.Value = EHorizontalAlien.Middle;
			if (taskBarItem.ShowOpenFlag) {
				var openoverlay = child.AddChild("IsOpenOverlay");
				var openoverlayrect = openoverlay.AttachComponent<UIRect>();
				openoverlayrect.Depth.Value += 0.01f;
				openoverlayrect.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
				openoverlayrect.AnchorMax.Value = new Vector2f(0.9f, 0.15f);
				var img = openoverlay.AttachComponent<UIRectangle>();
				img.Tint.Value = new Colorf(0.7f, 0.7f, 0.7f, 0.5f);
				img.Material.Target = mit;
			}

		}

		[Exsposed]
		public void OpenStart(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				RLog.Info("OpenStart");
			}
		}
		[Exsposed]
		public void OpenSoundOptions(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				RLog.Info("OpenSoundOptions");
			}
		}

		[Exsposed]
		public void OpenNotifications(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				RLog.Info("OpenNotifications");
			}
		}

		public override void OnAttach() {
			ProgramsHolder = World.RootEntity.AddChild("Programms");
			uICanvas = Entity.AttachComponent<UICanvas>();
			Engine.SettingsUpdate += Engine_SettingsUpdate;
			uICanvas.scale.Value = new Vector3f(16, 1.25f, 1);
			Engine_SettingsUpdate();
			var shader = World.RootEntity.GetFirstComponentOrAttach<UnlitClipShader>();
			mit = Entity.AttachComponent<DynamicMaterial>();
			mit.shader.Target = shader;
			iconMit = Entity.AttachComponent<DynamicMaterial>();
			iconMit.shader.Target = shader;
			var icons = World.RootEntity.GetFirstComponentOrAttach<IconsTex>();
			iconMit.SetPram("diffuse", icons);
			iconMit.transparency.Value = Transparency.Blend;
			sprite = Entity.AttachComponent<SpriteProvder>();
			sprite.Texture.Target = icons;
			sprite.GridSize.Value = new Vector2i(26,7);
			mit.transparency.Value = Transparency.Blend;

			//BackGround
			var rectTwo = Entity.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = Vector2f.Zero;
			rectTwo.AnchorMax.Value = Vector2f.One;
			var img = Entity.AttachComponent<UIRectangle>();
			img.Tint.Value = new Colorf(0,0,0,0.9f);
			img.Material.Target = mit;
			
			var leftSide = Entity.AddChild("leftSide");
			var leftSideList = leftSide.AttachComponent<HorizontalList>();
			leftSideList.AnchorMin.Value = Vector2f.Zero;
			leftSideList.AnchorMax.Value = new Vector2f(0.20f,1);
			leftSideList.Depth.Value = 0;
			leftSideList.Fit.Value = true;
			AddButton(leftSide, new Vector2i(16,0), OpenStart);
			AddButton(leftSide, new Vector2i(8,3), OpenSoundOptions);
		
			var listentitHolder = Entity.AddChild("listentitHolder");
			var listentitHolderrect = listentitHolder.AttachComponent<CuttingUIRect>();
			listentitHolderrect.AnchorMin.Value = new Vector2f(0.20f, 0.1f);
			listentitHolderrect.AnchorMax.Value = new Vector2f(0.8f, 0.9f);
			
			var listentit = listentitHolder.AddChild("list");
			var list = listentit.AttachComponent<HorizontalList>();
			var interaction = listentit.AttachComponent<UIScrollInteraction>();
			interaction.OnScroll.Target += list.Scroll;
			var img4 = listentit.AttachComponent<UIRectangle>();
			img4.Tint.Value = new Colorf(0.2f, 0.2f, 0.2f, 0.5f);
			img4.Material.Target = mit;
			list.Fit.Value = false;
			TaskBarItems = listentit;

			var RightSide = Entity.AddChild("RightSide");
			var RightSideList = RightSide.AttachComponent<UIRect>();
			RightSideList.AnchorMin.Value = new Vector2f(0.81f, 0.1f);
			RightSideList.AnchorMax.Value = new Vector2f(0.99f, 0.9f);

			var child = RightSide.AddChild("childEliment");
			rectTwo = child.AttachComponent<UIRect>();
			rectTwo.AnchorMin.Value = new Vector2f(0f);
			rectTwo.AnchorMax.Value = new Vector2f(0.4f, 1f);
			img = child.AttachComponent<UIRectangle>();
			img.Tint.Value = new Colorf(0.1f, 0.1f, 0.1f, 0.5f);
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
			spriterender.PosMin.Value = iconindex;
			spriterender.PosMax.Value = iconindex;
			child.AttachComponent<UIButtonInteraction>().ButtonEvent.Target = OpenNotifications;


			var child3 = RightSide.AddChild("childEliment");
			var rectTwo2 = child3.AttachComponent<UIRect>();
			rectTwo2.AnchorMin.Value = new Vector2f(0.5f, 0);
			rectTwo2.AnchorMax.Value = new Vector2f(0.9f,1f);
			child3 = child3.AddChild("childEliment");
			var rectTwom = child3.AttachComponent<UIRect>();
			var text = child3.AttachComponent<UIText>();
			TimeText.SetLinkerTarget(text.Text);
			text.StartingColor.Value = Colorf.White;

			AddTaskBarItemToList(new ProgramTaskBarItem(this, typeof(Login)));
		}

		public void OpenProgram(string name,Type programType) {
			RLog.Info($"Open program {name} Type: {programType.GetFormattedName()}");
			var programentity = ProgramsHolder.AddChild(name);
			var programcomp = programentity.AttachComponent<Program>(programType);
			programcomp.taskBar = this;
			programcomp.taskBarItem = new ProgramTaskBarItem(this, programcomp);
			programcomp.IntProgram();
			programs.Add(programcomp);
			AddTaskBarItemToList(programcomp.taskBarItem);
		}

		private void Engine_SettingsUpdate() {

			//Ui 
			uICanvas.FrontBindSegments.Value = Engine.MainSettings.UISettings.DashRoundingSteps;
			uICanvas.TopOffset.Value = Engine.MainSettings.UISettings.TopOffset != 0;
			uICanvas.TopOffsetValue.Value = Engine.MainSettings.UISettings.TopOffset;
			uICanvas.FrontBind.Value = Engine.MainSettings.UISettings.FrontBindAngle > 0;
			uICanvas.FrontBindAngle.Value = Engine.MainSettings.UISettings.FrontBindAngle;
			uICanvas.FrontBindRadus.Value = Engine.MainSettings.UISettings.FrontBindRadus;
			
			

			Entity.position.Value = new Vector3f(-0.7f, 0.1f, -0.1f);

			//uICanvas.TopOffset.Value = false;
			//uICanvas.FrontBind.Value = false;
			//Entity.position.Value = new Vector3f(-0.7f, 0.1f, -1f);

			Entity.rotation.Value = Quaternionf.CreateFromEuler(-22.5f, 0, 0);

			Entity.parent.Target.position.Value = new Vector3f(0, -0.4f - Engine.MainSettings.UISettings.DashOffsetDown, -(Engine.MainSettings.UISettings.FrontBindRadus/100) - Engine.MainSettings.UISettings.DashOffsetForward);

		}

		public override void Step() {

			//System Time
			var date = DateTime.Now;
			var sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
			var sysFormatTime = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
			var newTimeText = $"{date.ToString($"{sysFormatTime} \n {sysFormat}",CultureInfo.InvariantCulture)} ";
			if (TimeText.Linked) {
				if(TimeText.LinkedValue != newTimeText) {
					TimeText.LinkedValue = newTimeText;
				}
			}
		}

	}
}
