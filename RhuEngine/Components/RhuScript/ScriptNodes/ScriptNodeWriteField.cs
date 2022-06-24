using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MessagePack;
using RhuEngine.WorldObjects;
using System.Linq;
using RNumerics;

namespace RhuEngine.Components.ScriptNodes
{
	[MessagePackObject()]
	public class ScriptNodeWriteField : IScriptNode
	{
		[IgnoreMember]
		public string Text => "Write " + Field + "\n " + InputType.GetFormattedName();
		[Key(0)]
		public IScriptNode ScriptNode;
		[Key(1)]
		public IScriptNode SetValue;
		[IgnoreMember]
		private FieldInfo _field;
		[Key(2)]
		public string Field;
		[Key(3)]
		public Type InputType;
		[IgnoreMember]
		public Type ReturnType => typeof(void);
		[IgnoreMember]
		public World World { get; private set; }
		[IgnoreMember()]
		public RhuScript RhuScript { get; private set; }

		public IScriptNode[] GetInputNodes() {
			return ScriptNodeBuidlers.GetScriptNodes(_field?.FieldType);
		}

		public void LoadField() {
			var field = InputType?.GetField(Field);
			if (field.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				return;
			}
			if (field.GetCustomAttribute<NoWriteExsposedAttribute>(true) is not null) {
				return;
			}
			if (field.GetCustomAttribute<ExposedAttribute>(true) is null) {
				return;
			}
			_field = field;
		}

		public object Invoke(ScriptNodeDataHolder dataHolder) {
			_field?.SetValue(ScriptNode.Invoke(dataHolder),SetValue.Invoke(dataHolder));
			return null;
		}
		public void LoadIntoWorld(World world, RhuScript rhuScript) {
			RhuScript = rhuScript;
			World = world;
			ScriptNode?.LoadIntoWorld(world, rhuScript);
			LoadField();
		}

		public void GetChildren(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			scriptNodes.Add(SetValue);

		}

		public void GetChildrenAll(List<IScriptNode> scriptNodes) {
			scriptNodes.Add(ScriptNode);
			ScriptNode.GetChildrenAll(scriptNodes);
			scriptNodes.Add(SetValue);
			SetValue.GetChildrenAll(scriptNodes);
		}

		public void ClearChildren() {
			ScriptNode = null;
			SetValue = null;
		}

		public ScriptNodeWriteField(IScriptNode node, FieldInfo fieldInfo) {
			if (fieldInfo.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<NoWriteExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<ExposedAttribute>(true) is null) {
				throw new Exception("Not Exposed");
			}
			InputType = node.ReturnType;
			ScriptNode = node;
			Field = fieldInfo.Name;
			_field = fieldInfo;
		}
		public ScriptNodeWriteField(Type node, FieldInfo fieldInfo) {
			if (fieldInfo.GetCustomAttribute<UnExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<NoWriteExsposedAttribute>(true) is not null) {
				throw new Exception("Not Exposed");
			}
			if (fieldInfo.GetCustomAttribute<ExposedAttribute>(true) is null) {
				throw new Exception("Not Exposed");
			}
			InputType = node;
			Field = fieldInfo.Name;
			_field = fieldInfo;
		}
		public ScriptNodeWriteField() {

		}
	}
}
