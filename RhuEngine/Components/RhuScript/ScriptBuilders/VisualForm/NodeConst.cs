//using RhuEngine.WorldObjects;
//using RhuEngine.WorldObjects.ECS;

//using StereoKit;
//using RhuEngine.Components.ScriptNodes;
//using System;
//using RhuEngine.DataStructure;
//using SharedModels;
//using System.Collections.Generic;
//using System.Linq;
//using World = RhuEngine.WorldObjects.World;
//using System.Reflection;

//namespace RhuEngine.Components
//{
//	[Category(new string[] { "RhuScript\\ScriptBuilders\\VisualForm" })]
//	public class NodeConst : Node
//	{
//		public SyncRef<Entity> UI;
//		[OnChanged(nameof(LoadType))]
//		public Sync<Type> OutputType;

//		public Sync<object> Value;

//		public SyncRef<GenericEditor> Editor;

//		public override string NodeName => "Const";

//		private void LoadType() {
//			if(Output.Target is not null) {
//				Output.Target.TargetType.Value = OutputType.Value;
//			}
//			Editor.Target?.Entity.Destroy();
//			if(UI.Target is null) {
//				return;
//			}
//			var editor = UI.Target.AddChild("Editor").AttachComponent<GenericEditor>();
//			editor.DynamicLinker.Target = Value;
//			editor.Type.Value = OutputType.Value;
//			Editor.Target = editor;
//		}

//		public override void LoadViusual(Entity entity) {
//			FlowOut.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow Out", true);
//			FlowIn.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow In", false);
//			Output.Target = LoadNodeButton(entity, typeof(void), -0.001f, "Output", true);
//			UI.Target = entity;
//			FlowButtons.Add().Target = FlowOut.Target;
//			FlowButtons.Add().Target = FlowIn.Target;
//		}

//		public override void OnClearError() {
//		}

//		public override void OnError() {
//		}

//		public override void Gen(VisualScriptBuilder visualScriptBuilder, IScriptNode node, VisualScriptBuilder.NodeBuilder Builder) {
//			Same(Builder);
//			OutputType.Value = ((ScriptNodeConst)node).ConstType;
//			Value.Value = ((ScriptNodeConst)node).Value;
//		}
//	}
//}
