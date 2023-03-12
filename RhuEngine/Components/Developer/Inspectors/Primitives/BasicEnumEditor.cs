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
	public sealed partial class BasicEnumEditor<T> : BasePrimitive<Sync<T>, T> where T : Enum
	{
		public Array enums;

		[OnChanged(nameof(ValueChange))]
		public readonly Linker<int> TargetID;

		public override void ValueChange() {
			if (TargetID.Linked) {
				TargetID.LinkedValue = Array.IndexOf(enums, GetCastedValue());
			}

		}
		[Exposed]
		public void ToggleStateChange() {
			if (TargetID.Linked) {
				SetCastedValue((T)enums.GetValue(TargetID.LinkedValue));
			}
		}

		protected override void BuildUI() {
			enums = Enum.GetValues(typeof(T));
			var MainBox = Entity.AttachComponent<BoxContainer>();
			MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var e = MainBox.Entity.AddChild("DropDown").AttachComponent<OptionButton>();
			e.Alignment.Value = RButtonAlignment.Center;
			for (var i = 0; i < enums.Length; i++) {
				var newButton = e.Items.Add();
				newButton.Id.Value = i;
				newButton.Text.Value = enums.GetValue(i)?.ToString() ?? "NULL";
			}
			e.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			TargetID.Target = e.Selected;
			e.Pressed.Target = ToggleStateChange;
			e.Text.Value = TargetField.Value?.Split('.')?.LastOrDefault();
		}
	}
}