using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public class SoundSourceLink : WorldPositionLinked<RhuEngine.Components.SoundSource, Node3D>
	{
		public override string ObjectName => "Sound";

		public override void StartContinueInit() {
		}
	}
}
