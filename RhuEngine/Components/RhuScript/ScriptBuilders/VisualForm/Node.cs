//using RhuEngine.WorldObjects;
//using RhuEngine.WorldObjects.ECS;

//using StereoKit;
//using RhuEngine.Components.ScriptNodes;
//using System;
//using RhuEngine.DataStructure;
//using SharedModels;
//using System.Collections.Generic;
//using System.Linq;
//using static RhuEngine.Components.VisualScriptBuilder;

//namespace RhuEngine.Components
//{
//	[Category(new string[] { "RhuScript\\ScriptBuilders\\VisualForm" })]
//	public abstract class Node : Component
//	{
//		public virtual bool HideFlow => true;
//		public SyncRef<NodeButton> FlowOut;
//		public SyncRef<NodeButton> FlowIn;
//		public SyncRef<NodeButton> Output;
//		public SyncRef<Entity> NodesRoot;

//		public SyncRef<VisualScriptBuilder> VScriptBuilder;

//		public SyncRef<UIWindow> Window;

//		public SyncObjList<SyncRef<NodeButton>> FlowButtons;

//		public abstract string NodeName { get; }

//		public override void OnAttach() {
//			var window = Entity.AttachComponent<UIWindow>();
//			Window.Target = window;
//			window.Text.Value = NodeName;
//			window.WindowType.Value = UIWin.Normal;
//			var UI = Entity.AddChild("UI");
//			var header = UI.AttachComponent<UIHeaderHover>();
//			header.OnHoverLost.Target = UnHover;
//			header.OnHover.Target = Hover;
//			UI.AttachComponent<UIGroup>();
//			LoadViusual(UI);
//		}

//		[Exsposed]
//		public void UnHover(Handed handed) {
//			if(!HideFlow) {
//				return;
//			}
//			var DoHide = true;
//			foreach (SyncRef<NodeButton> item in FlowButtons) {
//				if (item.Target is not null) {
//					if(item.Target.ConnectedTo.Target is not null) {
//						DoHide = false;
//					}
//					if (item.Target.ConnectFrom.Count > 0) {
//						DoHide = false;
//					}
//				}
//			}
//			if (DoHide) {
//				foreach (SyncRef<NodeButton> item in FlowButtons) {
//					if (item.Target is not null) {
//						item.Target.Enabled.Value = false;
//					}
//				}
//			}
//		}

//		[Exsposed]
//		public void Hover(Handed handed) {
//			if (!HideFlow) {
//				return;
//			}
//			foreach (SyncRef<NodeButton> item in FlowButtons) {
//				if (item.Target is not null) {
//					item.Target.Enabled.Value = true;
//				}
//			}
//		}

//		public NodeButton LoadNodeButton(Entity entity,Type type,float level,string text,bool isOut = false) {
//			if(NodesRoot.Target is null) {
//				NodesRoot.Target = entity.AddChild("NodeRoot");
//			}
//			var Out = NodesRoot.Target.AttachComponent<NodeButton>();
//			Out.Node.Target = this;
//			Out.IsOutput.Value = isOut;
//			Out.TargetType.Value = type;
//			Out.Level.Value = level;
//			Out.ToolTipText.Value = text;
//			return Out;
//		}
//		public abstract void LoadViusual(Entity entity);
//		public void OnErrorInt() {
//			if (Window.Target is null) {
//				return;
//			}
//			Window.Target.TintColor.Value = new Color(1, 0.1f, 0.1f);
//			OnError();
//		}

//		public abstract void OnError();

//		public void OnClearErrorInt() {
//			if (Window.Target is null) {
//				return;
//			}
//			Window.Target.TintColor.Value = Color.White;
//			OnClearError();
//		}

//		public abstract void OnClearError();

//		public abstract void Gen(VisualScriptBuilder visualScriptBuilder,IScriptNode node, NodeBuilder builder);

//		public void Same(NodeBuilder Builder) {
//			if (Builder.LastInputPoint is null) {
//				if (Builder.LastFlowPoint is not null) {
//					Builder.LastFlowPoint.ConnectedTo.Target = FlowIn.Target;
//					Builder.Flow = true;
//				}
//			}
//			else {
//				Output.Target.ConnectedTo.Target = Builder.LastInputPoint;
//				Builder.Flow = false;
//			}
//			Hover(Handed.Max);
//			UnHover(Handed.Max);
//		}
//	}
//}
