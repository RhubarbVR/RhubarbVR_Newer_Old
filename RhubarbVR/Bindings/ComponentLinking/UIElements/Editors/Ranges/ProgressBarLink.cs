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

namespace RhubarbVR.Bindings.ComponentLinking {
	public sealed class ProgressBarLink : RangeBase<RhuEngine.Components.ProgressBar, GDExtension.ProgressBar>
	{
		public override string ObjectName => "ProgressBar";

		public override void StartContinueInit() {
			LinkedComp.ShowPerrcentage.Changed += ShowPerrcentage_Changed;
			LinkedComp.FillMode.Changed += FillMode_Changed;
			ShowPerrcentage_Changed(null);
			FillMode_Changed(null);
		}

		private void FillMode_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FillModeValue = (int)LinkedComp.FillMode.Value);
		}

		private void ShowPerrcentage_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ShowPercentage = LinkedComp.ShowPerrcentage.Value);
		}
	}
}
