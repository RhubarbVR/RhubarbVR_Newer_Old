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
using static Godot.TextServer;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class RichTextLabelLink : UIElementLinkBase<RhuEngine.Components.RichTextLabel, Godot.RichTextLabel>
	{
		public override string ObjectName => "RichTextLabel";

		public override void StartContinueInit() {
			LinkedComp.BitcodeEnabled.Changed += BitcodeEnabled_Changed;
			LinkedComp.Text.Changed += Text_Changed;
			LinkedComp.FitContentHeight.Changed += FitContentHeight_Changed;
			LinkedComp.ScrollActive.Changed += ScrollActive_Changed;
			LinkedComp.ScrollFollowing.Changed += ScrollFollowing_Changed;
			LinkedComp.AutoWrapMode.Changed += AutoWrapMode_Changed;
			LinkedComp.TabSize.Changed += TabSize_Changed;
			LinkedComp.ContextMenuEnabled.Changed += ContextMenuEnabled_Changed;
			LinkedComp.ShortcutKeysEnabled.Changed += ShortcutKeysEnabled_Changed;
			LinkedComp.MetaUnderline.Changed += MetaUnderline_Changed;
			LinkedComp.HintUnderline.Changed += HintUnderline_Changed;
			LinkedComp.Threading.Changed += Threading_Changed;
			LinkedComp.ProgressBarDelay.Changed += ProgressBarDelay_Changed;
			LinkedComp.TextSelectionEnabled.Changed += TextSelectionEnabled_Changed;
			LinkedComp.DeselectingOnFocusLossEnabled.Changed += DeselectingOnFocusLossEnabled_Changed;
			LinkedComp.VisibleCharactersBehavior.Changed += VisibleCharactersBehavior_Changed;
			LinkedComp.VisibleRatio.Changed += VisibleRatio_Changed;
			LinkedComp.TextDir.Changed += TextDir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			BitcodeEnabled_Changed(null);
			Text_Changed(null);
			FitContentHeight_Changed(null);
			ScrollActive_Changed(null);
			ScrollFollowing_Changed(null);
			AutoWrapMode_Changed(null);
			TabSize_Changed(null);
			ContextMenuEnabled_Changed(null);
			ShortcutKeysEnabled_Changed(null);
			MetaUnderline_Changed(null);
			HintUnderline_Changed(null);
			Threading_Changed(null);
			ProgressBarDelay_Changed(null);
			TextSelectionEnabled_Changed(null);
			DeselectingOnFocusLossEnabled_Changed(null);
			VisibleCharactersBehavior_Changed(null);
			VisibleRatio_Changed(null);
			TextDir_Changed(null);
			Language_Changed(null);
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

		private void VisibleRatio_Changed(IChangeable obj) {
			node.VisibleRatio = LinkedComp.VisibleRatio.Value;
		}

		private void VisibleCharactersBehavior_Changed(IChangeable obj) {
			node.VisibleCharactersBehavior = LinkedComp.VisibleCharactersBehavior.Value switch {
				RhuEngine.Components.RVisibleCharactersBehavior.CharactersBeforeShaping => VisibleCharactersBehavior.CharsBeforeShaping,
				RhuEngine.Components.RVisibleCharactersBehavior.CharactersAfterShaping => VisibleCharactersBehavior.CharsAfterShaping,
				RhuEngine.Components.RVisibleCharactersBehavior.GlyphsLayoutDir => VisibleCharactersBehavior.GlyphsAuto,
				RhuEngine.Components.RVisibleCharactersBehavior.GlyphsLayoutRightToLeft => VisibleCharactersBehavior.GlyphsRtl,
				_ => VisibleCharactersBehavior.GlyphsLtr,
			};
		}

		private void DeselectingOnFocusLossEnabled_Changed(IChangeable obj) {
			node.DeselectOnFocusLossEnabled = LinkedComp.DeselectingOnFocusLossEnabled.Value;
		}


		private void TextSelectionEnabled_Changed(IChangeable obj) {
			node.SelectionEnabled = LinkedComp.TextSelectionEnabled.Value;
		}

		private void ProgressBarDelay_Changed(IChangeable obj) {
			node.ProgressBarDelay = LinkedComp.ProgressBarDelay.Value;
		}

		private void Threading_Changed(IChangeable obj) {
			node.Threaded = LinkedComp.Threading.Value;
		}

		private void HintUnderline_Changed(IChangeable obj) {
			node.HintUnderlined = LinkedComp.HintUnderline.Value;
		}

		private void MetaUnderline_Changed(IChangeable obj) {
			node.MetaUnderlined = LinkedComp.MetaUnderline.Value;
		}

		private void ShortcutKeysEnabled_Changed(IChangeable obj) {
			node.ShortcutKeysEnabled = LinkedComp.ShortcutKeysEnabled.Value;
		}

		private void ContextMenuEnabled_Changed(IChangeable obj) {
			node.ContextMenuEnabled = LinkedComp.ContextMenuEnabled.Value;
		}

		private void TabSize_Changed(IChangeable obj) {
			node.TabSize = LinkedComp.TabSize.Value;
		}

		private void AutoWrapMode_Changed(IChangeable obj) {
			node.AutowrapMode = LinkedComp.AutoWrapMode.Value switch {
				RAutowrapMode.Arbitrary => AutowrapMode.Arbitrary,
				RAutowrapMode.Word => AutowrapMode.Word,
				RAutowrapMode.WordSmart => AutowrapMode.WordSmart,
				_ => AutowrapMode.Off,
			};
		}

		private void ScrollFollowing_Changed(IChangeable obj) {
			node.ScrollFollowing = LinkedComp.ScrollFollowing.Value;
		}

		private void ScrollActive_Changed(IChangeable obj) {
			node.ScrollActive = LinkedComp.ScrollActive.Value;
		}

		private void FitContentHeight_Changed(IChangeable obj) {
			node.FitContentHeight = LinkedComp.FitContentHeight.Value;
		}

		private void Text_Changed(IChangeable obj) {
			node.Text = LinkedComp.Text.Value;
		}

		private void BitcodeEnabled_Changed(IChangeable obj) {
			node.BbcodeEnabled = LinkedComp.BitcodeEnabled.Value;
		}
	}
}
