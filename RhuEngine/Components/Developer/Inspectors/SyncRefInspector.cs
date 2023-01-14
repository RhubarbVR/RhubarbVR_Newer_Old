using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	public class SyncRefInspector<T> : BaseInspector<T> where T : class, ISyncRef
	{
		[Exposed]
		public void SetNull() {
			if (TargetObject.Target is not null) {
				TargetObject.Target.TargetIWorldObject = null;
			}
		}

		protected override void BuildUI() {
			var MainBox = Entity.AttachComponent<BoxContainer>();
			MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var e = MainBox.Entity.AddChild().AttachComponent<Button>();
			e.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var nullButton = MainBox.Entity.AddChild("Null").AttachComponent<Button>();
			nullButton.Pressed.Target = SetNull;
			nullButton.Text.Value = "∅";
			nullButton.MinSize.Value = new Vector2i(18);
		}
	}
}