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
	public abstract class SliderBarBase<T, T2> : RangeBase<T, T2> where T : RhuEngine.Components.Slider, new() where T2 : Godot.Slider, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.Editable.Changed += Editable_Changed;
			LinkedComp.Scrollable.Changed += Scrollable_Changed;
			LinkedComp.TickCount.Changed += TickCount_Changed;
			LinkedComp.TickOnBorders.Changed += TickOnBorders_Changed;
			Editable_Changed(null);
			Scrollable_Changed(null);
			TickCount_Changed(null);
			TickOnBorders_Changed(null);

		}

		private void TickOnBorders_Changed(IChangeable obj) {
			node.TicksOnBorders = LinkedComp.TickOnBorders.Value;
		}

		private void TickCount_Changed(IChangeable obj) {
			node.TickCount = LinkedComp.TickCount.Value;
		}

		private void Scrollable_Changed(IChangeable obj) {
			node.Scrollable = LinkedComp.Scrollable.Value;
		}

		private void Editable_Changed(IChangeable obj) {
			node.Editable = LinkedComp.Editable.Value;
		}
	}
}
