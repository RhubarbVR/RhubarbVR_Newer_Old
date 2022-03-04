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
			else if(type == typeof(int)) {
				editor = child.AttachComponent<IntEditor>();
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
