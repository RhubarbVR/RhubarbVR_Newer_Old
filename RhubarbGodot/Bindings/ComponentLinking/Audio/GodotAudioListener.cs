using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine;
using RhuEngine.Components;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class GodotAudioListener : WorldPositionLinked<RhuEngine.Components.AudioListener3D, Godot.AudioListener3D>
	{
		public override bool GoToEngineRoot => false;
		public override string ObjectName => "AudioListener3D";

		public override void StartContinueInit() {
			LinkedComp.Current.Changed += Current_Changed;
			Current_Changed(null);
		}

		private void Current_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (!LinkedComp.Current.Value) {
					node.ClearCurrent();
					return;
				}
				if (LinkedComp.Entity.Viewport is null) {
					return;
				}
				node.MakeCurrent();
			});
		}
	}
}
