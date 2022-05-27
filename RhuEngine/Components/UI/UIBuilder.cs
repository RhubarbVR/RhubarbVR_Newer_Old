using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Collections.Generic;
using System;

namespace RhuEngine.Components
{
	public class UIBuilder
	{
		public AssetProvider<RMaterial> MainMit;

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

		public UIBuilder(Entity entity, AssetProvider<RMaterial> mainMit, UIRect firstRect = null, bool addLocal = false) {
			AddLocal = addLocal;
			Root = entity;
			CurretRectEntity = entity;
			if (firstRect is null) {
				firstRect = CurretRectEntity.AttachComponent<UIRect>();
			}
			CurrentRect = firstRect;
			MainMit = mainMit;
		}

		public T AttachComponentToStack<T>() where T : Component, new() {
			return CurretRectEntity.AttachComponent<T>();
		}

		public UIRect PushRect(Vector2f? min = null, Vector2f? max = null, float? Depth = null) {
			return AttachChildRect<UIRect>(min, max, Depth);
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

		public void AddSprit(Vector2i min, Vector2i max, IAssetProvider<RMaterial> asset, SpriteProvder psprite) {
			var sprite = AttachComponentToStack<UISprite>();
			sprite.PosMax.Value = max;
			sprite.PosMin.Value = min;
			sprite.Material.Target = asset;
			sprite.Sprite.Target = psprite;
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
		public StandardLocale AddTextWithLocal(string text, float? size = null, Colorf? color = null, FontStyle? fontStyle = null) {
			var uitext = AttachComponentToStack<UIText>();
			var local = AttachComponentToStack<StandardLocale>();
			local.Key.Value = text;
			local.TargetValue.Target = uitext.Text;
			if (size is not null) {
				uitext.StatingSize.Value = size ?? 0;
			}
			if (color is not null) {
				uitext.StartingColor.Value = color ?? Colorf.White;
			}
			if (fontStyle is not null) {
				uitext.StartingStyle.Value = fontStyle ?? FontStyle.Regular;
			}
			return local;
		}

		public UIText AddText(string text, float? size = null, Colorf? color = null, FontStyle? fontStyle = null, bool removeLocal = false) {
			var uitext = AttachComponentToStack<UIText>();

			uitext.Text.Value = text;
			if (AddLocal && !removeLocal) {
				var local = AttachComponentToStack<StandardLocale>();
				local.Key.Value = text;
				local.TargetValue.Target = uitext.Text;
			}
			if (size is not null) {
				uitext.StatingSize.Value = size ?? 0;
			}
			if (color is not null) {
				uitext.StartingColor.Value = color ?? Colorf.White;
			}
			if (fontStyle is not null) {
				uitext.StartingStyle.Value = fontStyle ?? FontStyle.Regular;
			}
			return uitext;
		}

		public void AddRectangle(Colorf? color = null, bool? fullbox = null) {
			var rectangle = AttachComponentToStack<UIRectangle>();
			rectangle.Material.Target = MainMit;
			if (color is not null) {
				rectangle.Tint.Value = color ?? Colorf.White;
			}
			if (fullbox is not null) {
				rectangle.FullBox.Value = fullbox ?? false;
			}
		}
		/// <summary>
		/// Add button to click
		/// </summary>
		/// <param name="onClick">Needs to be a method with the <see cref="ExsposedAttribute"/></param>
		/// <param name="onPressing">Needs to be a method with the <see cref="ExsposedAttribute"/></param>
		/// <param name="onReleases">Needs to be a method with the <see cref="ExsposedAttribute"/></param>
		/// <param name="autoPop">Auto reset the stack after the button</param>
		public (UIButtonInteraction, ButtonEventManager) AddButtonEvent(Action onClick = null, Action onPressing = null, Action onReleases = null, bool autoPop = true, Colorf? color = null, bool? fullbox = null, Vector2f? min = null, Vector2f? max = null) {
			AttachChildRect<UIRect>(min, max);
			AddRectangle(color, fullbox);
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

		public UIButtonInteraction AddButton(bool autoPop = true, Colorf? color = null, bool? fullbox = null, Vector2f? min = null, Vector2f? max = null) {
			AttachChildRect<UIRect>(min, max);
			AddRectangle(color, fullbox);
			var buttonInteraction = AttachComponentToStack<UIButtonInteraction>();
			if (autoPop) {
				PopRect();
			}
			return buttonInteraction;
		}

		public (UIText, UITextEditorInteraction, UITextCurrsor, Sync<string>) AddTextEditor(string emptytext = "This Is Input", Colorf? buttoncolor = null, string defaultString = "", float? size = null, Colorf? color = null, FontStyle? fontStyle = null, Vector2f? min = null, Vector2f? max = null) {
			var button = AddButtonEvent(null, null, null, false, buttoncolor, null, min, max);
			var uitext = AddText(defaultString, size, color, fontStyle, true);
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
			currsor.Material.Target = MainMit;
			PopRect();
			return (uitext, editor, currsor, uitext.Text);
		}
	}
}
