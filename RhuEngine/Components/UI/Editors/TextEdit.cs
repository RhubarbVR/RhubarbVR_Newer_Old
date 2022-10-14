using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Editors")]
	[UpdateLevel(UpdateEnum.Normal)]
	public class TextEdit : UIElement
	{
		public override string EditString => Text.Value;

		public readonly Sync<string> Text;
		public readonly Sync<string> PlaceHolderText;
		[Default(true)]
		public readonly Sync<bool> Editable;
		[Default(true)]
		public readonly Sync<bool> ContectMenuEnabled;
		[Default(true)]
		public readonly Sync<bool> ShortcutKeysEnabled;
		[Default(true)]
		public readonly Sync<bool> SelectingEnabled;
		[Default(true)]
		public readonly Sync<bool> DeselectingOnFocusLossEnabled;
		[Default(true)]
		public readonly Sync<bool> DragAndDropSelectionEnabled;
		[Default(true)]
		public readonly Sync<bool> VirtualKeyboardEnabled;
		[Default(true)]
		public readonly Sync<bool> MiddleMousePasteEnabled;
		public readonly Sync<bool> WrapText;
		public readonly Sync<bool> OverrideSelectFontColor;
		public readonly Sync<bool> HighlightAllOccurrences;
		public readonly Sync<bool> HighlightCurrentLine;
		public readonly Sync<bool> DrawControlChars;
		public readonly Sync<bool> DrawTabs;
		public readonly Sync<bool> DrawSpaces;
		public readonly Sync<bool> SmoothScroll;
		public readonly Sync<float> VScrollSpeed;
		public readonly Sync<bool> ScrollPashEndOfFile;
		public readonly Sync<int> VerticalScroll;
		public readonly Sync<int> HorizontalScroll;
		public readonly Sync<bool> FitContentHight;
		public readonly Sync<bool> MiniMap;
		[Default(80)]
		public readonly Sync<int> MiniMapWidth;
		public readonly Sync<bool> LineCaret;
		public readonly Sync<bool> CaretBlink;
		[Default(0.65f)]
		public readonly Sync<float> BlinkInterval;
		[Default(true)]
		public readonly Sync<bool> MoveOnRightClick;
		[Default(true)]
		public readonly Sync<bool> MidGrapheme;
		public readonly Sync<RTextDirection> TextDir;
		public readonly Sync<string> Language;

		[Default(true)]
		public readonly Sync<bool> FocusLossOnEnter;

		protected override void Step() {
			base.Step();
			if (Engine.KeyboardInteraction == this) {
				if (Engine.inputManager.KeyboardSystem.IsKeyJustDown(Key.Return) && !Engine.inputManager.KeyboardSystem.IsKeyDown(Key.Shift)) {
					KeyboardUnBind();
				}
			}
		}
		protected override void OnAttach() {
			base.OnAttach();
			FocusMode.Value = RFocusMode.All;
		}
	}
}
