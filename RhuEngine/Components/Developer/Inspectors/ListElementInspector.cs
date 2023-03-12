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
	public sealed partial class ListElementInspector : BaseInspector<ISyncObject>
	{
		public readonly SyncRef<Button> TargetButton;

		public override void LocalBind() {
			base.LocalBind();
			if (TargetObject.Target is null) {
				return;
			}
			TargetObject.Target.NameChange += Target_NameChange;
			Target_NameChange(TargetObject.Target.Name);
		}

		private void Target_NameChange(string obj) {
			if (TargetButton.Target is not null) {
				TargetButton.Target.ToolTipText.Value = obj;
				TargetButton.Target.Text.Value = obj;
			}
		}

		[Exposed]
		public void GetRef() {
			if (TargetButton.Target is not null) {
				try {
					PrivateSpaceManager.GetGrabbableHolder(TargetButton.Target.LastHanded).GetGrabbableHolderFromWorld(World).Referencer.Target = TargetObject.Target;
				}
				catch {
				}
			}
		}

		protected override void BuildUI() {
			var box = Entity.AttachComponent<BoxContainer>();
			box.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;

			var fieldButton = Entity.AddChild("Field").AttachComponent<Button>();
			fieldButton.Alignment.Value = RButtonAlignment.Center;
			fieldButton.ActionMode.Value = RButtonActionMode.Press;
			fieldButton.ButtonMask.Value = RButtonMask.Secondary;
			fieldButton.ButtonDown.Target = GetRef;
			TargetButton.Target = fieldButton;
			fieldButton.Text.Value = TargetObject.Target.Name;
			Entity.AddChild("InData").AttachComponent<IInspector>(GetFiled(TargetObject.Target.GetType(), TargetObject.Target)).TargetObjectWorld = TargetObject.Target;

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