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
	[Category(new string[] { "RhuScript\\ScriptBuilders" })]
	public class VisualScriptBuilder : ScriptBuilder
	{
		public SyncRef<UIWindow> EditWindow;

		public SyncRef<UIGroupPages> NodesGroup;

		public Linker<Color> WindowTint;

		public Linker<string> WindowText;

		public Linker<bool> IsFocusedLinker;

		[OnChanged(nameof(LoadScriptNodes))]
		public Sync<Type> InputType;

		[OnChanged(nameof(LoadScriptNodes))]
		public Sync<Type> OutPutType;

		public SyncObjList<SyncRef<Node>> nodes;

		public override void OnError() {
			foreach (SyncRef<Node> item in nodes) {
				if(item.Target is not null) {
					item.Target.OnErrorInt();
				}
			}
		}

		public override void OnClearError() {
			foreach (SyncRef<Node> item in nodes) {
				if (item.Target is not null) {
					item.Target.OnClearErrorInt();
				}
			}
		}

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
			var enabledGroup = Window.Entity.AddChild("NodesSelect").AttachComponent<UIGroupPages>();
			IsFocusedLinker.SetLinkerTarget(enabledGroup.Enabled);
			NodesGroup.Target = enabledGroup;
			LoadScriptNodes();
			Window.Entity.AddChild("Bar").AttachComponent<UIHSeparator>();
			var previousButton = Window.Entity.AddChild("PreviousButton");
			var pbutton = previousButton.AttachComponent<UIButton>();
			pbutton.Size.Value = new Vec2(((0.3f + (Engine.UISettings.gutter * 2)) / 2) - (Engine.UISettings.gutter / 2), 0);
			pbutton.Text.Value = "<";
			pbutton.onClick.Target = enabledGroup.PreviousPage;
			previousButton.AttachComponent<UISameLine>();
			var nextButton = Window.Entity.AddChild("NextButton");
			var nbutton = nextButton.AttachComponent<UIButton>();
			nbutton.Text.Value = ">";
			nbutton.Size.Value = pbutton.Size.Value;
			nbutton.onClick.Target = enabledGroup.NextPage;
		}
		[Exsposed]
		public void SpawnNode(byte[] node) {
			if (!Serializer.TryToRead<IScriptNode>(node,out var scriptNode)) {
				Log.Info("Failed to Spawned Node ");
				return;
			}
			scriptNode.LoadIntoWorld(World, null);
			Log.Info("Spawned Node " + scriptNode.Text);

		}

		public void LoadScriptNodes() {
			Log.Info($"Loading node list with Input {InputType.Value?.GetFormattedName() ?? "Null"} Output {OutPutType.Value?.GetFormattedName() ?? "Null"}");
			var nodes = new List<IScriptNode>();
			if (InputType.Value is not null) {
				nodes.AddRange(ScriptNodeBuidlers.GetNodeMethods(InputType.Value));
				nodes.AddRange(ScriptNodeBuidlers.GetNodeFieldsRead(InputType.Value));
				nodes.AddRange(ScriptNodeBuidlers.GetNodeFieldsWrite(InputType.Value));
			}
			nodes.AddRange(ScriptNodeBuidlers.GetScriptNodes());
			if(OutPutType.Value is null) {
				LoadNodeListSelectList(nodes);
			}
			else {
				var nodesOut = from node in nodes
							   where node.ReturnType == OutPutType.Value
							   select node;
				LoadNodeListSelectList(nodesOut);
			}
		}

		public void LoadNodeListSelectList(IEnumerable<IScriptNode> scriptNodes) {
			if(NodesGroup.Target is null) {
				return;
			}
			Log.Info($"Loading node list with {scriptNodes.Count()} Nodes");
			NodesGroup.Target.Entity.children.Clear();
			var rootEntity = NodesGroup.Target.Entity;
			Entity CurrentPageEntity = null;
			var count = 1;
			var PageAmount = 0;
			var ammount = scriptNodes.Count();
			foreach (var node in scriptNodes) {
				if (count % 9 == 1) {
					CurrentPageEntity = rootEntity.AddChild($"page{PageAmount}");
					CurrentPageEntity.AttachComponent<UIGroupPageItem>().PageIndex.Value = PageAmount;
					PageAmount++;
				}
				var nodeEntity = CurrentPageEntity.AddChild(node.Text);
				var button = nodeEntity.AttachComponent<UIButton>();
				button.Size.Value = new Vec2(0.1f, 0.05f);
				button.Text.Value = node.Text;
				node.ClearChildren();
				var nodeCall = nodeEntity.AttachComponent<AddSingleValuePram<byte[]>>();
				nodeCall.Value.Value = Serializer.Save(node);
				nodeCall.Target.Target = SpawnNode;
				button.onClick.Target = nodeCall.Call;
				if (count % 3 != 0) {
					if (ammount != count) {
						nodeEntity.AttachComponent<UISameLine>();
					}
				}
				count++;
			}
			var Pages = NodesGroup.Target;
			Pages.MaxPages.Value = PageAmount;

		}

		public override void Compile() {
			FocusScriptBuilder();
		}

		public override void LoadFromScript() {
			FocusScriptBuilder();
			LoadNodes(ScriptNode);
		}

		public class NodeBuilder
		{
			public InitNode node;
			public IScriptNode CurrentNode;
			public IScriptNode LastNode;
		}

		private T SpawnNode<T>()where T : Node,new() {
			var entity = Entity.AddChild(typeof(T).GetFormattedName());
			entity.GlobalTrans = Matrix.TS(new Vec3(-0.05f, 0.05f, -0.035f),new Vec3(1.5f)) * (EditWindow.Target?.Entity.GlobalTrans ?? Matrix.Identity);
			var ret = entity.AttachComponent<T>((node) => node.VScriptBuilder.Target = this);
			nodes.Add().Target = ret;
			return ret;
		}

		private void LoadNodes(IScriptNode node, NodeBuilder Builder = null) {
			if (node is null) {
				return;
			}
			if(Builder is null) {
				Builder = new NodeBuilder {
					node = SpawnNode<InitNode>()
				};
			}
			Builder.LastNode = Builder.CurrentNode;
			Builder.CurrentNode = node;
			if (node is ScriptNodeMethod scriptNodeMethod) {

			}
			else if (node is ScriptNodeConst scriptNodeConst) {

			}
			else if (node is ScriptNodeGroup scriptNodeGroup) {

			}
			else if (node is ScriptNodeWorld scriptNodeWorld) {

			}
			else if (node is ScriptNodeRoot scriptNodeRoot) {

			}
			else if (node is ScriptNodeThrow scriptNodeThrow) {

			}
			else if (node is ScriptNodeWrite scriptNodeWrite) {

			}
			else if (node is ScriptNodeRead scriptNodeRead) {

			}
			else if (node is ScriptNodeReadField scriptNodeReadField) {

			}
			else if (node is ScriptNodeWriteField scriptNodeWriteField) {

			}
			else {
				Log.Err($"Uknown node type: {node.GetType().GetFormattedName()}");
			}
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

		public override void OnRhuScriptAdded() {
			LoadFromScript();
		}
	}
}
