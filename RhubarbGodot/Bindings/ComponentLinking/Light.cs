using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public class GodotLight : WorldPositionLinked<Light, Node3D>
	{
		public override string ObjectName => "Light";

		public override void StartContinueInit() {
		}
	}
}
