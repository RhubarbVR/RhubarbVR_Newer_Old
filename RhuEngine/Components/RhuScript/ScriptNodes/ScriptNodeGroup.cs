using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MessagePack;
using RhuEngine.WorldObjects;
using System.Linq;

namespace RhuEngine.Components.ScriptNodes
{
	[MessagePackObject()]
	public class ScriptNodeGroup : IScriptNode
	{
		[IgnoreMember]
		public string Text => "Group";
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public List<IScriptNode> Children = new();
		[IgnoreMember]
		public Type ReturnType => typeof(void);
		[IgnoreMember]
		public World World { get; private set; }
		public void GetChildren(List<IScriptNode> scriptNodes) {
			foreach (var item in Children) {
				scriptNodes.Add(item);
			}
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			foreach (var item in Children) {
				scriptNodes.Add(item);
				item.GetChildrenAll(scriptNodes);
			}
		}
		public object Invoke(ScriptNodeDataHolder dataHolder) {
			foreach (var item in Children) {
				item.Invoke(dataHolder);
			}
			return null;
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
			foreach (var item in Children) {
				item.LoadIntoWorld(world, rhuScript);
			}
		}
		public void ClearChildren() {
			Children.Clear();
		}
	}
}
