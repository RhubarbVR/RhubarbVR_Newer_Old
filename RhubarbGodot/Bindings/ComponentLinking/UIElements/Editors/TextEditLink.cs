using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using Godot;
using RhuEngine.Components;
using static Godot.Control;
using static System.Net.Mime.MediaTypeNames;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class TextEditBase<T, T2> : UIElementLinkBase<T, T2> where T : RhuEngine.Components.TextEdit, new() where T2 : Godot.TextEdit, new()
	{
		public override void Init() {
			base.Init();

			LinkedComp.Text.Changed += Text_Changed;
			LinkedComp.PlaceHolderText.Changed += PlaceHolderText_Changed;
			LinkedComp.Editable.Changed += Editable_Changed;
			LinkedComp.ContectMenuEnabled.Changed += ContectMenuEnabled_Changed;
			LinkedComp.ShortcutKeysEnabled.Changed += ShortcutKeysEnabled_Changed;
			LinkedComp.SelectingEnabled.Changed += SelectingEnabled_Changed;
			LinkedComp.DeselectingOnFocusLossEnabled.Changed += DeselectingOnFocusLossEnabled_Changed;
			LinkedComp.DragAndDropSelectionEnabled.Changed += DragAndDropSelectionEnabled_Changed;
			LinkedComp.VirtualKeyboardEnabled.Changed += VirtualKeyboardEnabled_Changed;
			LinkedComp.MiddleMousePasteEnabled.Changed += MiddleMousePasteEnabled_Changed;
			LinkedComp.WrapText.Changed += WrapText_Changed;
			LinkedComp.HighlightAllOccurrences.Changed += HighlightAllOccurrences_Changed;
			LinkedComp.HighlightCurrentLine.Changed += HighlightCurrentLine_Changed;
			LinkedComp.DrawControlChars.Changed += DrawControlChars_Changed;
			LinkedComp.DrawTabs.Changed += DrawTabs_Changed;
			LinkedComp.DrawSpaces.Changed += DrawSpaces_Changed;
			LinkedComp.SmoothScroll.Changed += SmoothScroll_Changed;
			LinkedComp.VScrollSpeed.Changed += VScrollSpeed_Changed;
			LinkedComp.ScrollPashEndOfFile.Changed += ScrollPashEndOfFile_Changed;
			LinkedComp.VerticalScroll.Changed += VerticalScroll_Changed;
			LinkedComp.HorizontalScroll.Changed += HorizontalScroll_Changed;
			LinkedComp.FitContentHight.Changed += FitContentHight_Changed;
			LinkedComp.MiniMap.Changed += MiniMap_Changed;
			LinkedComp.MiniMapWidth.Changed += MiniMapWidth_Changed;
			LinkedComp.LineCaret.Changed += LineCaret_Changed;
			LinkedComp.CaretBlink.Changed += CaretBlink_Changed;
			LinkedComp.BlinkInterval.Changed += BlinkInterval_Changed;
			LinkedComp.MoveOnRightClick.Changed += MoveOnRightClick_Changed;
			LinkedComp.MidGrapheme.Changed += MidGrapheme_Changed;
			LinkedComp.TextDir.Changed += TextDir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			Text_Changed(null);
			PlaceHolderText_Changed(null);
			Editable_Changed(null);
			ContectMenuEnabled_Changed(null);
			ShortcutKeysEnabled_Changed(null);
			SelectingEnabled_Changed(null);
			DeselectingOnFocusLossEnabled_Changed(null);
			DragAndDropSelectionEnabled_Changed(null);
			VirtualKeyboardEnabled_Changed(null);
			MiddleMousePasteEnabled_Changed(null);
			WrapText_Changed(null);
			HighlightAllOccurrences_Changed(null);
			HighlightCurrentLine_Changed(null);
			DrawControlChars_Changed(null);
			DrawTabs_Changed(null);
			DrawSpaces_Changed(null);
			SmoothScroll_Changed(null);
			VScrollSpeed_Changed(null);
			ScrollPashEndOfFile_Changed(null);
			VerticalScroll_Changed(null);
			HorizontalScroll_Changed(null);
			FitContentHight_Changed(null);
			MiniMap_Changed(null);
			MiniMapWidth_Changed(null);
			LineCaret_Changed(null);
			CaretBlink_Changed(null);
			BlinkInterval_Changed(null);
			MoveOnRightClick_Changed(null);
			MidGrapheme_Changed(null);
			TextDir_Changed(null);
			Language_Changed(null);
			node.TextChanged += Node_TextChanged;
		}

		private void Node_TextChanged() {
			LinkedComp.Text.Value = node.Text;
		}

		private void Language_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Language = LinkedComp.Language.Value);
		}

		private void TextDir_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextDirection = LinkedComp.TextDir.Value switch {
				RTextDirection.Auto => TextDirection.Auto,
				RTextDirection.Ltr => TextDirection.Ltr,
				RTextDirection.Rtl => TextDirection.Rtl,
				_ => TextDirection.Inherited,
			});
		}

		private void MidGrapheme_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CaretMidGrapheme = LinkedComp.MidGrapheme.Value);
		}

		private void MoveOnRightClick_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CaretMoveOnRightClick = LinkedComp.MoveOnRightClick.Value);
		}

		private void BlinkInterval_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CaretBlinkInterval = LinkedComp.BlinkInterval.Value);
		}

		private void CaretBlink_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CaretBlink = LinkedComp.CaretBlink.Value);
		}

		private void LineCaret_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CaretType = LinkedComp.LineCaret.Value ? Godot.TextEdit.CaretTypeEnum.Line : Godot.TextEdit.CaretTypeEnum.Block);
		}

		private void MiniMapWidth_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MinimapWidth = LinkedComp.MiniMapWidth.Value);
		}

		private void MiniMap_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MinimapDraw = LinkedComp.MiniMap.Value);
		}

		private void FitContentHight_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollFitContentHeight = LinkedComp.FitContentHight.Value);
		}

		private void HorizontalScroll_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollHorizontal = LinkedComp.HorizontalScroll.Value);
		}

		private void VerticalScroll_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollVertical = LinkedComp.VerticalScroll.Value);
		}

		private void ScrollPashEndOfFile_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollPastEndOfFile = LinkedComp.ScrollPashEndOfFile.Value);
		}

		private void VScrollSpeed_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollVScrollSpeed = LinkedComp.VScrollSpeed.Value);
		}

		private void SmoothScroll_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScrollSmooth = LinkedComp.SmoothScroll.Value);
		}

		private void DrawSpaces_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DrawSpaces = LinkedComp.DrawSpaces.Value);
		}

		private void DrawTabs_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DrawTabs = LinkedComp.DrawTabs.Value);
		}

		private void DrawControlChars_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DrawControlChars = LinkedComp.DrawControlChars.Value);
		}

		private void HighlightCurrentLine_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.HighlightCurrentLine = LinkedComp.HighlightCurrentLine.Value);
		}

		private void HighlightAllOccurrences_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.HighlightAllOccurrences = LinkedComp.HighlightAllOccurrences.Value);
		}

		private void WrapText_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.WrapMode = LinkedComp.WrapText ? Godot.TextEdit.LineWrappingMode.None : Godot.TextEdit.LineWrappingMode.Boundary);
		}

		private void MiddleMousePasteEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MiddleMousePasteEnabled = LinkedComp.MiddleMousePasteEnabled.Value);
		}

		private void VirtualKeyboardEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VirtualKeyboardEnabled = LinkedComp.MiddleMousePasteEnabled.Value);
		}

		private void DragAndDropSelectionEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DragAndDropSelectionEnabled = LinkedComp.DragAndDropSelectionEnabled.Value);
		}

		private void DeselectingOnFocusLossEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DeselectOnFocusLossEnabled = LinkedComp.DeselectingOnFocusLossEnabled.Value);
		}

		private void SelectingEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SelectingEnabled = LinkedComp.SelectingEnabled.Value);
		}

		private void ShortcutKeysEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShortcutKeysEnabled = LinkedComp.ShortcutKeysEnabled.Value);
		}

		private void ContectMenuEnabled_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ContextMenuEnabled = LinkedComp.ContectMenuEnabled.Value);
		}

		private void Editable_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Editable = LinkedComp.Editable.Value);
		}

		private void PlaceHolderText_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PlaceholderText = LinkedComp.PlaceHolderText.Value);
		}

		private void Text_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (node.Text != LinkedComp.Text.Value) {
					node.Text = LinkedComp.Text.Value;
				}
			});
		}
	}

	public sealed class TextEditLink : TextEditBase<RhuEngine.Components.TextEdit, Godot.TextEdit>
	{
		protected override bool FreeKeyboard => true;

		public override string ObjectName => "TextEdit";

		public override void StartContinueInit() {
		}
	}

}
