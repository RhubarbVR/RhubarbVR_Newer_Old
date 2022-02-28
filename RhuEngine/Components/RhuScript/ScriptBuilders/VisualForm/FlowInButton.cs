using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;
using System.Collections.Generic;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript\\ScriptBuilders\\VisualForm" })]
	public class FlowInButton : UIComponent
	{
		public override void RenderUI() {
			UI.PushId(Pointer.GetHashCode());
			var enabled = false;
			if (UI.ToggleAt(" ",ref enabled, new Vec3((UI.LayoutRemaining.x/1.25f) + 0.01f, 0.025f,0), new Vec2(0.025f))) {
				AddWorldCoroutine(() => {

				});
			}
			UI.PopId();
		}
	}
}
