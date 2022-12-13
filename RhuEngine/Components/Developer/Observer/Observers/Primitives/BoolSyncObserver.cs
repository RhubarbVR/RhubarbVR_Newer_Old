using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System.Reflection;
using RhuEngine.Commads;
using System;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/Primitives" })]
	[GenericTypeConstraint(TypeConstGroups.Serializable)]
	public class BoolSyncObserver : EditingField<Sync<bool>>
	{
		public readonly SyncRef<CheckBox> TargetCheckBox;
		public readonly Linker<bool> Linker;
		protected override Task LoadEditor(UIBuilder2D ui) {
			ui.PushElement<UIElement>();
			var check = ui.PushElement<CheckBox>();
			check.Max.Value = new Vector2f(0, 1);
			check.MinSize.Value = new Vector2i(ELMENTHIGHTSIZE, ELMENTHIGHTSIZE);
			check.Toggled.Target = ValueUpdated;
			Linker.Target = check.ButtonPressed;
			TargetCheckBox.Target = check;
			ui.Pop();
			ui.Pop();
			return Task.CompletedTask;
		}

		[Exposed]
		public void ValueUpdated(bool value) {
			if (!ValueLoadedIn) {
				return;
			}
			try {
				if (Linker.Linked) {
					if (TargetCheckBox.Target?.ButtonPressed.IsLinkedTo ?? false) {
						try {
							TargetElement.Value = TargetCheckBox.Target.ButtonPressed.Value;
						}
						catch { }
					}
				}
			}
			catch {

			}
			LoadValueIn();
		}

		protected bool ValueLoadedIn = false;

		protected override void LoadValueIn() {
			if (Linker.Linked) {
				if (TargetCheckBox.Target?.ButtonPressed.IsLinkedTo ?? false) {
					try {
						TargetCheckBox.Target.ButtonPressed.Value = TargetElement.Value;
						ValueLoadedIn = true;
					}
					catch { }
				}
			}
		}
	}
}