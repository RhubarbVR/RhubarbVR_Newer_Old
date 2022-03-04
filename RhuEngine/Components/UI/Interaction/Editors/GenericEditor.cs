using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Editors" })]
	public class GenericEditor : UIGroup
	{
		[OnChanged(nameof(TypeChange))]
		public Sync<Type> Type;

		public DynamicLinker DynamicLinker;

		public SyncRef<Editor> Editor;

		public void TypeChange() {
			if(Editor.Target is not null) {
				Editor.Target.Entity.Destroy();
			}
			var type = Type.Value;
			var child = Entity.AddChild($"Editor{type.Name}");
			Editor editor;
			if (type == typeof(string)) {
				editor = child.AttachComponent<StringEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			if (type == typeof(Type)) {
				editor = child.AttachComponent<TypeEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(byte)) {
				editor = child.AttachComponent<ByteEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(sbyte)) {
				editor = child.AttachComponent<SByteEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(short)) {
				editor = child.AttachComponent<ShortEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(ushort)) {
				editor = child.AttachComponent<UShortEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(uint)) {
				editor = child.AttachComponent<UIntEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if(type == typeof(int)) {
				editor = child.AttachComponent<IntEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(long)) {
				editor = child.AttachComponent<LongEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else if (type == typeof(ulong)) {
				editor = child.AttachComponent<ULongEditor>();
				editor.SetValue.Target = SetValue;
				editor.GetValue.Target = GetValue;
			}
			else {
				Log.Err("Type Not Found for GenericEditor");
				child.Destroy();
				return;
			}
			Editor.Target = editor;
		}

		[Exsposed]
		public void SetValue(object value) {
			try {
				if (DynamicLinker.Linked) {
					DynamicLinker.LinkedValue = value;
				}
			}
			catch { }
		}
		[Exsposed]
		public object GetValue() {
			return DynamicLinker.Linked ? DynamicLinker.LinkedValue : null;
		}
	}
}
