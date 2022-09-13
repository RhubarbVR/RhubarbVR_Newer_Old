using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	public sealed class ValueCopy<T> : Component
	{
		[OnChanged(nameof(OnChanged))]
		public readonly Linker<T> linkedLocation;

		[OnChanged(nameof(OnChanged))]
		public readonly SyncRef<IValueSource<T>> source;

		public readonly Sync<bool> writeBack;

		private IChangeable _linkedSource;

		private IChangeable _linkedTarget;

		public void SourceChange(IChangeable val) {
			if (source.Target != null && linkedLocation.Linked) {
				linkedLocation.LinkedValue = source.Target.Value;
			}
		}

		public void TargetChange(IChangeable val) {
			if (writeBack.Value && source.Target != null && linkedLocation.Linked) {
				source.Target.Value = linkedLocation.LinkedValue;
			}
		}
		public void OnChanged() {
			if (source.Target != null && linkedLocation.Linked) {
				if (_linkedSource != null) {
					_linkedTarget.Changed -= SourceChange;
				}
				if (_linkedTarget != null) {
					_linkedTarget.Changed -= TargetChange;
				}
				_linkedSource = source.Target;
				_linkedTarget = linkedLocation.Target;
				_linkedTarget.Changed += TargetChange;
				_linkedTarget.Changed += SourceChange;

			}
		}
	}
}
