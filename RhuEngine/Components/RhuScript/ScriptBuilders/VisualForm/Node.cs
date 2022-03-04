using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
using RhuEngine.Components.ScriptNodes;
using System;
using RhuEngine.DataStructure;
using SharedModels;
using System.Collections.Generic;
using System.Linq;
using static RhuEngine.Components.VisualScriptBuilder;

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript\\ScriptBuilders\\VisualForm" })]
	public abstract class Node : Component
	{
		public SyncRef<NodeButton> FlowOut;
		public SyncRef<NodeButton> FlowIn;
		public SyncRef<NodeButton> Output;
		public SyncRef<Entity> NodesRoot;

		public SyncRef<VisualScriptBuilder> VScriptBuilder;

		public SyncRef<UIWindow> Window;

		public SyncObjList<SyncRef<NodeButton>> FlowButtons;

		public abstract string NodeName { get; }

		public override void OnAttach() {
			var window = Entity.AttachComponent<UIWindow>();
			Window.Target = window;
			window.Text.Value = NodeName;
			window.WindowType.Value = UIWin.Normal;
			var UI = Entity.AddChild("UI");
			UI.AttachComponent<UIGroup>();
			LoadViusual(UI);
		}

		public NodeButton LoadNodeButton(Entity entity,Type type,float level,string text,bool isOut = false) {
			if(NodesRoot.Target is null) {
				NodesRoot.Target = entity.AddChild("NodeRoot");
			}
			var Out = NodesRoot.Target.AttachComponent<NodeButton>();
			Out.Node.Target = this;
			Out.IsOutput.Value = isOut;
			Out.TargetType.Value = type;
			Out.Level.Value = level;
			Out.ToolTipText.Value = text;
			return Out;
		}
		public abstract void LoadViusual(Entity entity);
		public void OnErrorInt() {
			if (Window.Target is null) {
				return;
			}
			Window.Target.TintColor.Value = new Color(1, 0.1f, 0.1f);
			OnError();
		}

		public abstract void OnError();

		public void OnClearErrorInt() {
			if (Window.Target is null) {
				return;
			}
			Window.Target.TintColor.Value = Color.White;
			OnClearError();
		}

		public abstract void OnClearError();

		public abstract void Gen(VisualScriptBuilder visualScriptBuilder,IScriptNode node, NodeBuilder builder);

		public void Same(NodeBuilder Builder) {
			if (Builder.LastInputPoint is null) {
				if (Builder.LastFlowPoint is not null) {
					Builder.LastFlowPoint.ConnectedTo.Target = FlowIn.Target;
					Builder.Flow = true;
				}
			}
			else {
				Output.Target.ConnectedTo.Target = Builder.LastInputPoint;
				Builder.Flow = false;
			}
		}
	}
}
