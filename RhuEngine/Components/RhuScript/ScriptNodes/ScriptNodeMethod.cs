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
				//text += "\n ( ";
				//foreach (var item in _method?.GetParameters()) {
				//	text += item.ParameterType?.GetFormattedName();
				//	text += " ";
				//	text += item.Name;
				//	if (item.HasDefaultValue) {
				//		text += " = ";
				//		text += item.DefaultValue.ToString();
				//	}
				//	text += ",";
				//}
				//text = text.Substring(0, text.Length - 1);
				//text += ") \n";
				text += "\n";
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
		public bool IsGeneric => _method?.IsGenericMethod??false;
		[Key(4)]
		public Type GenericArgument;
		[Key(5)]
		public Type InputType;
		[IgnoreMember]
		public object[] Defaults;
		[IgnoreMember]
		public Type ReturnType => _method?.ReturnType;
		[IgnoreMember]
		public World World { get; private set; }
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		public void LoadMethod() {
			var method = InputType?.GetMethod(Method, PramTypes);
			if (method.IsGenericMethod && GenericArgument is not null) {
				method = method?.MakeGenericMethod(GenericArgument);
			}
			if (method?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (InputType?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
					return;
				}
				else {
					if (!method.IsStatic) {
						return;
					}
				}
			}
			if (method.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				return;
			}
			var prams = method.GetParameters();
			Defaults = new object[prams.Length];
			for (var i = 0; i < prams.Length; i++) {
				Defaults[i] = prams[i].DefaultValue;
			}
			_method = method;
		}

		public object Invoke(ScriptNodeDataHolder dataHolder) {
			var prams = new object[Prams.Length];
			for (var i = 0; i < prams.Length; i++) {
				prams[i] = Prams[i] == null ? Defaults.Length > 0 ? Defaults[i] : null : Prams[i].Invoke(dataHolder);
			}
			return _method?.Invoke(ScriptNode?.Invoke(dataHolder), prams);
		}
		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			RhuScript = rhuScript;
			World = world;
			ScriptNode?.LoadIntoWorld(world, rhuScript);
			foreach (var item in Prams) {
				item?.LoadIntoWorld(world, rhuScript);
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
			if (ScriptNode is not null) {
				scriptNodes.Add(ScriptNode);
				ScriptNode.GetChildrenAll(scriptNodes);
			}
			foreach (var item in Prams) {
				if (item is not null) {
					scriptNodes.Add(item);
					item.GetChildrenAll(scriptNodes);
				}
			}
		}
		public ScriptNodeMethod(IScriptNode node, MethodInfo methodInfo) {
			if (methodInfo?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (node.ReturnType?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
					throw new Exception("Not Exposed");
				}
				else {
					if (!methodInfo.IsStatic) {
						throw new Exception("Not Exposed");
					}
				}
			}
			if (methodInfo.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			Method = methodInfo.Name;
			ScriptNode = node;
			InputType = node.ReturnType;
			PramTypes = methodInfo.GetParameters().Select((pram) => pram.ParameterType).ToArray();
			Prams = new IScriptNode[PramTypes.Length];
			_method = methodInfo;
		}
		public ScriptNodeMethod(Type node, MethodInfo methodInfo) {
			if (methodInfo?.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (node.GetCustomAttribute<ExsposedAttribute>(true) is null) {
					throw new Exception("Not Exposed");
				}
				else {
					if (!methodInfo.IsStatic) {
						throw new Exception("Not Exposed");
					}
				}
			}
			if(methodInfo.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			Method = methodInfo.Name;
			InputType = node;
			PramTypes = methodInfo.GetParameters().Select((pram) => pram.ParameterType).ToArray();
			Prams = new IScriptNode[PramTypes.Length];
			_method = methodInfo;
		}
		public ScriptNodeMethod() {

		}
		public void ClearChildren() {
			ScriptNode = null;
		}
	}
}
