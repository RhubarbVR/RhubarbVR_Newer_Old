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
	public class InitNode : Node
	{
		public override string NodeName => "Init ";

		public override void LoadViusual(Entity entity) {
			var Out = entity.AttachComponent<FlowOutButton>();

		}
	}
}
