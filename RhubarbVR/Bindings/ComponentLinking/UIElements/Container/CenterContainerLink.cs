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
	public sealed class CenterContainerLink : ContainerBase<RhuEngine.Components.CenterContainer, GDExtension.CenterContainer>
	{
		public override string ObjectName => "CenterContainer";

		public override void StartContinueInit() {
			LinkedComp.UseTopLeft.Changed += UseTopLeft_Changed;
			UseTopLeft_Changed(null);
		}

		private void UseTopLeft_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.UseTopLeft = LinkedComp.UseTopLeft.Value);
		}
	}
}
