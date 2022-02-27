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
	public class ScriptNodeWrite : IScriptNode
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public IScriptNode scriptNode;
		[Key(1)]
		public uint NodeIndex;
		[IgnoreMember()]
		public Type ReturnType => scriptNode.ReturnType;
		[IgnoreMember]
		public World World { get; private set; }
		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(scriptNode);
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(scriptNode);
			scriptNode.GetChildrenAll(scriptNodes);
		}
		public object Invoke(ScriptNodeDataHolder dataHolder) {
			var value = scriptNode.Invoke(dataHolder);
			dataHolder.localValues[NodeIndex] = value;
			return value;
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
	}
}
