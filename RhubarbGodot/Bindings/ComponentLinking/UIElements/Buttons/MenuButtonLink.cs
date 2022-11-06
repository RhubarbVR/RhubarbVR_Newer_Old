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
	public sealed class MenuButtonLink : Button<RhuEngine.Components.MenuButton, Godot.MenuButton>
	{
		public override string ObjectName => "MenuButton";

		public override void StartContinueInit() {
			LinkedComp.SwitchOnHover.Changed += SwitchOnHover_Changed;
			LinkedComp.Items.Changed += Items_Changed;
			SwitchOnHover_Changed(null);
			Items_Changed(null);
		}
		//Todo Load in items;
		private void Items_Changed(IChangeable obj) {

		}

		private void SwitchOnHover_Changed(IChangeable obj) {
			node.SwitchOnHover = LinkedComp.SwitchOnHover.Value;
		}
	}
}
