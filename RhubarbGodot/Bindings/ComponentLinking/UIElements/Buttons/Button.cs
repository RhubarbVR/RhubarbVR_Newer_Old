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
	public abstract class Button<T, T2> : ButtonBase<T, T2> where T : RhuEngine.Components.Button, new() where T2 : Godot.Button, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.Text.Changed += Text_Changed;
			LinkedComp.Icon.LoadChange += Icon_LoadChange;
			LinkedComp.Flat.Changed += Flat_Changed;
			LinkedComp.ClipText.Changed += ClipText_Changed;
			LinkedComp.Alignment.Changed += Alignment_Changed;
			LinkedComp.TextOverrunBehavior.Changed += TextOverrunBehavior_Changed;
			LinkedComp.IconAlignment.Changed += IconAlignment_Changed;
			LinkedComp.TextDir.Changed += TextDir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			Text_Changed(null);
			Icon_LoadChange(null);
			Flat_Changed(null);
			ClipText_Changed(null);
			Alignment_Changed(null);
			TextOverrunBehavior_Changed(null);
			IconAlignment_Changed(null);
			TextDir_Changed(null);
			Language_Changed(null);
		}

		private void Icon_LoadChange(RTexture2D obj) {
			node.Icon = LinkedComp.Icon.Asset?.Inst is GodotTexture2D godotTex ? (godotTex?.Texture2D) : null;
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

		private void IconAlignment_Changed(IChangeable obj) {
			node.IconAlignment = LinkedComp.IconAlignment.Value switch {
				RButtonAlignment.Left => HorizontalAlignment.Left,
				RButtonAlignment.Right => HorizontalAlignment.Right,
				RButtonAlignment.Center => HorizontalAlignment.Center,
				_ => HorizontalAlignment.Fill,
			};
		}

		private void TextOverrunBehavior_Changed(IChangeable obj) {
			node.TextOverrunBehavior = LinkedComp.TextOverrunBehavior.Value switch {
				ROverrunBehavior.TrimChar => TextServer.OverrunBehavior.TrimChar,
				ROverrunBehavior.TrimWord => TextServer.OverrunBehavior.TrimWord,
				ROverrunBehavior.TrimEllipsis => TextServer.OverrunBehavior.TrimEllipsis,
				ROverrunBehavior.TrimWordEllipsis => TextServer.OverrunBehavior.TrimWordEllipsis,
				_ => TextServer.OverrunBehavior.NoTrimming,
			};
		}

		private void Alignment_Changed(IChangeable obj) {
			node.Alignment = LinkedComp.Alignment.Value switch {
				RButtonAlignment.Left => HorizontalAlignment.Left,
				RButtonAlignment.Center => HorizontalAlignment.Center,
				RButtonAlignment.Right => HorizontalAlignment.Right,
				_ => HorizontalAlignment.Fill,
			};
		}

		private void ClipText_Changed(IChangeable obj) {
			node.ClipText = LinkedComp.ClipText.Value;
		}

		private void Flat_Changed(IChangeable obj) {
			node.Flat = LinkedComp.Flat.Value;
		}

		private void Text_Changed(IChangeable obj) {
			node.Text = LinkedComp.Text.Value;
		}
	}

	public sealed class ButtonLink : Button<RhuEngine.Components.Button, Godot.Button>
	{
		public override string ObjectName => "Button";

		public override void StartContinueInit() {

		}
	}
}
