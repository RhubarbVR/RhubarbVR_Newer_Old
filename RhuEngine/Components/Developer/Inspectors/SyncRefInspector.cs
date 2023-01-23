using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using RhuEngine.Components.UI;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Inspectors" })]
	public class SyncRefInspector<T, T2> : BaseInspector<T> where T : class, ISyncRef where T2 : class, IWorldObject
	{
		[Exposed]
		public void SetNull() {
			if (TargetObject.Target is not null) {
				TargetObject.Target.TargetIWorldObject = null;
			}
		}

		[Exposed]
		public void DropRef(T2 setValue) {
			if (TargetObject.Target is not null) {
				TargetObject.Target.TargetIWorldObject = setValue;
			}
		}

		public override void LocalBind() {
			if (TargetObject.Target is not null) {
				TargetObject.Target.Changed += Target_Changed;
				Target_Changed(null);
			}
		}

		private void Target_Changed(IChangeable obj) {
			if (TargetButton.Target is not null) {
				TargetButton.Target.Text.Value = TargetObject.Target?.TargetIWorldObject?.GetExtendedNameStringWithRef() ?? "NULL";
			}
		}


		[OnChanged(nameof(Target_Changed))]
		public readonly SyncRef<Button> TargetButton;

		[Exposed]
		public void GetRef() {
			if (TargetButton.Target is not null) {
				try {
					PrivateSpaceManager.GetGrabbableHolder(TargetButton.Target.LastHanded).GetGrabbableHolderFromWorld(World).Referencer.Target = TargetObject.Target?.TargetIWorldObject;
				}
				catch {
				}
			}
		}

		protected override void BuildUI() {
			var MainBox = Entity.AttachComponent<BoxContainer>();
			MainBox.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			var e = MainBox.Entity.AddChild().AttachComponent<Button>();
			e.ButtonMask.Value = RButtonMask.Secondary;
			TargetButton.Target = e;
			var uiElem= e.Entity.AddChild().AttachComponent<UIElement>();
			uiElem.InputFilter.Value = RInputFilter.Pass;
			var refer = uiElem.Entity.AttachComponent<ReferenceAccepter<T2>>();
			refer.TryAndFind.Value = false;
			refer.Dropped.Target = DropRef;
			e.HorizontalFilling.Value = RFilling.Expand | RFilling.Fill;
			e.ActionMode.Value = RButtonActionMode.Press;
			e.ButtonDown.Target = GetRef;
			var nullButton = MainBox.Entity.AddChild("Null").AttachComponent<Button>();
			nullButton.Pressed.Target = SetNull;
			nullButton.Text.Value = "∅";
			nullButton.MinSize.Value = new Vector2i(18);
		}
	}
}