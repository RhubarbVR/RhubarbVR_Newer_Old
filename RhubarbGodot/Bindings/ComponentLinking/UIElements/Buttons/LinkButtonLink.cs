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

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class LinkButtonLink : ButtonBase<RhuEngine.Components.LinkButton, Godot.LinkButton>
	{
		public override string ObjectName => "LinkButton";

		public override void StartContinueInit() {
			LinkedComp.Text.Changed += Text_Changed;
			LinkedComp.UnderLine.Changed += UnderLine_Changed;
			LinkedComp.TextDir.Changed += TextDir_Changed;
			LinkedComp.Language.Changed += Language_Changed;
			Text_Changed(null);
			UnderLine_Changed(null);
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

		private void UnderLine_Changed(IChangeable obj) {
			node.Underline = LinkedComp.UnderLine.Value switch {
				RUnderLine.OnHover => Godot.LinkButton.UnderlineMode.OnHover,
				RUnderLine.Never => Godot.LinkButton.UnderlineMode.Never,
				_ => Godot.LinkButton.UnderlineMode.Always,
			};
		}

		private void Text_Changed(IChangeable obj) {
			node.Text = LinkedComp.Text.Value;
		}
	}
}
