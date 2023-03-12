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
	public sealed partial class CheckBoxEditor : BasePrimitive<Sync<bool>, bool>
	{
		[OnChanged(nameof(ValueChange))]
		public readonly Linker<bool> LineEditorValue;

		public override void ValueChange() {
			if (LineEditorValue.Linked) {
				LineEditorValue.LinkedValue = GetCastedValue();
			}

		}
		[Exposed]
		public void ToggleStateChange() {
			if (LineEditorValue.Linked) {
				SetCastedValue(LineEditorValue.LinkedValue);
			}
		}

		protected override void BuildUI() {
			var MainBox = Entity.AttachComponent<BoxContainer>();
			MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var e = MainBox.Entity.AddChild("Check").AttachComponent<CheckBox>();
			e.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			LineEditorValue.Target = e.ButtonPressed;
			e.Pressed.Target = ToggleStateChange;
			e.Text.Value = TargetField.Value?.Split('.')?.LastOrDefault();
		}
	}
}