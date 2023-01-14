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
using RhuEngine;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class FlowContainerLink : ContainerBase<RhuEngine.Components.FlowContainer, Godot.FlowContainer>
	{
		public override string ObjectName => "FlowContainer";

		public override void StartContinueInit() {
			LinkedComp.Vertical.Changed += Vertical_Changed;
			Vertical_Changed(null);
		}

		private void Vertical_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Vertical = LinkedComp.Vertical.Value);
		}
	}
}
