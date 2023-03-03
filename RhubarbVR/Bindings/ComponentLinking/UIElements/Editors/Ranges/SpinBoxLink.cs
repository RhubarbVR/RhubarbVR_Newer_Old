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
	public sealed class SpinBoxLink : RangeBase<RhuEngine.Components.SpinBox, GDExtension.SpinBox>
	{
		public override string ObjectName => "SpinBox";
		protected override bool FreeKeyboard => true;

		public override void StartContinueInit() {
			LinkedComp.Editable.Changed += Editable_Changed;
			LinkedComp.UpdateOnTextChanged.Changed += UpdateOnTextChanged_Changed;
			LinkedComp.Prefix.Changed += Prefix_Changed;
			LinkedComp.Suffix.Changed += Suffix_Changed;
			LinkedComp.ArrowStep.Changed += ArrowStep_Changed;
			Editable_Changed(null);
			UpdateOnTextChanged_Changed(null);
			Prefix_Changed(null);
			Suffix_Changed(null);
			ArrowStep_Changed(null);

		}

		private void ArrowStep_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CustomArrowStep = LinkedComp.ArrowStep.Value);
		}

		private void Suffix_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Suffix = LinkedComp.Suffix.Value);
		}

		private void Prefix_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Prefix = LinkedComp.Prefix.Value);
		}

		private void UpdateOnTextChanged_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.UpdateOnTextChanged = LinkedComp.UpdateOnTextChanged.Value);
		}

		private void Editable_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Editable = LinkedComp.Editable.Value);
		}
	}
}
