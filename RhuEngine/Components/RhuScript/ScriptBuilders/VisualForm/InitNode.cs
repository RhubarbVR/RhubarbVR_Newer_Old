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

namespace RhuEngine.Components
{
	[Category(new string[] { "RhuScript\\ScriptBuilders\\VisualForm" })]
	public class InitNode : Node
	{
		public override bool HideFlow => false;
		public SyncRef<UIButtonSafe> SafeButton;

		public SyncRef<UILabel> ErrorLable;
		public override string NodeName => "Init";

		public override void Gen(VisualScriptBuilder visualScriptBuilder, IScriptNode node, VisualScriptBuilder.NodeBuilder builder) {
			throw new NotImplementedException();
		}

		public override void LoadViusual(Entity entity) {
			FlowOut.Target = LoadNodeButton(entity, typeof(Action), 0.035f, "Flow Out", true);
			var clickButton = entity.AttachComponent<UIButton>();
			entity.AttachComponent<UISameLine>();
			var safeButton = entity.AttachComponent<UIButtonSafe>();
			safeButton.Text.Value = "Clear Error";
			SafeButton.Target = safeButton;
			safeButton.Enabled.Value = false;
			ErrorLable.Target = entity.AttachComponent<UILabel>();
			ErrorLable.Target.Enabled.Value = false;
			clickButton.Text.Value = "Pulse";
			if(VScriptBuilder.Target?.script.Target is null) {
				return;
			}
			clickButton.onClick.Target = VScriptBuilder.Target.script.Target.CallMainMethod;
			// Refs for other nodes
			//LoadNodeButton(entity, typeof(Action), 0.035f, "Flow In", false);
			//LoadNodeButton(entity, typeof(string), -0.001f, "First input").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(bool), -0.001f, "Second input").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(int), -0.001f, "Hey look it is an int").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(uint), -0.001f, "Hey look it is an uint").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(float), -0.001f, "Hey look it is an float").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(Sync<float>), -0.001f, "Hey look it is an sync<float>").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(InitNode), -0.001f, "Hey look it is an random type").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(World), -0.001f, "Hey look it is another random type").RenderLabel.Value = true;
			//LoadNodeButton(entity, typeof(Sync<float>), -0.001f, "Hey look it is an output", true);
		}

		public override void OnClearError() {
			if (ErrorLable.Target is null) {
				return;
			}
			ErrorLable.Target.Enabled.Value = false;
			if(SafeButton.Target is null) {
				return;
			}
			SafeButton.Target.Enabled.Value = false;
			SafeButton.Target.onClick.Target = null;
			Entity.GlobalTrans = Matrix.T(-0.1f, 0, 0) * Entity.GlobalTrans;
		}

		public override void OnError() {
			if(ErrorLable.Target is null) {
				return;
			}
			ErrorLable.Target.Enabled.Value = true;
			if (SafeButton.Target is null) {
				return;
			}
			if (VScriptBuilder.Target?.script.Target is null) {
				return;
			}
			ErrorLable.Target.Text.Value = (VScriptBuilder.Target?.script.Target?.Error?.Message ?? "Error Unknown");

			SafeButton.Target.Enabled.Value = true;
			SafeButton.Target.onClick.Target = VScriptBuilder.Target.script.Target.ClearErrorsSafe;
			Entity.GlobalTrans = Matrix.T(0.1f,0,0) * Entity.GlobalTrans;
		}
	}
}
