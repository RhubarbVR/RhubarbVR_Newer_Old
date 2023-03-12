using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using CategoryAttribute = RhuEngine.WorldObjects.ECS.CategoryAttribute;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors/Primitives" })]
	public sealed partial class TypeEditor : BasePrimitive<ISync, Type>
	{
		[OnChanged(nameof(ValueChange))]
		public readonly Linker<string> LineEditorValue;

		public override void ValueChange() {
			if (LineEditorValue.Linked) {
				try {
					LineEditorValue.LinkedValue = GetCastedValue()?.GetFormattedName();
				}
				catch { }
			}

		}

		[Exposed]
		public void TextSubmitted() {
			if (Target is null) {
				return;
			}
			if (LineEditorValue.Linked) {
				try {
					SetCastedValue(FamcyTypeParser.PraseType(LineEditorValue.LinkedValue));
				}
				catch { }
			}
		}

		protected override void BuildUI() {
			var MainBox = Entity.AttachComponent<BoxContainer>();
			MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			if (TargetField.Value is not null) {
				var textLabel = MainBox.Entity.AddChild("Label").AttachComponent<TextLabel>();
				textLabel.Text.Value = TargetField.Value?.Split('.')?.LastOrDefault();
				textLabel.TextSize.Value = 16;
			}
			var e = MainBox.Entity.AddChild("LineEdit").AttachComponent<LineEdit>();
			e.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			e.TextSubmitted.Target = TextSubmitted;
			e.Text.Value = "ERROR";
			LineEditorValue.Target = e.Text;
			var value = TargetObject.Target.GetValueType();
			var nullButton = MainBox.Entity.AddChild("Null").AttachComponent<Button>();
			nullButton.Pressed.Target = SetNull;
			nullButton.Text.Value = "∅";
			nullButton.MinSize.Value = new Vector2i(18);

		}
	}
}