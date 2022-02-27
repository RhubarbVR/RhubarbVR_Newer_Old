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
	public class ScriptNodeRead : IScriptNode
	{
		[IgnoreMember]
		public string Text => "ReadNode";

		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public uint NodeIndex;
		[IgnoreMember()]
		public Type ReturnType => RhuScript?.LocalValueNode[NodeIndex]?.ReturnType;
		[IgnoreMember]
		public World World { get; private set; }

		public object Invoke(ScriptNodeDataHolder dataHolder) {
			return dataHolder.localValues[NodeIndex];
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
		public void GetChildren(List<IScriptNode> scriptNodes) {
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
		}
	}
}
