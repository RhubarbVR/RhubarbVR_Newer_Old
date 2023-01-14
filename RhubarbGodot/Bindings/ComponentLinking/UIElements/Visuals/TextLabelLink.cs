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
using RhubarbVR.Bindings.TextureBindings;
using RhubarbVR.Bindings.FontBindings;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class TextLabelLink : UIElementLinkBase<RhuEngine.Components.TextLabel, Godot.Label>
	{
		public override string ObjectName => "TextLabel";

		public override void StartContinueInit() {
			node.LabelSettings = new LabelSettings();
			LinkedComp.LineSpacing.Changed += LineSpacing_Changed;
			LinkedComp.Font.LoadChange += Font_LoadChange;
			LinkedComp.TextSize.Changed += TextSize_Changed;
			LinkedComp.TextColor.Changed += TextColor_Changed;
			LinkedComp.OutlineSize.Changed += OutlineSize_Changed;
			LinkedComp.OutlineColor.Changed += OutlineColor_Changed;
			LinkedComp.ShadowSize.Changed += ShadowSize_Changed;
			LinkedComp.ShadowColor.Changed += ShadowColor_Changed;
			LinkedComp.ShadowOffset.Changed += ShadowOffset_Changed;
			LinkedComp.HorizontalAlignment.Changed += HorizontalAlignment_Changed;
			LinkedComp.VerticalAlignment.Changed += VerticalAlignment_Changed;
			LinkedComp.AutowrapMode.Changed += AutowrapMode_Changed;
			LinkedComp.ClipText.Changed += ClipText_Changed;
			LinkedComp.OverrunBehavior.Changed += OverrunBehavior_Changed;
			LinkedComp.Uppercase.Changed += Uppercase_Changed;
			LinkedComp.LinesSkipped.Changed += LinesSkipped_Changed;
			LinkedComp.MaxLinesVisible.Changed += MaxLinesVisible_Changed;
			LinkedComp.VisibleCharactersBehavior.Changed += VisibleCharactersBehavior_Changed;
			LinkedComp.VisibleRatio.Changed += VisibleRatio_Changed;
			LinkedComp.TextDir.Changed += TextDir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			LinkedComp.Text.Changed += Text_Changed;
			Text_Changed(null);
			LineSpacing_Changed(null);
			Font_LoadChange(null);
			TextSize_Changed(null);
			TextColor_Changed(null);
			OutlineSize_Changed(null);
			OutlineColor_Changed(null);
			ShadowSize_Changed(null);
			ShadowColor_Changed(null);
			ShadowOffset_Changed(null);
			HorizontalAlignment_Changed(null);
			VerticalAlignment_Changed(null);
			AutowrapMode_Changed(null);
			ClipText_Changed(null);
			OverrunBehavior_Changed(null);
			Uppercase_Changed(null);
			LinesSkipped_Changed(null);
			MaxLinesVisible_Changed(null);
			VisibleCharactersBehavior_Changed(null);
			VisibleRatio_Changed(null);
			TextDir_Changed(null);
			Language_Changed(null);
		}

		private void Text_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => 
				node.Text = LinkedComp.Text.Value);
		}

		private void Font_LoadChange(RFont obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.Font = LinkedComp.Font?.Asset?.Inst is GodotFont font ? (font?.FontFile) : null);
		}

		private void Language_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Language = LinkedComp.Language.Value);
		}

		private void TextDir_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextDirection = LinkedComp.TextDir.Value switch { RTextDirection.Auto => TextDirection.Auto, RTextDirection.Ltr => TextDirection.Ltr, RTextDirection.Rtl => TextDirection.Rtl, _ => TextDirection.Inherited, });
		}

		private void VisibleRatio_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibleRatio = LinkedComp.VisibleRatio.Value);
		}

		private void VisibleCharactersBehavior_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VisibleCharactersBehavior = LinkedComp.VisibleCharactersBehavior.Value switch { RhuEngine.Components.RVisibleCharactersBehavior.CharactersBeforeShaping => VisibleCharactersBehavior.CharsBeforeShaping, RhuEngine.Components.RVisibleCharactersBehavior.CharactersAfterShaping => VisibleCharactersBehavior.CharsAfterShaping, RhuEngine.Components.RVisibleCharactersBehavior.GlyphsLayoutDir => VisibleCharactersBehavior.GlyphsAuto, RhuEngine.Components.RVisibleCharactersBehavior.GlyphsLayoutRightToLeft => VisibleCharactersBehavior.GlyphsRtl, _ => VisibleCharactersBehavior.GlyphsLtr, });
		}

		private void MaxLinesVisible_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.MaxLinesVisible = LinkedComp.MaxLinesVisible.Value);
		}

		private void LinesSkipped_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LinesSkipped = LinkedComp.LinesSkipped.Value);
		}

		private void Uppercase_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Uppercase = LinkedComp.Uppercase.Value);
		}

		private void OverrunBehavior_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextOverrunBehavior = LinkedComp.OverrunBehavior.Value switch { ROverrunBehavior.TrimChar => OverrunBehavior.TrimChar, ROverrunBehavior.TrimWord => OverrunBehavior.TrimWord, ROverrunBehavior.TrimEllipsis => OverrunBehavior.TrimEllipsis, ROverrunBehavior.TrimWordEllipsis => OverrunBehavior.TrimWordEllipsis, _ => OverrunBehavior.NoTrimming, });
		}

		private void ClipText_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ClipText = LinkedComp.ClipText.Value);
		}

		private void AutowrapMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.AutowrapMode = LinkedComp.AutowrapMode.Value switch { RAutowrapMode.Arbitrary => AutowrapMode.Arbitrary, RAutowrapMode.Word => AutowrapMode.Word, RAutowrapMode.WordSmart => AutowrapMode.WordSmart, _ => AutowrapMode.Off, });
		}

		private void VerticalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.VerticalAlignment = LinkedComp.VerticalAlignment.Value switch { RVerticalAlignment.Top => VerticalAlignment.Top, RVerticalAlignment.Center => VerticalAlignment.Center, RVerticalAlignment.Bottom => VerticalAlignment.Bottom, _ => VerticalAlignment.Fill, });
		}

		private void HorizontalAlignment_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.HorizontalAlignment = LinkedComp.HorizontalAlignment.Value switch { RHorizontalAlignment.Left => HorizontalAlignment.Left, RHorizontalAlignment.Center => HorizontalAlignment.Center, RHorizontalAlignment.Right => HorizontalAlignment.Right, _ => HorizontalAlignment.Fill, });
		}

		private void ShadowOffset_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.ShadowOffset = new Vector2(LinkedComp.ShadowOffset.Value.x, LinkedComp.ShadowOffset.Value.y));
		}

		private void ShadowColor_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.ShadowColor = new Color(LinkedComp.ShadowColor.Value.r, LinkedComp.ShadowColor.Value.g, LinkedComp.ShadowColor.Value.b, LinkedComp.ShadowColor.Value.a));
		}

		private void ShadowSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.ShadowSize = LinkedComp.ShadowSize.Value);
		}

		private void OutlineColor_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.OutlineColor = new Color(LinkedComp.OutlineColor.Value.r, LinkedComp.OutlineColor.Value.g, LinkedComp.OutlineColor.Value.b, LinkedComp.OutlineColor.Value.a));
		}

		private void OutlineSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.OutlineSize = LinkedComp.OutlineSize.Value);
		}

		private void TextColor_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.FontColor = new Color(LinkedComp.TextColor.Value.r, LinkedComp.TextColor.Value.g, LinkedComp.TextColor.Value.b, LinkedComp.TextColor.Value.a));
		}

		private void TextSize_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.FontSize = LinkedComp.TextSize.Value);
		}

		private void LineSpacing_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.LabelSettings.LineSpacing = LinkedComp.LineSpacing.Value);
		}
	}
}
