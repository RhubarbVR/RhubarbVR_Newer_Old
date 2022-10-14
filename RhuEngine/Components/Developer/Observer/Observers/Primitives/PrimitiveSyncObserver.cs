using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System.Reflection;
using RhuEngine.Commads;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/Primitives" })]
	public class PrimitiveSyncObserver : EditingField<ISync>
	{
		public readonly Linker<string> Linker;
		public readonly Linker<string> PlaceHolderText;

		public readonly SyncRef<LineEdit> Editor;

		protected override void LoadEditor(UIBuilder2D ui) {
			var isNUllable = TargetElement?.GetValueType()?.IsNullable() ?? false;
			var lineEditor = ui.PushElement<LineEdit>();
			lineEditor.ClearButtonEnabled.Value = isNUllable;
			PlaceHolderText.Target = lineEditor.PlaceholderText;
			Editor.Target = lineEditor;
			Linker.Target = lineEditor.Text;
			lineEditor.TextSubmitted.Target = TextSubmitted;
			ui.Pop();
			if (isNUllable) {
				var lablebutton = ui.PushElement<Button>();
				lablebutton.Pressed.Target = NullPress;
				lineEditor.MaxOffset.Value = new Vector2f(-40, 0);
				lablebutton.MinOffset.Value = new Vector2f(-35, 0);
				lablebutton.Min.Value = new Vector2f(1, 0);
				lablebutton.Text.Value = "🚫";
				ui.Pop();
			}
		}

		[Exposed]
		public void NullPress() {
			try {
				TargetElement.SetValue(null);
			}
			catch {

			}
			LoadValueIn();
		}


		[Exposed]
		public void TextSubmitted() {
			try {
				var data = Convert.ChangeType(Editor.Target?.Text.Value, TargetElement.GetValueType());
				TargetElement.SetValue(data);
			}
			catch {

			}
			LoadValueIn();
		}

		protected override void LoadValueIn() {
			if (Linker.Linked) {
				if (Editor.Target?.Text.IsLinkedTo ?? false) {
					var newValue = TargetElement?.GetValue()?.ToString();
					Editor.Target.Text.Value = newValue;
					if (newValue is null) {
						if (PlaceHolderText.Linked) {
							PlaceHolderText.LinkedValue = "NULL";
						}
					}
					else {
						if (PlaceHolderText.Linked) {
							PlaceHolderText.LinkedValue = "";
						}
					}
				}
			}
		}
	}
}
