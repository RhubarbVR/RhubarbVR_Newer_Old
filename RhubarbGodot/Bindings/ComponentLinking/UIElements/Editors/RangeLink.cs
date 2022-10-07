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
	public abstract class RangeBase<T, T2> : UIElementLinkBase<T, T2> where T : RhuEngine.Components.Range, new() where T2 : Godot.Range, new()
	{
		public override void Init() {
			base.Init();
			LinkedComp.MinValue.Changed += MinValue_Changed;
			LinkedComp.MaxValue.Changed += MaxValue_Changed;
			LinkedComp.StepValue.Changed += StepValue_Changed;
			LinkedComp.PageValue.Changed += PageValue_Changed;
			LinkedComp.Value.Changed += Value_Changed;
			LinkedComp.ExpEdit.Changed += ExpEdit_Changed;
			LinkedComp.Rounded.Changed += Rounded_Changed;
			LinkedComp.AllowGreater.Changed += AllowGreater_Changed;
			LinkedComp.AllowLesser.Changed += AllowLesser_Changed;
			MinValue_Changed(null);
			MaxValue_Changed(null);
			StepValue_Changed(null);
			PageValue_Changed(null);
			Value_Changed(null);
			ExpEdit_Changed(null);
			Rounded_Changed(null);
			AllowGreater_Changed(null);
			AllowLesser_Changed(null);
		}

		private void AllowLesser_Changed(IChangeable obj) {
			node.AllowLesser = LinkedComp.AllowLesser.Value;
		}

		private void AllowGreater_Changed(IChangeable obj) {
			node.AllowGreater = LinkedComp.AllowGreater.Value;
		}

		private void Rounded_Changed(IChangeable obj) {
			node.Rounded = LinkedComp.Rounded.Value;
		}

		private void ExpEdit_Changed(IChangeable obj) {
			node.ExpEdit = LinkedComp.ExpEdit.Value;
		}

		private void Value_Changed(IChangeable obj) {
			node.Value = LinkedComp.Value.Value;
		}

		private void PageValue_Changed(IChangeable obj) {
			node.Page = LinkedComp.PageValue.Value;
		}

		private void StepValue_Changed(IChangeable obj) {
			node.Step = LinkedComp.StepValue.Value;
		}

		private void MaxValue_Changed(IChangeable obj) {
			node.MaxValue = LinkedComp.MaxValue.Value;
		}

		private void MinValue_Changed(IChangeable obj) {
			node.MinValue = LinkedComp.MinValue.Value;
		}
	}

	public sealed class RangeLink : RangeBase<RhuEngine.Components.Range, Godot.Range>
	{
		public override string ObjectName => "Range";

		public override void StartContinueInit() {
		}
	}

}
