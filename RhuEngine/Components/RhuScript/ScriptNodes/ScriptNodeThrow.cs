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
	public class ScriptNodeThrow : IScriptNode
	{
		[IgnoreMember]
		public string Text => "Throw\nvoid";

		[Key(0)]
		public IScriptNode ScriptNode;

		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[IgnoreMember()]
		public Type ReturnType => typeof(void);
		[IgnoreMember()]
		public World World { get; private set; }
		public object Invoke(ScriptNodeDataHolder dataHolder) {
			var value = ScriptNode.Invoke(dataHolder);
			if (value is string svalue) {
				throw new Exception(svalue);
			}
			else if (value is Exception exception) {
				throw exception;
			}
			throw new Exception("No value was given for Throw");
		}
		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
		}
		public void ClearChildren() {
			ScriptNode = null;
		}
		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			ScriptNode.GetChildrenAll(scriptNodes);
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			ScriptNode?.LoadIntoWorld(world, rhuScript);
			World = world;
			RhuScript = rhuScript;
		}
	}
}
