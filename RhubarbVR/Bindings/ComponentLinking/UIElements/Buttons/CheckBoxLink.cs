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
	public sealed class CheckBoxLink : Button<RhuEngine.Components.CheckBox, GDExtension.CheckBox>
	{
		public override string ObjectName => "CheckBox";

		public override void StartContinueInit() {
		}
	}
}
