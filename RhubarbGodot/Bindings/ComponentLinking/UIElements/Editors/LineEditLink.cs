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
using RhubarbVR.Bindings.TextureBindings;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class LineEditLink : UIElementLinkBase<RhuEngine.Components.LineEdit, Godot.LineEdit>
	{
		protected override bool FreeKeyboard => true;

		public override string ObjectName => "LineEdit";

		public override void StartContinueInit() {

			LinkedComp.Text.Changed += Text_Changed;
			LinkedComp.PlaceholderText.Changed += PlaceholderText_Changed;
			LinkedComp.Alignment.Changed += Alignment_Changed;
			LinkedComp.Editable.Changed += Editable_Changed;
			LinkedComp.Secret.Changed += Secret_Changed;
			LinkedComp.SecretCharacter.Changed += SecretCharacter_Changed;
			LinkedComp.ExpandToTextLength.Changed += ExpandToTextLength_Changed;
			LinkedComp.ContextMenuEnabled.Changed += ContextMenuEnabled_Changed;
			LinkedComp.VirtualKeyboardEnabled.Changed += VirtualKeyboardEnabled_Changed;
			LinkedComp.VirtualKeyboardType.Changed += VirtualKeyboardType_Changed;
			LinkedComp.ClearButtonEnabled.Changed += ClearButtonEnabled_Changed;
			LinkedComp.ShortcutKeysEnabled.Changed += ShortcutKeysEnabled_Changed;
			LinkedComp.MiddleMousePasteEnabled.Changed += MiddleMousePasteEnabled_Changed;
			LinkedComp.SelectingEnabled.Changed += SelectingEnabled_Changed;
			LinkedComp.DeselectingOnFocusLossEnabled.Changed += DeselectingOnFocusLossEnabled_Changed;
			LinkedComp.RightIcon.LoadChange += RightIcon_LoadChange;
			LinkedComp.Flat.Changed += Flat_Changed;
			LinkedComp.DrawControlChars.Changed += DrawControlChars_Changed;
			LinkedComp.CaretBlink.Changed += CaretBlink_Changed;
			LinkedComp.CaretColumn.Changed += CaretColumn_Changed;
			LinkedComp.CaretForceDisplay.Changed += CaretForceDisplay_Changed;
			LinkedComp.CaretMidGrapheme.Changed += CaretMidGrapheme_Changed;
			LinkedComp.TextDir.Changed += TextDir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			LinkedComp.SelectAllOnFocus.Changed += SelectAllOnFocus_Changed;
			SelectAllOnFocus_Changed(null);
			Text_Changed(null);
			PlaceholderText_Changed(null);
			Alignment_Changed(null);
			Editable_Changed(null);
			Secret_Changed(null);
			SecretCharacter_Changed(null);
			ExpandToTextLength_Changed(null);
			ContextMenuEnabled_Changed(null);
			VirtualKeyboardEnabled_Changed(null);
			VirtualKeyboardType_Changed(null);
			ClearButtonEnabled_Changed(null);
			ShortcutKeysEnabled_Changed(null);
			MiddleMousePasteEnabled_Changed(null);
			SelectingEnabled_Changed(null);
			DeselectingOnFocusLossEnabled_Changed(null);
			RightIcon_LoadChange(null);
			Flat_Changed(null);
			DrawControlChars_Changed(null);
			CaretBlink_Changed(null);
			CaretColumn_Changed(null);
			CaretForceDisplay_Changed(null);
			CaretMidGrapheme_Changed(null);
			TextDir_Changed(null);
			Language_Changed(null);
			node.TextChanged += Node_TextChanged;
			node.TextSubmitted += Node_TextSubmitted;
		}

		private void SelectAllOnFocus_Changed(IChangeable obj) {
			node.SelectAllOnFocus = LinkedComp.SelectAllOnFocus.Value;
		}

		private void Node_TextSubmitted(string newText) {
			LinkedComp.Text.Value = newText;
			LinkedComp.TextSubmitted?.Target?.Invoke();
		}

		private void Node_TextChanged(string newText) {
			LinkedComp.Text.Value = newText;
			LinkedComp.TextChange?.Target?.Invoke(newText);
		}

		private void RightIcon_LoadChange(RTexture2D obj) {
			node.RightIcon = LinkedComp.RightIcon.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null;
		}

		private void Language_Changed(IChangeable obj) {
			node.Language = LinkedComp.Language.Value;
		}

		private void TextDir_Changed(IChangeable obj) {
			node.TextDirection = LinkedComp.TextDir.Value switch {
				RTextDirection.Auto => TextDirection.Auto,
				RTextDirection.Ltr => TextDirection.Ltr,
				RTextDirection.Rtl => TextDirection.Rtl,
				_ => TextDirection.Inherited,
			};
		}

		private void CaretMidGrapheme_Changed(IChangeable obj) {
			node.CaretMidGrapheme = LinkedComp.CaretMidGrapheme.Value;
		}

		private void CaretForceDisplay_Changed(IChangeable obj) {
			node.CaretForceDisplayed = LinkedComp.CaretForceDisplay.Value;
		}

		private void CaretColumn_Changed(IChangeable obj) {
			node.CaretColumn = LinkedComp.CaretColumn.Value;
		}

		private void CaretBlink_Changed(IChangeable obj) {
			node.CaretBlink = LinkedComp.CaretBlink.Value;
		}

		private void DrawControlChars_Changed(IChangeable obj) {
			node.DrawControlChars = LinkedComp.DrawControlChars.Value;
		}

		private void Flat_Changed(IChangeable obj) {
			node.Flat = LinkedComp.Flat.Value;
		}


		private void DeselectingOnFocusLossEnabled_Changed(IChangeable obj) {
			node.DeselectOnFocusLossEnabled = LinkedComp.DeselectingOnFocusLossEnabled.Value;
		}

		private void SelectingEnabled_Changed(IChangeable obj) {
			node.SelectingEnabled = LinkedComp.SelectingEnabled.Value;
		}

		private void MiddleMousePasteEnabled_Changed(IChangeable obj) {
			node.MiddleMousePasteEnabled = LinkedComp.MiddleMousePasteEnabled.Value;
		}

		private void ShortcutKeysEnabled_Changed(IChangeable obj) {
			node.ShortcutKeysEnabled = LinkedComp.ShortcutKeysEnabled.Value;
		}

		private void ClearButtonEnabled_Changed(IChangeable obj) {
			node.ClearButtonEnabled = LinkedComp.ClearButtonEnabled.Value;
		}

		private void VirtualKeyboardType_Changed(IChangeable obj) {
			node.VirtualKeyboardType = LinkedComp.VirtualKeyboardType.Value switch {
				RVirtualKeyboardType.MultiLine => Godot.LineEdit.VirtualKeyboardTypeEnum.Multiline,
				RVirtualKeyboardType.Number => Godot.LineEdit.VirtualKeyboardTypeEnum.Number,
				RVirtualKeyboardType.Decimal => Godot.LineEdit.VirtualKeyboardTypeEnum.NumberDecimal,
				RVirtualKeyboardType.Phone => Godot.LineEdit.VirtualKeyboardTypeEnum.Phone,
				RVirtualKeyboardType.Email => Godot.LineEdit.VirtualKeyboardTypeEnum.EmailAddress,
				RVirtualKeyboardType.Passowrd => Godot.LineEdit.VirtualKeyboardTypeEnum.Password,
				RVirtualKeyboardType.URL => Godot.LineEdit.VirtualKeyboardTypeEnum.Url,
				_ => Godot.LineEdit.VirtualKeyboardTypeEnum.Default,
			};
		}

		private void VirtualKeyboardEnabled_Changed(IChangeable obj) {
			node.VirtualKeyboardEnabled = LinkedComp.VirtualKeyboardEnabled.Value;
		}

		private void ContextMenuEnabled_Changed(IChangeable obj) {
			node.ContextMenuEnabled = LinkedComp.ContextMenuEnabled.Value;
		}

		private void ExpandToTextLength_Changed(IChangeable obj) {
			node.ExpandToTextLength = LinkedComp.ExpandToTextLength.Value;
		}

		private void SecretCharacter_Changed(IChangeable obj) {
			node.SecretCharacter = LinkedComp.SecretCharacter.Value;
		}

		private void Secret_Changed(IChangeable obj) {
			node.Secret = LinkedComp.Secret.Value;
		}

		private void Editable_Changed(IChangeable obj) {
			node.Editable = LinkedComp.Editable.Value;
		}

		private void Alignment_Changed(IChangeable obj) {
			node.Alignment = LinkedComp.Alignment.Value switch {
				RHorizontalAlignment.Left => HorizontalAlignment.Left,
				RHorizontalAlignment.Center => HorizontalAlignment.Center,
				RHorizontalAlignment.Right => HorizontalAlignment.Right,
				_ => HorizontalAlignment.Fill,
			};
		}

		private void PlaceholderText_Changed(IChangeable obj) {
			node.PlaceholderText = LinkedComp.PlaceholderText.Value;
		}

		private void Text_Changed(IChangeable obj) {
			if(node.Text != LinkedComp.Text.Value) {
				node.Text = LinkedComp.Text.Value;
			}
		}
	}

}
