using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using System;

namespace RhuEngine.Components
{
	[Category(new string[] { "Developer/Observer/Observers/SyncElements" })]
	public sealed class PrimitiveSyncObserver : ObserverSyncElement<ISync>
	{
		public readonly SyncRef<ILinkerMember<string>> editorValue;
		IChangeable _changeable;
		protected override void LoadSideUI(UIBuilder ui) {
			var type = TargetElement.GetValue()?.GetType();
			var canBeNull = !(type?.IsValueType ?? false);
			if (canBeNull) {
				ui.PushRectNoDepth();
				ui.SetOffsetMinMax(null,new Vector2f(-ELMENTHIGHTSIZE,0));
			}
			var wa = ui.AddTextEditor("null", 0.1f, 1, "", 0.1f, null, 2f);
			if (canBeNull) {
				ui.PopRect();
				ui.PushRectNoDepth(new Vector2f(1,0));
				ui.SetOffsetMinMax(new Vector2f(-ELMENTHIGHTSIZE, 0));
				ui.AddButtonEvent(NullButtonClick, null, null, false, 0.1f, 0.9f);
				ui.PushRect(null, null, 0.05f);
				ui.AddText("🚫", null, 2f);
				ui.PopRect();
				ui.PopRect();
				ui.PopRect();
			}
			editorValue.Target = wa.Item4;
			wa.Item2.OnDoneEditing.Target += OnDoneEditing;
		}

		protected override void EveryUserOnLoad() {
			if (TargetElement is IChangeable changeable) {
				if (_changeable is not null) {
					changeable.Changed -= Changeable_Changed;
					_changeable = null;
				}
				changeable.Changed += Changeable_Changed;
				Changeable_Changed(null);
			}
		}

		[Exposed]
		public void NullButtonClick() {
			try {
				TargetElement.SetValue(null);
			}catch {

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
		public void OnDoneEditing() {
			if (TargetElement is null) {
				return;
			}
			if (editorValue.Target is null) {
				return;
			}
			try {
				var data = Convert.ChangeType(editorValue.Target.Value, TargetElement.GetValueType());
				TargetElement.SetValue(data);
			}
			catch {

			}
			Changeable_Changed(null);
		}
	}
}
