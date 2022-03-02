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
		public SyncRef<NodeButton> FlowOut;
		public SyncRef<NodeButton> FlowIn;
		public SyncRef<NodeButton> Input;
		public SyncRef<NodeButton> Output;
		public SyncObjList<SyncRef<NodeButton>> Prams;

		public Sync<Type> InputType;

		public Sync<Type> GenericArgument;

		[OnChanged(nameof(LoadMethod))]
		public Sync<string> Method;

		public SyncValueList<Type> PramTypes;

		public override string NodeName => Method;

		private void LoadMethod() {
			Log.Info($"Loading Method Input{InputType.Value}  Method{Method.Value} PramTypes {PramTypes.Count}");
			if(Window.Target is not null) {
				Window.Target.Text.Value = Method.Value;
			}
			var method = InputType.Value?.GetMethod(Method, (PramTypes.Count == 0)? null:PramTypes);
			if ((method?.IsGenericMethod??false) && GenericArgument.Value is not null) {
				method = method?.MakeGenericMethod(GenericArgument.Value);
			}
			if (method?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				return;
			}
			Log.Info($"Method loaded");
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
			foreach (var item in method.GetParameters()) {
				if (item.HasDefaultValue) {
					LoadNodeButton(UI.Target, item.ParameterType, -0.001f, item.Name + " = " + item.DefaultValue?.ToString() ?? "Null").RenderLabel.Value = true;
				}
				else {
					LoadNodeButton(UI.Target, item.ParameterType, -0.001f, item.Name).RenderLabel.Value = true;
				}
			}
		}

		public override void LoadViusual(Entity entity) {
			FlowOut.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow Out", true);
			FlowIn.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow In", false);
			Output.Target = LoadNodeButton(entity, typeof(void), -0.001f, "Output", true);
			Input.Target = LoadNodeButton(entity, typeof(World), -0.001f, "Input");
			Input.Target.RenderLabel.Value = true;
			UI.Target = entity;
		}

		public override void OnClearError() {
		}

		public override void OnError() {
		}
	}
}
