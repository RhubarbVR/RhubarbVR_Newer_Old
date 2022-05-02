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
		[IgnoreMember]
		public string Text => "Read " + Field + "\n " + ReturnType.GetFormattedName();

		[Key(0)]
		public IScriptNode ScriptNode;
		[IgnoreMember]
		private FieldInfo _field;
		[Key(1)]
		public string Field;

		[Key(2)]
		public Type InputType;
		[IgnoreMember]
		public Type ReturnType => _field?.FieldType;
		[IgnoreMember]
		public World World { get; private set; }
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }
		public void LoadField() {
			var field = InputType?.GetField(Field);
			if (field.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				return;
			}
			if (field.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (!typeof(IWorldObject).IsAssignableFrom(field.FieldType)) {
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
			ScriptNode?.LoadIntoWorld(world, rhuScript);
			LoadField();
		}

		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			ScriptNode.GetChildrenAll(scriptNodes);
		}
		public void ClearChildren() {
			ScriptNode = null;
		}
		public ScriptNodeReadField(IScriptNode node, FieldInfo fieldInfo) {
			if (fieldInfo.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (!typeof(IWorldObject).IsAssignableFrom(fieldInfo.FieldType)) {
					throw new Exception("Not Exposed");
				}
			}
			InputType = node.ReturnType;
			ScriptNode = node;
			Field = fieldInfo.Name;
			_field = fieldInfo;
		}
		public ScriptNodeReadField(Type node, FieldInfo fieldInfo) {
			if (fieldInfo.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<ExsposedAttribute>(true) is null) {
				if (!typeof(IWorldObject).IsAssignableFrom(fieldInfo.FieldType)) {
					throw new Exception("Not Exposed");
				}
			}
			InputType = node;
			Field = fieldInfo.Name;
			_field = fieldInfo;
		}
		public ScriptNodeReadField() {

		}
	}
}
