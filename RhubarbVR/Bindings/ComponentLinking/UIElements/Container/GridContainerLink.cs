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
	public sealed class GridContainerLink : ContainerBase<RhuEngine.Components.GridContainer, GDExtension.GridContainer>
	{
		public override string ObjectName => "GridContainer";

		public override void StartContinueInit() {
			LinkedComp.Columns.Changed += Columns_Changed;
			Columns_Changed(null);
		}

		private void Columns_Changed(IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Columns = LinkedComp.Columns.Value);
		}
	}
}
