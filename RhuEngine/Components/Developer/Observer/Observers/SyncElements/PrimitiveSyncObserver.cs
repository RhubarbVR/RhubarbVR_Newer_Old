using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/SyncElements" })]
	public class PrimitiveSyncObserver : ObserverSyncElement<ISync>
	{
		public readonly SyncRef<ILinkerMember<string>> editorValue;
		IChangeable _changeable;
		protected override void LoadSideUI(UIBuilder ui) {
			var wa = ui.AddTextEditor("null", 0.1f, 1, "", 0.1f, null, 2f);
			editorValue.Target = wa.Item4;
			wa.Item2.OnDoneEditing.Target += OnDoneEditing;
			if (TargetElement is IChangeable changeable) {
				if (_changeable is not null) {
					changeable.Changed -= Changeable_Changed;
					_changeable = null;
				}
				changeable.Changed += Changeable_Changed;
				Changeable_Changed(null);
			}
		}

		private void Changeable_Changed(IChangeable obj) {
			if (TargetElement is null) {
				return;
			}
			if (editorValue.Target is not null) {
				editorValue.Target.Value = TargetElement.GetValue()?.ToString();
			}
		}

		[Exposed]
		private void OnDoneEditing() {
			if (TargetElement is null) {
				return;
			}
			if (editorValue.Target is null) {
				return;
			}
			try {
				var data = Convert.ChangeType(editorValue.Target.Value, TargetElement.GetValue()?.GetType());
				TargetElement.SetValue(data);
			}
			catch {

			}
			Changeable_Changed(null);
		}
	}
}
