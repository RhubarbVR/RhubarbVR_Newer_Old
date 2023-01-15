using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	public class ListElementInspector : BaseInspector<ISyncObject>
	{
		protected override void BuildUI() {
			var box = Entity.AttachComponent<BoxContainer>();
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			
			Entity.AddChild("InData").AttachComponent<IInspector>(GetFiled(TargetObject.Target.GetType())).TargetObjectWorld = TargetObject.Target;

			var deleteButton = Entity.AddChild("Delete").AttachComponent<Button>();
			deleteButton.Alignment.Value = RButtonAlignment.Center;
			deleteButton.Text.Value = "X";
			deleteButton.VerticalFilling.Value = RFilling.Fill | RFilling.Fill;
			deleteButton.MinSize.Value = new Vector2i(32);
			if (TargetObject.Target is not null) {
				deleteButton.Pressed.Target = TargetObject.Target.Destroy;
			}
		}
	}
}