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
	public sealed class CheckButtonLink : Button<RhuEngine.Components.CheckButton, Godot.CheckButton>
	{
		public override string ObjectName => "CheckButton";

		public override void StartContinueInit() {

		}
	}
}
