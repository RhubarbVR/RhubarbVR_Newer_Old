using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "UI\\Interaction\\Editors" })]
	public class TypeEditor : Editor
	{
		public Linker<Type> Linker;

		[BindProperty(nameof(EditStringText))]
		public SyncProperty<string> EditString;

		public string EditStringText
		{
			get {
				try {
					return GetValue.Target is not null ? (string)GetValue.Target.Invoke() : Linker.Linked ? Linker.LinkedValue.FullName : string.Empty;
				}
				catch { return string.Empty; }
			}
			set {
				try {
					if (SetValue.Target is not null) {
						SetValue.Target.Invoke(Type.GetType(value));
					}
					else {
						if (Linker.Linked) {
							Linker.LinkedValue = Type.GetType(value);
						}
					}
				}
				catch { }
			}
		}

		public override void OnAttach() {
			base.OnAttach();
			var textInput = Entity.AttachComponent<UITextInput>();
			textInput.Value.Target = EditString;
			textInput.NullButton.Value = false;
		}
	}
}
