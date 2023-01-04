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
	public sealed class QuaternionEditor : BasePrimitive<Sync<Quaternionf>, Quaternionf>
	{

		[OnChanged(nameof(ValueChange))]
		public readonly Linker<string> yawEdit;

		[OnChanged(nameof(ValueChange))]
		public readonly Linker<string> pitchEdit;

		[OnChanged(nameof(ValueChange))]
		public readonly Linker<string> rollEdit;

		public override void ValueChange() {
			var euler = GetCastedValue().GetEuler();
			if (yawEdit.Linked) {
				yawEdit.LinkedValue = euler.x.ToString();
			}
			if (pitchEdit.Linked) {
				pitchEdit.LinkedValue = euler.y.ToString();
			}
			if (rollEdit.Linked) {
				rollEdit.LinkedValue = euler.z.ToString();
			}
		}
		[Exposed]
		public void TextEdited() {
			if (yawEdit.Linked && pitchEdit.Linked && rollEdit.Linked) {
				try {
					var converter = TypeDescriptor.GetConverter(typeof(float));
					var x = (float)converter.ConvertFrom(yawEdit.LinkedValue);
					var y = (float)converter.ConvertFrom(pitchEdit.LinkedValue);
					var z = (float)converter.ConvertFrom(rollEdit.LinkedValue);
					SetValue(Quaternionf.CreateFromEuler(x, y, z));
				}
				catch {
					ValueChange();
				}

			}
		}

		protected override void BuildUI() {
			var MainBox = Entity.AttachComponent<BoxContainer>();
			MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			Sync<string> AddEditField(string label) {
				var newainBox = Entity.AddChild(label).AttachComponent<BoxContainer>();
				newainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
				var textLabel = newainBox.Entity.AddChild("Label").AttachComponent<TextLabel>();
				textLabel.Text.Value = label;
				textLabel.TextSize.Value = 16;

				var e = newainBox.Entity.AddChild("LineEdit").AttachComponent<LineEdit>();
				e.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

				var fe = e.Entity.AttachComponent<UIFocusEvents>();
				fe.FocusExited.Target = TextEdited;
				e.Text.Value = "ERROR";
				return e.Text;
			}
			pitchEdit.Target = AddEditField("X");
			yawEdit.Target = AddEditField("Y");
			rollEdit.Target = AddEditField("Z");
		}
	}
}