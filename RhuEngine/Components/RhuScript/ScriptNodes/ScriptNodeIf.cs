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
	public class ScriptNodeIf : IScriptNode
	{
		[IgnoreMember]
		public string Text => "If";
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public IScriptNode check;
		[Key(1)]
		public IScriptNode trueValue;
		[Key(2)]
		public IScriptNode falseValue;
		[IgnoreMember()]
		public Type ReturnType => typeof(void);
		[IgnoreMember]
		public World World { get; private set; }
		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(check);
			scriptNodes.Add(trueValue);
			scriptNodes.Add(falseValue);
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(check);
			check.GetChildrenAll(scriptNodes);
			scriptNodes.Add(trueValue);
			trueValue.GetChildrenAll(scriptNodes);
			scriptNodes.Add(falseValue);
			falseValue.GetChildrenAll(scriptNodes);
		}
		public object Invoke(ScriptNodeDataHolder dataHolder) {
			if ((bool)check.Invoke(dataHolder)) {
				trueValue.Invoke(dataHolder);
			}
			else {
				falseValue.Invoke(dataHolder);
			}
			return null;
		}
		public void ClearChildren() {
			check = null;
			trueValue = null;
			falseValue = null;
		}
		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
			check?.LoadIntoWorld(world, rhuScript);
			trueValue?.LoadIntoWorld(world, rhuScript);
			falseValue?.LoadIntoWorld(world, rhuScript);
		}
	}
}
