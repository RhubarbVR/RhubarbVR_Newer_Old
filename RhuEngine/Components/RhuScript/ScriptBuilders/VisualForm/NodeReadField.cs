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
//	[Category(new string[] { "RhuScript/ScriptBuilders/VisualForm" })]
//	public class NodeReadField : Node
//	{
//		public SyncRef<Entity> UI;
//		public SyncRef<NodeButton> Input;

//		public Sync<Type> InputType;

//		[OnChanged(nameof(LoadField))]
//		public Sync<string> Field;

//		public override string NodeName => Field;

//		private void LoadField() {
//			if(Window.Target is not null) {
//				Window.Target.Text.Value = Field.Value;
//			}
//			var field = InputType.Value?.GetField(Field.Value);
//			if (field.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
//				return;
//			}
//			if (field.GetCustomAttribute<ExsposedAttribute>(true) is null) {
//				if (!typeof(IWorldObject).IsAssignableFrom(field.FieldType)) {
//					return;
//				}
//			}
//			if (Output.Target is not null) {
//				Output.Target.TargetType.Value = field.FieldType;
//			}
//			if (Input.Target is not null) {
//				Input.Target.TargetType.Value = InputType;
//			}
//			if(UI.Target is null) {
//				return;
//			}
//			if((InputType.Value?.IsAbstract ?? false) && (InputType.Value?.IsSealed ?? false)) {
//				if(Input.Target is not null) {
//					Input.Target.Enabled.Value = false;
//				}
//			}
//			else {
//				if (Input.Target is not null) {
//					Input.Target.Enabled.Value = true;
//				}
//			}
//		}

//		public override void LoadViusual(Entity entity) {
//			FlowOut.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow Out", true);
//			FlowIn.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow In", false);
//			Output.Target = LoadNodeButton(entity, typeof(void), -0.001f, "Output", true);
//			Input.Target = LoadNodeButton(entity, typeof(World), -0.001f, "Target");
//			Input.Target.RenderLabel.Value = true;
//			UI.Target = entity;
//			FlowButtons.Add().Target = FlowOut.Target;
//			FlowButtons.Add().Target = FlowIn.Target;

//		}

//		public override void OnClearError() {
//		}

//		public override void OnError() {
//		}

//		public override void Gen(VisualScriptBuilder visualScriptBuilder, IScriptNode node, VisualScriptBuilder.NodeBuilder Builder) {
//			var scriptNodeMethod = (ScriptNodeReadField)node;
//			Same(Builder);
//			InputType.Value = scriptNodeMethod.InputType;
//			Field.Value = scriptNodeMethod.Field;
//			Builder.LastInputPoint = Input.Target;
//			var lastpos = Builder.pos;
//			var lastFlow = Builder.Flow;
//			visualScriptBuilder.LoadNodes(scriptNodeMethod.ScriptNode, Builder);
//			Builder.pos = lastpos;
//			Builder.Flow = lastFlow;
//		}
//	}
//}
