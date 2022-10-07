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
	public abstract class ContainerBase<T,T2> : UIElementLinkBase<T,T2> where T : RhuEngine.Components.Container, new() where T2 : Godot.Container, new()
	{
		public override void Init() {
			base.Init();

		}
	}

}
