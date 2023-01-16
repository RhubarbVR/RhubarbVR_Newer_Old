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

namespace RhubarbVR.Bindings.ComponentLinking {
	public sealed class ViewPortConRectLink : UIElementLinkBase<ViewportConnector, ConnectedViewport>
	{
		public override string ObjectName => "ViewPortConRect";

		public override void StartContinueInit() {
			LinkedComp.Target.Changed += Target_Changed;
			Target_Changed(null);
		}


		private void Target_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrameNoPass(() => node.Viewport = LinkedComp?.Target?.Target);
		}
	}
}
