using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MessagePack;
using RhuEngine.WorldObjects;
using System.Linq;

namespace RhuEngine.Components.ScriptNodes
{
	[Union(0, typeof(ScriptNodeMethod))]
	[Union(1, typeof(ScriptNodeConst))]
	[Union(2, typeof(ScriptNodeGroup))]
	[Union(3, typeof(ScriptNodeWorld))]
	[Union(4, typeof(ScriptNodeRoot))]
	[Union(5, typeof(ScriptNodeThrow))]
	[Union(6, typeof(ScriptNodeWrite))]
	[Union(7, typeof(ScriptNodeRead))]
	[Union(8, typeof(ScriptNodeReadField))]
	[Union(9, typeof(ScriptNodeWriteField))]
	public interface IScriptNode
	{
		public string Text { get; }
		public RhuScript RhuScript { get; }
		public World World { get; }
		public void LoadIntoWorld(World world, RhuScript rhuScript);
		public Type ReturnType { get; }
		public object Invoke(ScriptNodeDataHolder dataHolder);
		public void GetChildren(List<IScriptNode> scriptNodes);
		public void GetChildrenAll(List<IScriptNode> scriptNodes);
		void ClearChildren();
	}
}
