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
		public readonly SyncRef<Button> TargetButton; 

		[Exposed]
		public void GetRef()
		{
			if (TargetButton.Target is not null) {
				try {
					PrivateSpaceManager.GetGrabbableHolder(TargetButton.Target.LastHanded).GetGrabbableHolderFromWorld(World).Referencer.Target = TargetObject.Target;
				}
				catch {
				}
			}
		}

		protected override void BuildUI() {
			var dropDown = Entity.AttachComponent<DropDown>();
			var button = dropDown.DropDownButton.Target.Entity.AddChild().AttachComponent<Button>();
			button.InputFilter.Value = RInputFilter.Pass;
			button.ButtonMask.Value = RButtonMask.Secondary;
			button.ButtonDown.Target = GetRef;
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