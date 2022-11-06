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

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class OptionsButtonLink : Button<RhuEngine.Components.OptionButton, Godot.OptionButton>
	{
		public override string ObjectName => "OptionsButton";

		public override void StartContinueInit() {
			LinkedComp.Selected.Changed += Selected_Changed;
			LinkedComp.FitLongestItem.Changed += FitLongestItem_Changed;
			LinkedComp.Items.Changed += Items_Changed;
			Selected_Changed(null);
			FitLongestItem_Changed(null);
			Items_Changed(null);

		}
		//Todo Load in items;
		private void Items_Changed(IChangeable obj) {

		}

		private void FitLongestItem_Changed(IChangeable obj) {
			node.FitToLongestItem = LinkedComp.FitLongestItem.Value;
		}

		private void Selected_Changed(IChangeable obj) {
			node.Selected = LinkedComp.Selected.Value;
		}
	}
}
