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
	public abstract class Node : Component
	{
		public SyncRef<VisualScriptBuilder> VScriptBuilder;

		public abstract string NodeName { get; }

		public override void OnAttach() {
			var window = Entity.AttachComponent<UIWindow>();
			window.Text.Value = NodeName;
			window.WindowType.Value = UIWin.Normal;
			LoadViusual(Entity.AddChild("UI"));
		}

		public abstract void LoadViusual(Entity entity);
	}
}
