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
	public class ScriptNodeMethod : IScriptNode
	{
		[IgnoreMember]
		public string Text
		{
			get {
				var text = Method;
				text += "(";
				foreach (var item in PramTypes) {
					text += item?.GetFormattedName();
				}
				text += ") Return:";
				text += ReturnType?.GetFormattedName();
				return text;
			}
		}

		[Key(0)]
		public IScriptNode ScriptNode;
		[Key(1)]
		public IScriptNode[] Prams;
		[IgnoreMember]
		private MethodInfo _method;
		[Key(2)]
		public string Method;
		[Key(3)]
		public Type[] PramTypes;
		[IgnoreMember]
		public bool IsGeneric => _method.IsGenericMethod;
		[Key(4)]
		public Type GenericArgument;
		[IgnoreMember]
		public Type ReturnType => _method.ReturnType;
		[IgnoreMember]
		public World World { get; private set; }
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		public void LoadMethod() {
			var method = ScriptNode.ReturnType?.GetMethod(Method, PramTypes);
			if (method.IsGenericMethod) {
				method.MakeGenericMethod(GenericArgument);
			}
			if (method.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				return;
			}
			_method = method;
		}

		public object Invoke(ScriptNodeDataHolder dataHolder) {
			var prams = new object[Prams.Length];
			for (var i = 0; i < prams.Length; i++) {
				prams[i] = Prams[i]?.Invoke(dataHolder);
			}
			return _method?.Invoke(ScriptNode.Invoke(dataHolder), prams);
		}
		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			RhuScript = rhuScript;
			World = world;
			ScriptNode.LoadIntoWorld(world, rhuScript);
			foreach (var item in Prams) {
				item.LoadIntoWorld(world, rhuScript);
			}
			LoadMethod();
		}

		public IScriptNode[] NodesForPram(int pram) {
			return ScriptNodeBuidlers.GetScriptNodes(PramTypes[pram]);
		}

		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			foreach (var item in Prams) {
				scriptNodes.Add(item);
			}
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			ScriptNode.GetChildrenAll(scriptNodes);
			foreach (var item in Prams) {
				scriptNodes.Add(item);
				item.GetChildrenAll(scriptNodes);
			}
		}

		public ScriptNodeMethod(IScriptNode node, MethodInfo methodInfo) {
			if (methodInfo.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				throw new Exception("Not Exposed");
			}
			ScriptNode = node;
			Method = methodInfo.Name;
			PramTypes = methodInfo.GetParameters().Select((pram) => pram.ParameterType).ToArray();
			Prams = new IScriptNode[PramTypes.Length];
			_method = methodInfo;
		}
		public ScriptNodeMethod() {

		}
	}
}
