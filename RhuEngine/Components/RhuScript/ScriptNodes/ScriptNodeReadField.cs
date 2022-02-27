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
	public class ScriptNodeReadField : IScriptNode
	{
		[Key(0)]
		public IScriptNode ScriptNode;
		[IgnoreMember]
		private FieldInfo _field;
		[Key(1)]
		public string Field;
		[IgnoreMember]
		public Type ReturnType => _field.FieldType;
		[IgnoreMember]
		public World World { get; private set; }
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		public void LoadField() {
			var field = ScriptNode.ReturnType?.GetField(Field);
			if (field.GetCustomAttribute<UnExsposedAttribute>() is not null) {
				return;
			}
			if (field.GetCustomAttribute<ExsposedAttribute>() is null) {
				if (!typeof(IWorldObject).IsAssignableFrom(field.GetType())) {
					return;
				}
			}
			_field = field;
		}

		public object Invoke(ScriptNodeDataHolder dataHolder) {
			return _field?.GetValue(ScriptNode.Invoke(dataHolder));
		}
		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			RhuScript = rhuScript;
			World = world;
			ScriptNode.LoadIntoWorld(world, rhuScript);
			LoadField();
		}

		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			ScriptNode.GetChildrenAll(scriptNodes);
		}

		public ScriptNodeReadField(IScriptNode node, FieldInfo fieldInfo) {
			if (fieldInfo.GetCustomAttribute<UnExsposedAttribute>() is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<ExsposedAttribute>() is null) {
				if (!typeof(IWorldObject).IsAssignableFrom(fieldInfo.GetType())) {
					throw new Exception("Not Exposed");
				}
			}
			ScriptNode = node;
			Field = fieldInfo.Name;
			_field = fieldInfo;
		}
		public ScriptNodeReadField() {

		}
	}
}
