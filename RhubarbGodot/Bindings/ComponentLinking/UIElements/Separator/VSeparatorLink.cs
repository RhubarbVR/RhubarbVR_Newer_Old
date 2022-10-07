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

namespace RhubarbVR.Bindings.ComponentLinking {
	public sealed class VSeparatorLink : SeparatorBase<RhuEngine.Components.VerticalSeparator, Godot.VSeparator>
	{
		public override string ObjectName => "VSeparator";

		public override void StartContinueInit() {
		}
	}
}
