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
	public class ScriptNodeWorld : IScriptNode
	{
		[IgnoreMember]
		public string Text => "World";
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }

		[IgnoreMember()]
		public World World { get; private set; }
		[IgnoreMember()]
		public Type ReturnType => typeof(World);
		public object Invoke(ScriptNodeDataHolder dataHolder) {
			return World;
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
