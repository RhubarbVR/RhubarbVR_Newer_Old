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

namespace RhubarbVR.Bindings.ComponentLinking
{
	public abstract class SeparatorBase<T,T2> : UIElementLinkBase<T,T2> where T : RhuEngine.Components.Separator, new() where T2 : GDExtension.Separator, new()
	{
		public override void Init() {
			base.Init();

		}
	}

}
