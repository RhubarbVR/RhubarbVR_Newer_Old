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

namespace RhubarbVR.Bindings.ComponentLinking { 
	public abstract class ScrollBarBase<T, T2> : RangeBase<T, T2> where T : RhuEngine.Components.ScrollBar, new() where T2 : Godot.ScrollBar, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.CustomStep.Changed += CustomStep_Changed;
			CustomStep_Changed(null);
		}

		private void CustomStep_Changed(IChangeable obj) {
			node.CustomStep = LinkedComp.CustomStep.Value;
		}
	}
}
