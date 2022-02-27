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
	[Category(new string[] { "RhuScript\\ScriptBuilders" })]
	public class VisualScriptBuilder : ScriptBuilder
	{
		public SyncRef<UIWindow> EditWindow;

		public SyncRef<UIEnableGroup> NodesGroup;

		public Linker<Color> WindowTint;

		public Linker<string> WindowText;

		public Linker<bool> IsFocusedLinker;

		public override void OnAttach() {
			var Window = Entity.AddChild("Edit Window").AttachComponent<UIWindow>();
			WindowTint.SetLinkerTarget(Window.TintColor);
			WindowText.SetLinkerTarget(Window.Text);
			Window.Text.Value = "Focused Visual Script Editor";
			Window.OnWindowGrab.Target = FocusScriptBuilder;
			EditWindow.Target = Window;
			var compEnitity = Window.Entity.AddChild("Compile Button");
			var CompileButton = compEnitity.AttachComponent<UIButton>();
			compEnitity.AttachComponent<UISameLine>();
			CompileButton.Text.Value = "Compile";
			CompileButton.onClick.Target = Compile;
			var LoadScriptButton = Window.Entity.AddChild("Load Script Button").AttachComponent<UIButton>();
			LoadScriptButton.Text.Value = "Load Script";
			LoadScriptButton.onClick.Target = LoadFromScript;
			Window.Entity.AddChild("Bar").AttachComponent<UIHSeparator>();
			FocusScriptBuilder();
			var enabledGroup = Window.Entity.AddChild("NodesSelect").AttachComponent<UIEnableGroup>();
			IsFocusedLinker.SetLinkerTarget(enabledGroup.UIEnabled);
			NodesGroup.Target = enabledGroup;
			LoadNodeListSelectList(ScriptNodeBuidlers.GetScriptNodes());
		}
		[Exsposed]
		public void SpawnNode(byte[] node) {
			if (!Serializer.TryToRead<IScriptNode>(node,out var scriptNode)) {
				return;
			}
			Log.Info("Spawned Node " + scriptNode.Text);
		}

		public void LoadNodeListSelectList(IScriptNode[] scriptNodes) {
			if(NodesGroup.Target is null) {
				return;
			}
			NodesGroup.Target.Entity.children.Clear();
			var rootEntity = NodesGroup.Target.Entity;
			var count = 0;
			foreach (var node in scriptNodes) {
				var nodeEntity = rootEntity.AddChild(node.Text);
				var button = nodeEntity.AttachComponent<UIButtonWithPram<byte[]>>();
				button.Text.Value = node.Text;
				button.Pram.Value = Serializer.Save(node);
				button.onClick.Target = SpawnNode;
				if (count % 3 != 0) {
					nodeEntity.AttachComponent<UISameLine>();
				}
				count++;
			}
		}

		public override void Compile() {
			FocusScriptBuilder();
		}

		public override void LoadFromScript() {
			FocusScriptBuilder();
		}

		public override void OnGainFocus() {
			if (WindowTint.Linked) {
				WindowTint.LinkedValue = Color.White;
			}
			if (WindowText.Linked) {
				WindowText.LinkedValue = "Focused Visual Script Editor";
			}
			if (IsFocusedLinker.Linked) {
				IsFocusedLinker.LinkedValue = true;
			}
		}

		public override void OnLostFocus() {
			if (WindowTint.Linked) {
				WindowTint.LinkedValue = new Color(1f, 0.5f, 0.5f);
			}
			if (WindowText.Linked) {
				WindowText.LinkedValue = "UnFocused Visual Script Editor";
			}
			if (IsFocusedLinker.Linked) {
				IsFocusedLinker.LinkedValue = false;
			}
		}
	}
}
