using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;
using SixLabors.Fonts;

namespace RhuEngine.Components
{
	public class UIBuilder
	{
		public IAssetProvider<RMaterial> MainMat;

		public Stack<Entity> RectEntityStack = new();
		public Stack<UIRect> RectStack = new();

		public UIRect CurrentRect { get => RectStack.Peek(); set => RectStack.Push(value); }

		public Entity CurretRectEntity { get => RectEntityStack.Peek(); set => RectEntityStack.Push(value); }

		public void PopRect() {
			RectEntityStack.Pop();
			RectStack.Pop();
		}
		public Entity Root { get; }

		public bool AddLocal;

		public UIBuilder(Entity entity, IAssetProvider<RMaterial> mainMit, UIRect firstRect = null, bool addLocal = false,bool noFirstUIRect = false) {
			AddLocal = addLocal;
			Root = entity;
			CurretRectEntity = entity;
			if (!noFirstUIRect) {
				firstRect ??= CurretRectEntity.AttachComponent<UIRect>();
			}
			CurrentRect = firstRect;
			MainMat = mainMit;
		}

		public T AttachComponentToStack<T>() where T : Component, new() {
			return CurretRectEntity.AttachComponent<T>();
		}

		public UIRect PushRectNoDepth(Vector2f? min = null, Vector2f? max = null) {
			return PushRect(min, max, 0);
		}

		public UIRect PushRect(Vector2f? min = null, Vector2f? max = null, float? Depth = null) {
			return AttachChildRect<UIRect>(min, max, Depth);
		}

		public Entity AddChildEntity() {
			return CurretRectEntity.AddChild("ChildRect");
		}

		public T AttachChildRect<T>(Vector2f? min = null, Vector2f? max = null, float? Depth = null) where T : UIRect, new() {
			CurretRectEntity = CurretRectEntity.AddChild("ChildRect");
			var comp = AttachComponentToStack<T>();
			if (min is not null) {
				comp.AnchorMin.Value = min ?? Vector2f.Zero;
			}
			if (max is not null) {
				comp.AnchorMax.Value = max ?? Vector2f.One;
			}
			if (Depth is not null) {
				comp.Depth.Value = Depth ?? 0f;
			}
			CurrentRect = comp;
			return comp;
		}

		public UISprite AddSprit(Vector2i min, Vector2i max, IAssetProvider<RMaterial> asset, SpriteProvder psprite,float coloroffset = 1.9f,float alpha = 1f) {
			var sprite = AttachComponentToStack<UISprite>();
			var colorassign = AttachComponentToStack<UIColorAssign>();
			colorassign.Alpha.Value = alpha;
			colorassign.ColorShif.Value = coloroffset;
			colorassign.TargetColor.Target = sprite.Tint;
			sprite.PosMax.Value = max;
			sprite.PosMin.Value = min;
			sprite.Material.Target = asset;
			sprite.Sprite.Target = psprite;
			return sprite;
		}

		public (UIImage, UnlitMaterial) AddImg(IAssetProvider<RTexture2D> assetProvider) {
			var img = AttachComponentToStack<UIImage>();
			var imgmit = AttachComponentToStack<UnlitMaterial>();
			imgmit.DullSided.Value = true;
			img.Texture.Target = assetProvider;
			img.Material.Target = imgmit;
			imgmit.MainTexture.Target = assetProvider;
			return (img, imgmit);
		}

		public void SetAnchorMinMax(Vector2f? min = null, Vector2f? max = null) {
			if (min is not null) {
				CurrentRect.AnchorMin.Value = min ?? Vector2f.Zero;
			}
			if (max is not null) {
				CurrentRect.AnchorMax.Value = max ?? Vector2f.Zero;
			}
		}
		public void SetOffsetMinMax(Vector2f? min = null, Vector2f? max = null) {
			if (min is not null) {
				CurrentRect.OffsetMin.Value = min ?? Vector2f.Zero;
			}
			if (max is not null) {
				CurrentRect.OffsetMax.Value = max ?? Vector2f.Zero;
			}
		}
		public void SetLoaclOffsetMinMax(Vector2f? min = null, Vector2f? max = null) {
			if (min is not null) {
				CurrentRect.OffsetLocalMin.Value = min ?? Vector2f.Zero;
			}
			if (max is not null) {
				CurrentRect.OffsetLocalMax.Value = max ?? Vector2f.Zero;
			}
		}
		public StandardLocale AddTextWithLocal(string text, float coloroffset = 0, float alpha = 1, float? size = null, FontStyle? fontStyle = null) {
			var uitext = AttachComponentToStack<UIText>();
			var local = AttachComponentToStack<StandardLocale>();
			local.Key.Value = text;
			local.TargetValue.Target = uitext.Text;
			if (size is not null) {
				uitext.StatingSize.Value = size ?? 0;
			}
			var colorassign = AttachComponentToStack<UIColorAssign>();
			colorassign.Alpha.Value = alpha;
			colorassign.ColorShif.Value = coloroffset;
			colorassign.TargetColor.Target = uitext.StartingColor;
			if (fontStyle is not null) {
				uitext.StartingStyle.Value = fontStyle ?? FontStyle.Regular;
			}
			return local;
		}

		public CheckBox AddGenaricCheckBox(IAssetProvider<RMaterial> asset, SpriteProvder psprite, float coloroffset = 1.9f, float alpha = 0.9f, Vector2f? min = null, Vector2f? max = null) {
			return AddCheckBox(new Vector2i(2,1), new Vector2i(2,1), new Vector2i(1, 1), new Vector2i(1, 1),asset,psprite,coloroffset,alpha,min,max);
		}

		public CheckBox AddCheckBox(Vector2i openmin, Vector2i openmax, Vector2i closemin, Vector2i closemax, IAssetProvider<RMaterial> asset, SpriteProvder psprite, float coloroffset = 1.9f, float alpha = 0.9f, Vector2f? min = null, Vector2f? max = null) {
			AttachChildRect<UIRect>(min, max);
			var buttonInteraction = AttachComponentToStack<UIButtonInteraction>();
			AddRectangle((-((coloroffset/2)-0.5f)) + 0.5f,alpha);
			PushRect();
			var sprite = AddSprit(openmin, openmax, asset, psprite, coloroffset, alpha);
			var checkbox = AttachComponentToStack<CheckBox>();
			checkbox.MaxClose.Value = closemax;
			checkbox.MinClose.Value = closemin;
			checkbox.MaxOpen.Value = openmax;
			checkbox.MinOpen.Value = openmin;
			buttonInteraction.ButtonEvent.Target = checkbox.Click;
			checkbox.Minicon.Target = sprite.PosMin;
			checkbox.Maxicon.Target = sprite.PosMax;
			PopRect();
			PopRect();
			return checkbox;
		}

		public UIText AddText(string text, float? size = null, float coloroffset = 0, float alpha = 1, FontStyle? fontStyle = null, bool removeLocal = false) {
			var uitext = AttachComponentToStack<UIText>();
			var colorassing = AttachComponentToStack<UIColorAssign>();
			colorassing.Alpha.Value = alpha;
			colorassing.ColorShif.Value = coloroffset;
			colorassing.TargetColor.Target = uitext.StartingColor;
			uitext.Text.Value = text;
			if (AddLocal && !removeLocal) {
				var local = AttachComponentToStack<StandardLocale>();
				local.Key.Value = text;
				local.TargetValue.Target = uitext.Text;
			}
			if (size is not null) {
				uitext.StatingSize.Value = size ?? 0;
			}
			if (fontStyle is not null) {
				uitext.StartingStyle.Value = fontStyle ?? FontStyle.Regular;
			}
			return uitext;
		}

		public void AddRectangle(float coloroffset = 0, float alpha = 1, bool? fullbox = null) {
			var rectangle = AttachComponentToStack<UIRectangle>();
			rectangle.Material.Target = MainMat;
			rectangle.AddRoundingSettings();
			var colorassing = AttachComponentToStack<UIColorAssign>();
			colorassing.ColorShif.Value = coloroffset;
			colorassing.Alpha.Value = alpha;
			colorassing.TargetColor.Target = rectangle.Tint;
			if (fullbox is not null) {
				rectangle.FullBox.Value = fullbox ?? false;
			}
		}
		/// <summary>
		/// Add button to click
		/// </summary>
		/// <param name="onClick">Needs to be a method with the <see cref="ExposedAttribute"/></param>
		/// <param name="onPressing">Needs to be a method with the <see cref="ExposedAttribute"/></param>
		/// <param name="onReleases">Needs to be a method with the <see cref="ExposedAttribute"/></param>
		/// <param name="autoPop">Auto reset the stack after the button</param>
		public (UIButtonInteraction, ButtonEventManager) AddButtonEvent(Action onClick = null, Action onPressing = null, Action onReleases = null, bool autoPop = true, float coloroffset = 0, float alpha = 1, bool? fullbox = null, Vector2f? min = null, Vector2f? max = null) {
			AttachChildRect<UIRect>(min, max);
			AddRectangle(coloroffset,alpha, fullbox);
			var buttonInteraction = AttachComponentToStack<UIButtonInteraction>();
			var enevents = AttachComponentToStack<ButtonEventManager>();
			buttonInteraction.ButtonEvent.Target = enevents.Call;
			if (onClick is not null) {
				enevents.Click.Target = onClick;
			}
			if (onPressing is not null) {
				enevents.Pressing.Target = onPressing;
			}
			if (onReleases is not null) {
				enevents.Releases.Target = onReleases;
			}
			if (autoPop) {
				PopRect();
			}
			return (buttonInteraction, enevents);
		}

		public UIButtonInteraction AddButton(bool autoPop = true, float coloroffset = 0, float alpha = 1, bool? fullbox = null, Vector2f? min = null, Vector2f? max = null) {
			AttachChildRect<UIRect>(min, max);
			AddRectangle(coloroffset, alpha, fullbox);
			var buttonInteraction = AttachComponentToStack<UIButtonInteraction>();
			if (autoPop) {
				PopRect();
			}
			return buttonInteraction;
		}

		public (UIText, UITextEditorInteraction, UITextCurrsor, Sync<string>) AddTextEditor(string emptytext = "This Is Input", float coloroffset = 0, float alpha = 1, string defaultString = "",float padding=0.1f, float? size = null, float textcoloroffset = 0, float textalpha = 1,bool autopop = true, FontStyle? fontStyle = null, Vector2f? min = null, Vector2f? max = null) {
			var button = AddButtonEvent(null, null, null, false, coloroffset,alpha, null, min, max);
			PushRect(new Vector2f(0), new Vector2f(1), 0.05f);
			SetOffsetMinMax(new Vector2f(padding), new Vector2f(-padding));
			var uitext = AddText(defaultString, size, textcoloroffset,textalpha, fontStyle, true);
			uitext.EmptyString.Value = emptytext;
			if (AddLocal) {
				var local = AttachComponentToStack<StandardLocale>();
				local.Key.Value = emptytext;
				local.TargetValue.Target = uitext.EmptyString;
			}
			var editor = AttachComponentToStack<UITextEditorInteraction>();
			editor.Value.Target = uitext.Text;
			button.Item2.Click.Target = editor.EditingClick;
			var currsor = AttachComponentToStack<UITextCurrsor>();
			currsor.TextCurrsor.Target = editor;
			currsor.TextComp.Target = uitext;
			currsor.Material.Target = MainMat;
			if (autopop) {
				PopRect();
				PopRect();
			}
			return (uitext, editor, currsor, uitext.Text);
		}

		public (UIText,UIButtonInteraction, ButtonEventManager) AddButtonEventLabled(string text, float? size = null, float textcoloroffset = 2, float textalpha = 1,  Action onClick = null, Action onPressing = null, Action onReleases = null, bool autoPop = true, float coloroffset = 0, float alpha = 1, bool? fullbox = null, Vector2f? min = null, Vector2f? max = null) {
			var buttonEvent = AddButtonEvent(onClick, onPressing, onReleases, false, coloroffset, alpha, fullbox, min, max);
			var uitext = AddText(text, size, textcoloroffset, textalpha);
			if (autoPop) {
				PopRect();
			}
			return (uitext, buttonEvent.Item1, buttonEvent.Item2);
		}
	}
}
