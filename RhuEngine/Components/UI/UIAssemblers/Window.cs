using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI/UIAssemblers" })]
	public sealed class Window : Component
	{
		[OnChanged(nameof(ChangeHeader))]
		[Default(1f)]
		public readonly Sync<float> HeaderHeight;

		[OnChanged(nameof(ProssCollapsed))]
		public readonly Sync<bool> IsCollapsed;
		[OnChanged(nameof(ProssPin))]
		public readonly Sync<bool> IsPin;
		[Default(true)]
		[OnChanged(nameof(UpdateButtons))]
		public readonly Sync<bool> PinButton;
		[Default(true)]
		[OnChanged(nameof(UpdateButtons))]
		public readonly Sync<bool> MinimizeButton;
		[Default(true)]
		[OnChanged(nameof(UpdateButtons))]
		public readonly Sync<bool> CloseButton;
		[Default(true)]
		[OnChanged(nameof(UpdateButtons))]
		public readonly Sync<bool> CollapseButton;

		public readonly Linker<Vector2i> CollapseButtonIconOne;
		public readonly Linker<Vector2i> CollapseButtonIconTwo;

		public readonly Linker<bool> CollapseUIEnable;

		[OnChanged(nameof(NameChanged))]
		public readonly Sync<string> NameValue;
		public readonly Linker<string> NameLink;
		private void NameChanged() {
			if (NameLink.Linked) {
				NameLink.LinkedValue = NameValue.Value;
			}
		}

		public readonly SyncRef<SpriteProvder> IconSprite; 

		public readonly Linker<bool> MinimizeButtonEnable;

		public readonly Linker<bool> CloseButtonEnable;

		public readonly Linker<bool> CollapseButtonEnable;

		public readonly Linker<bool> PinButtonEnable;

		public readonly Linker<Vector2i> PinButtonIconOne;
		public readonly Linker<Vector2i> PinButtonIconTwo;

		public readonly Linker<Vector2f> WindowRootRectOffsetMin;
		public readonly Linker<Vector2f> HeaderRectOffsetMax;

		public readonly Linker<Vector2f> MainUIMax;

		public readonly SyncRef<Entity> PannelRoot;

		public readonly SyncRef<UICanvas> Canvas;

		public readonly SyncDelegate OnClose;

		public readonly SyncDelegate OnCollapse;

		public readonly SyncDelegate OnPin;

		public readonly SyncDelegate OnMinimize;

		public readonly SyncDelegate<Action<bool>> PinChanged;

		public readonly SyncRef<UnlitMaterial> IconMit;
		public readonly SyncRef<UnlitMaterial> MainMit;

		private void ChangeHeader() {
			if (WindowRootRectOffsetMin.Linked) {
				WindowRootRectOffsetMin.LinkedValue = new Vector2f(0, HeaderHeight.Value);
			}
			if (HeaderRectOffsetMax.Linked) {
				HeaderRectOffsetMax.LinkedValue = new Vector2f(0, HeaderHeight.Value);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			var icons = World.RootEntity.GetFirstComponentOrAttach<IconsTex>();
			var sprite = IconSprite.Target = World.RootEntity.GetFirstComponentOrAttach<SpriteProvder>();
			sprite.Texture.Target = icons;
			sprite.GridSize.Value = new Vector2i(26, 7);
			IconMit.Target = Entity.AttachComponent<UnlitMaterial>();
			IconMit.Target.DullSided.Value = true;
			IconMit.Target.Transparency.Value = Transparency.Blend;
			IconMit.Target.MainTexture.Target = icons;
			var mit = MainMit.Target = Entity.AttachComponent<UnlitMaterial>();
			mit.DullSided.Value = true;
			mit.Transparency.Value = Transparency.Blend;
			(Entity, UISprite) AddButton(Entity were, float PADDING, Vector2i iconindex, Action<ButtonEvent> action) {
				var child = were.AddChild("childEliment");
				var rectTwo = child.AttachComponent<UIRect>();
				rectTwo.AnchorMin.Value = new Vector2f(0.1f, 0.1f);
				rectTwo.AnchorMax.Value = new Vector2f(0.9f, 0.9f);
				var img = child.AttachComponent<UIRectangle>();
				var colorassign = child.AttachComponent<UIColorAssign>();
				colorassign.Alpha.Value = 0.5f;
				colorassign.ColorShif.Value = 0.2f;
				colorassign.TargetColor.Target = img.Tint;
				img.Material.Target = mit;
				img.AddRoundingSettings();
				var icon = child.AddChild("Icon");
				var iconrect = icon.AttachComponent<UIRect>();
				iconrect.AnchorMin.Value = new Vector2f(PADDING, PADDING);
				iconrect.AnchorMax.Value = new Vector2f(1 - PADDING, 1 - PADDING);
				var spriterender = icon.AttachComponent<UISprite>();
				spriterender.Sprite.Target = sprite;
				spriterender.Material.Target = IconMit.Target;
				spriterender.PosMin.Value = iconindex;
				spriterender.PosMax.Value = iconindex;
				colorassign = child.AttachComponent<UIColorAssign>();
				colorassign.ColorShif.Value = 1.9f;
				colorassign.TargetColor.Target = spriterender.Tint;
				if (action != null) {
					child.AttachComponent<UIButtonInteraction>().ButtonEvent.Target = action;
				}
				return (child,spriterender);
			}

			Canvas.Target = Entity.AttachComponent<UICanvas>();
			Canvas.Target.scale.Value = new Vector3f(10, 8, 1.5f);
			var rect = Entity.AttachComponent<UIRect>();
			var grabber = Entity.AttachComponent<UIGrabInteraction>();
			var grabable = Entity.AttachComponent<Grabbable>();
			grabber.Grabeded.Target = grabable.RemoteGrab;
			rect.OffsetMax.Value = new Vector2f(0, HeaderHeight.Value);
			MainUIMax.Target = rect.AnchorMax;
			var windowRoot = Entity.AddChild("WindowRoot");
			var windowRootrect = windowRoot.AttachComponent<UIRect>();
			windowRootrect.OffsetMin.Value = new Vector2f(0, HeaderHeight.Value);
			WindowRootRectOffsetMin.Target = windowRootrect.OffsetMin;
			var header = Entity.AddChild("Header");
			var headerrect = header.AttachComponent<UIRect>();
			headerrect.AnchorMax.Value = new Vector2f(1f, 0f);
			headerrect.OffsetMax.Value = new Vector2f(0, HeaderHeight.Value);
			HeaderRectOffsetMax.Target = headerrect.OffsetMax;
			var headerTextSide = header.AddChild("HeaderTextSide");
			var headerTextSiderect = headerTextSide.AttachComponent<UIRect>();
			headerTextSiderect.OffsetMax.Value -= new Vector2f(HeaderHeight.Value * 4, 0);
			headerTextSide = headerTextSide.AddChild("HeaderTextSide");
			var rectoffset = headerTextSide.AttachComponent<UIRect>();
			rectoffset.AnchorMax.Value = new Vector2f(0.9f);
			rectoffset.AnchorMin.Value = new Vector2f(0.1f);
			var text = headerTextSide.AttachComponent<UIText>();
			NameLink.Target = text.Text;
			text.VerticalAlien.Value = EVerticalAlien.Bottom;
			var colorassign = Entity.AttachComponent<UIColorAssign>();
			colorassign.ColorShif.Value = 1.9f;
			colorassign.TargetColor.Target = text.StartingColor;
			var headerButtonSide = header.AddChild("ButtonGroup");
			var headerButtonSiderect = headerButtonSide.AttachComponent<UIRect>();
			headerButtonSiderect.OffsetMin.Value -= new Vector2f(HeaderHeight.Value * 4, 1);
			headerButtonSiderect.AnchorMin.Value = Vector2f.One;
			var headerButtonSideh = headerButtonSide.AddChild("ButtonGroup");
			var headerButtonSiderecth = headerButtonSideh.AttachComponent<HorizontalList>();
			headerButtonSiderecth.Fit.Value = true;
			var min = AddButton(headerButtonSideh, 0f, new Vector2i(18, 0), MinimizeAction);
			MinimizeButtonEnable.Target = min.Item1.enabled;
			var calps = AddButton(headerButtonSideh, 0f, new Vector2i(0, 1), CollapseAction);
			CollapseButtonEnable.Target = calps.Item1.enabled;
			CollapseButtonIconTwo.Target = calps.Item2.PosMin;
			CollapseButtonIconOne.Target = calps.Item2.PosMax;
			calps = AddButton(headerButtonSideh, 0f, new Vector2i(5, 6), PinAction);
			PinButtonEnable.Target = calps.Item1.enabled;
			PinButtonIconOne.Target = calps.Item2.PosMin;
			PinButtonIconTwo.Target = calps.Item2.PosMax;
			var close = AddButton(headerButtonSideh, 0f, new Vector2i(20, 0), CloseAction);
			CloseButtonEnable.Target = close.Item1.enabled;
			OnCollapse.Target = CollapseUI;
			OnPin.Target = PinUI;
			UpdateButtons();



			PannelRoot.Target = windowRoot.AddChild("PannelRoot");
			CollapseUIEnable.Target = PannelRoot.Target.enabled;

			var img = Entity.AttachComponent<UIRectangle>();
			colorassign = Entity.AttachComponent<UIColorAssign>();
			colorassign.Alpha.Value = 0.7f;
			colorassign.TargetColor.Target = img.Tint;
			img.Material.Target = mit;
		}
		private void UpdateButtons() {
			if (CloseButtonEnable.Linked) {
				CloseButtonEnable.LinkedValue = CloseButton.Value;
			}
			if (CollapseButtonEnable.Linked) {
				CollapseButtonEnable.LinkedValue = CollapseButton.Value;
			}
			if (MinimizeButtonEnable.Linked) {
				MinimizeButtonEnable.LinkedValue = MinimizeButton.Value;
			}
		}
		private void ProssCollapsed() {
			if (IsCollapsed) {
				if (CollapseButtonIconOne.Linked) {
					CollapseButtonIconOne.LinkedValue = new Vector2i(19, 0);
				}
				if (CollapseButtonIconTwo.Linked) {
					CollapseButtonIconTwo.LinkedValue = new Vector2i(19, 0);
				}
			}
			else {
				if (CollapseButtonIconOne.Linked) {
					CollapseButtonIconOne.LinkedValue = new Vector2i(0, 1);
				}
				if (CollapseButtonIconTwo.Linked) {
					CollapseButtonIconTwo.LinkedValue = new Vector2i(0, 1);
				}
			}
			if (MainUIMax.Linked) {
				MainUIMax.LinkedValue = IsCollapsed.Value ? new Vector2f(1,0): Vector2f.One;
			}
			if (CollapseUIEnable.Linked) {
				CollapseUIEnable.LinkedValue = !IsCollapsed;
			}
		}

		private void ProssPin() {
			if (IsPin) {
				if (PinButtonIconOne.Linked) {
					PinButtonIconOne.LinkedValue = new Vector2i(6, 6);
				}
				if (PinButtonIconTwo.Linked) {
					PinButtonIconTwo.LinkedValue = new Vector2i(6, 6);
				}
			}
			else {
				if (PinButtonIconOne.Linked) {
					PinButtonIconOne.LinkedValue = new Vector2i(5, 6);
				}
				if (PinButtonIconTwo.Linked) {
					PinButtonIconTwo.LinkedValue = new Vector2i(5, 6);
				}
			}
		}


		[Exposed]
		public void CollapseUI() {
			IsCollapsed.Value = !IsCollapsed.Value;
		}
		[Exposed]
		public void PinUI() {
			IsPin.Value = !IsPin.Value;
			PinChanged.Target?.Invoke(IsPin.Value);
		}
		[Exposed]
		public void CollapseAction(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				OnCollapse.Target?.Invoke();
			}
		}

		[Exposed]
		public void PinAction(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				OnPin.Target?.Invoke();
			}
		}


		[Exposed]
		public void CloseAction(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				OnClose.Target?.Invoke();
			}
		}

		[Exposed]
		public void MinimizeAction(ButtonEvent buttonEvent) {
			if (buttonEvent.IsClicked) {
				OnMinimize.Target?.Invoke();
			}
		}
	}
}
