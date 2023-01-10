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
	public class CompoentInspector : BaseInspector<IComponent>
	{
		protected override void BuildUI() {
			var dropDown = Entity.AttachComponent<DropDown>();
			try {
				dropDown.DropDownButton.Target.Text.Value = TargetObject.Target?.GetType().GetFormattedName();
				var delete = dropDown.DropDownHeader.Target.AddChild("Delete").AttachComponent<Button>();
				delete.Alignment.Value = RButtonAlignment.Center;
				delete.Text.Value = "X";
				delete.MinSize.Value = new Vector2i(32);
				delete.Pressed.Target = TargetObject.Target.Destroy;
				dropDown.DropDownData.Target.AttachComponent<WorldObjectInspector>().TargetObject.Target = TargetObject.Target;
			}
			catch {

			}
		}
	}
}