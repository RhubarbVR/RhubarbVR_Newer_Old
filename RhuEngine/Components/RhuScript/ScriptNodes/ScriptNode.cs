using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MessagePack;
using RhuEngine.WorldObjects;
using System.Linq;

namespace RhuEngine.Components.ScriptNodes
{
	[Union(0,typeof(ScriptNodeMethod))]
	[Union(1, typeof(ScriptNodeConst))]
	[Union(2, typeof(ScriptNodeGroup))]
	[Union(3, typeof(ScriptNodeWorld))]
	[Union(4, typeof(ScriptNodeRoot))]
	[Union(5, typeof(ScriptNodeThrow))]
	public interface IScriptNode
	{
		public RhuScript RhuScript { get; }
		public World World { get; }
		public void LoadIntoWorld(World world,RhuScript rhuScript);
		public Type ReturnType { get; }
		public object Invoke();
	}

	public static class ScriptNodeBuidlers
	{
		public static ScriptNodeMethod[] GetNodeMethods(this IScriptNode node) {
			var methods = new List<ScriptNodeMethod>();
			if(node.ReturnType is not null) {
				foreach (var item in node.ReturnType.GetMethods()) {
					if (item.GetCustomAttribute<ExsposedAttribute>() is not null) {
						methods.Add(new ScriptNodeMethod(node, item));
					}
				}
			}
			return methods.ToArray();
		}

		public static ScriptNodeMethod[] GetNodeMethods(this IScriptNode node,string method) {
			var methods = new List<ScriptNodeMethod>();
			if (node.ReturnType is not null) {
				foreach (var item in node.ReturnType.GetMethods()) {
					if (item.Name == method) {
						if (item.GetCustomAttribute<ExsposedAttribute>() is not null) {
							methods.Add(new ScriptNodeMethod(node, item));
						}
					}
				}
			}
			return methods.ToArray();
		}

		public static IScriptNode[] GetScriptNodes(Type typeRequirement = null) {
			var list = new List<IScriptNode> {
				new ScriptNodeConst(new short()),
				new ScriptNodeConst(new int()),
				new ScriptNodeConst(new long()),
				new ScriptNodeConst(new ushort()),
				new ScriptNodeConst(new uint()),
				new ScriptNodeConst(new ulong()),
				new ScriptNodeConst(new float()),
				new ScriptNodeConst(new double()),
				new ScriptNodeConst(""),
				new ScriptNodeConst(new bool()),
				new ScriptNodeConst(new byte()),
				new ScriptNodeConst(new sbyte()),
				new ScriptNodeRoot(),
				new ScriptNodeWorld(),
				new ScriptNodeThrow(),
			};

			if (typeRequirement is not null) {
				list = (from e in list
					   where e.ReturnType == typeRequirement
					   select e).ToList();
			}
			return list.ToArray();
		}
	}
	[MessagePackObject()]
	public class ScriptNodeMethod : IScriptNode
	{
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
		public Type ReturnType => _method.ReturnType;
		[IgnoreMember]
		public World World { get; private set; }
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		public void LoadMethod() {
			var method = ScriptNode.ReturnType?.GetMethod(Method, PramTypes);
			if (method.GetCustomAttribute<ExsposedAttribute>() is null) {
				return;
			}
			_method = method;
		}

		public object Invoke() {
			var prams = new object[Prams.Length];
			for (var i = 0; i < prams.Length; i++) {
				prams[i] = Prams[i]?.Invoke();
			}
			return _method?.Invoke(ScriptNode.Invoke(), prams);
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

		public ScriptNodeMethod(IScriptNode node,MethodInfo methodInfo) {
			if(methodInfo.GetCustomAttribute<ExsposedAttribute>() is null) {
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
	[MessagePackObject()]
	public class ScriptNodeGroup : IScriptNode
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public List<IScriptNode> Children;
		[IgnoreMember]
		public Type ReturnType => typeof(void);
		[IgnoreMember]
		public World World { get; private set; }

		public object Invoke() {
			foreach (var item in Children) {
				item.Invoke();
			}
			return null;
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
			foreach (var item in Children) {
				item.LoadIntoWorld(world,rhuScript);
			}
		}
	}
	[MessagePackObject()]
	public class ScriptNodeConst:IScriptNode //Every Value is getting boxed
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[Key(0)]
		public object Value { get; set; }
		[IgnoreMember()]
		public Type ReturnType => Value?.GetType();
		[IgnoreMember]
		public World World { get; private set; }

		public object Invoke() {
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

	}
	[MessagePackObject()]
	public class ScriptNodeWorld : IScriptNode 
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }

		[IgnoreMember()]
		public World World { get; private set; }
		[IgnoreMember()]
		public Type ReturnType => typeof(World);
		public object Invoke() {
			return World;
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
	}
	[MessagePackObject()]
	public class ScriptNodeRoot : IScriptNode
	{
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }

		[IgnoreMember()]
		public Type ReturnType => typeof(RhuScript);
		[IgnoreMember()]
		public World World { get; private set; }
		public object Invoke() {
			return RhuScript;
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
	}

	[MessagePackObject()]
	public class ScriptNodeThrow : IScriptNode
	{
		[Key(0)]
		public IScriptNode ScriptNode;

		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		[IgnoreMember()]
		public Type ReturnType => typeof(void);
		[IgnoreMember()]
		public World World { get; private set; }
		public object Invoke() {
			var value = ScriptNode.Invoke();
			if (value is string svalue) {
				throw new Exception(svalue);
			}
			else if (value is Exception exception) {
				throw exception;
			}
			throw new Exception("No value was given for Throw");
		}

		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			World = world;
			RhuScript = rhuScript;
		}
	}
}
