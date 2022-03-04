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
	public class MethodNode : Node
	{
		public SyncRef<Entity> UI;
		public SyncRef<NodeButton> Input;
		public SyncObjList<SyncRef<NodeButton>> Prams;

		public Sync<Type> InputType;

		public Sync<Type> GenericArgument;

		[OnChanged(nameof(LoadMethod))]
		public Sync<string> Method;

		public SyncValueList<Type> PramTypes;

		public override string NodeName => Method;

		private void LoadMethod() {
			if(Window.Target is not null) {
				Window.Target.Text.Value = Method.Value;
			}
			var method = InputType.Value?.GetMethod(Method.Value, (PramTypes.Count == 0)? Type.EmptyTypes: PramTypes);
			if ((method?.IsGenericMethod??false) && GenericArgument.Value is not null) {
				method = method?.MakeGenericMethod(GenericArgument.Value);
			}
			if (method?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (InputType.Value?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
					return;
				}
				else {
					if (!method.IsStatic) {
						return;
					}
				}
			}
			if (method.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				return;
			}
			if (Prams.Count > 0) {
				foreach (SyncRef<NodeButton> item in Prams) {
					item.Target?.Destroy();
				}
				Prams.Clear();
			}
			if(Output.Target is not null) {
				Output.Target.TargetType.Value = method.ReturnType;
			}
			if (Input.Target is not null) {
				Input.Target.TargetType.Value = InputType;
			}
			if(UI.Target is null) {
				return;
			}
			if((InputType.Value?.IsAbstract ?? false) && (InputType.Value?.IsSealed ?? false)) {
				if(Input.Target is not null) {
					Input.Target.Enabled.Value = false;
				}
			}
			else {
				if (Input.Target is not null) {
					Input.Target.Enabled.Value = true;
				}
			}
			foreach (var item in method.GetParameters()) {
				var node = item.HasDefaultValue
					? LoadNodeButton(UI.Target, item.ParameterType, -0.001f, item.Name + " = " + item.DefaultValue?.ToString() ?? "Null")
					: LoadNodeButton(UI.Target, item.ParameterType, -0.001f, item.Name);
				node.RenderLabel.Value = true;
				Prams.Add().Target = node;
			}
		}

		public override void LoadViusual(Entity entity) {
			FlowOut.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow Out", true);
			FlowIn.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow In", false);
			Output.Target = LoadNodeButton(entity, typeof(void), -0.001f, "Output", true);
			Input.Target = LoadNodeButton(entity, typeof(World), -0.001f, "Target");
			Input.Target.RenderLabel.Value = true;
			UI.Target = entity;
			FlowButtons.Add().Target = FlowOut.Target;
			FlowButtons.Add().Target = FlowIn.Target;

		}

		public override void OnClearError() {
		}

		public override void OnError() {
		}

		public override void Gen(VisualScriptBuilder visualScriptBuilder, IScriptNode node, VisualScriptBuilder.NodeBuilder Builder) {
			var scriptNodeMethod = (ScriptNodeMethod)node;
			Same(Builder);
			InputType.Value = scriptNodeMethod.InputType;
			GenericArgument.Value = scriptNodeMethod.GenericArgument;
			PramTypes.Clear();
			PramTypes.Append(scriptNodeMethod.PramTypes);
			Method.Value = scriptNodeMethod.Method;
			Builder.LastInputPoint = Input.Target;
			var lastpos = Builder.pos;
			var lastFlow = Builder.Flow;
			visualScriptBuilder.LoadNodes(scriptNodeMethod.ScriptNode, Builder);
			Builder.pos = lastpos;
			Builder.Flow = lastFlow;
			var count = 0;
			foreach (var item in scriptNodeMethod.Prams) {
				Builder.LastInputPoint = Prams[count].Target;
				lastpos += new Vec3(0, -0.15f, 0);
				Builder.pos = lastpos;
				Builder.Flow = lastFlow;
				visualScriptBuilder.LoadNodes(item, Builder);
				count++;
			}
		}
	}
}
