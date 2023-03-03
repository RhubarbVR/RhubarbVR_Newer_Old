using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;
using GDExtension;
using RhuEngine.Components;
using static GDExtension.Control;
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ColorPickerButtonLink : Button<RhuEngine.Components.ColorPickerButton, GDExtension.ColorPickerButton>
	{
		public override string ObjectName => "ColorPickerButton";
		protected override bool FreeKeyboard => true;

		public override void StartContinueInit() {
			LinkedComp.Color.Changed += Color_Changed;
			LinkedComp.EditAlpha.Changed += EditAlpha_Changed;
			Color_Changed(null);
			EditAlpha_Changed(null);
		}

		private void EditAlpha_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.EditAlpha = LinkedComp.EditAlpha.Value);
		}

		private void Color_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Color = new Color(LinkedComp.Color.Value.r, LinkedComp.Color.Value.g, LinkedComp.Color.Value.b, LinkedComp.Color.Value.a));
		}
	}
}
