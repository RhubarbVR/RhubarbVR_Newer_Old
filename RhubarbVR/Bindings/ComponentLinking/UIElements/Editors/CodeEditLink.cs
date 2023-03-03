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
	public sealed class CodeEditLink : TextEditBase<RhuEngine.Components.CodeEdit, GDExtension.CodeEdit>
	{
		public override string ObjectName => "CodeEdit";
		protected override bool FreeKeyboard => true;

		public override void StartContinueInit() {
		}
	}

}
