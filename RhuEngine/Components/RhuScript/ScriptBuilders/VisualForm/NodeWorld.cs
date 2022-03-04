using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;
using System.Collections.Generic;
using System.Linq;
using World = RhuEngine.WorldObjects.World;
using System.Reflection;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript\\ScriptBuilders\\VisualForm" })]
	public class NodeWorld : Node
	{
		public override string NodeName => "World";

		public override void Gen(VisualScriptBuilder visualScriptBuilder, IScriptNode node, VisualScriptBuilder.NodeBuilder builder) {
			Same(builder);
		}

		public override void LoadViusual(Entity entity) {
			FlowOut.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow Out", true);
			FlowIn.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow In", false);
			Output.Target = LoadNodeButton(entity, typeof(World), -0.001f, "Output", true);
			var label = entity.AttachComponent<UILabel>();
			label.Text.Value = "\n";
			FlowButtons.Add().Target = FlowOut.Target;
			FlowButtons.Add().Target = FlowIn.Target;
		}

		public override void OnClearError() {
		}

		public override void OnError() {
		}
	}
}
