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
	public class ScriptNodeRoot : IScriptNode
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }

		[IgnoreMember()]
		public Type ReturnType => typeof(RhuScript);
		[IgnoreMember()]
		public World World { get; private set; }
		public object Invoke(ScriptNodeDataHolder dataHolder) {
			return RhuScript;
		}
		public void GetChildren(List<IScriptNode> scriptNodes) {

		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {

		}
		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
	}
}
