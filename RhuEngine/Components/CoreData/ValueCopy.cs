using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	public sealed partial class ValueCopy<T> : Component
	{
		[OnChanged(nameof(OnChanged))]
		public readonly Linker<T> Target;

		[OnChanged(nameof(OnChanged))]
		public readonly SyncRef<IValueSource<T>> Source;

		public readonly Sync<bool> WriteBack;

		private IChangeable _linkedSource;

		private IChangeable _linkedTarget;

		public void SourceChange(IChangeable val) {
			if (Source.Target != null && Target.Linked) {
				Target.LinkedValue = Source.Target.Value;
			}
		}

		public void TargetChange(IChangeable val) {
			if (WriteBack.Value && Source.Target != null && Target.Linked) {
				Source.Target.Value = Target.LinkedValue;
			}
		}
		public void OnChanged() {
			if (_linkedSource != null) {
				_linkedSource.Changed -= SourceChange;
			}
			if (_linkedTarget != null) {
				_linkedTarget.Changed -= TargetChange;
			}
			_linkedSource = Source.Target;
			_linkedTarget = Target.Target;
			if (_linkedSource is not null) {
				_linkedSource.Changed += SourceChange;
			}
			if (_linkedTarget is not null) {
				_linkedTarget.Changed += TargetChange;
			}
			SourceChange(null);
		}
	}
}
