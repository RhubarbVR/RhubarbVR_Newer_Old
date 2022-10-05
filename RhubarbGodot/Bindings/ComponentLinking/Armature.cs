using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public class ArmatureLink : WorldPositionLinked<Armature, Skeleton3D>
	{
		public override string ObjectName => "Armature";

		public override void StartContinueInit() {
		}
	}
}
