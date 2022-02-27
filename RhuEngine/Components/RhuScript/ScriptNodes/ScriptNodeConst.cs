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
	public class ScriptNodeConst : IScriptNode //Every Value is getting boxed
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public object Value { get; set; }
		[IgnoreMember()]
		public Type ReturnType => Value?.GetType();
		[IgnoreMember]
		public World World { get; private set; }

		public object Invoke(ScriptNodeDataHolder dataHolder) {
			return Value;
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
		public ScriptNodeConst(object value) {
			Value = value;
		}
		public ScriptNodeConst() {

		}
		public void GetChildren(List<IScriptNode> scriptNodes) {

		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {

		}
	}
}
