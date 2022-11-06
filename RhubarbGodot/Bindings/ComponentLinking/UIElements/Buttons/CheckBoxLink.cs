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
	public sealed class CheckBoxLink : Button<RhuEngine.Components.CheckBox, Godot.CheckBox>
	{
		public override string ObjectName => "CheckBox";

		public override void StartContinueInit() {
		}
	}
}
