using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	public sealed partial class ValueCopyTernary<T> : Component
	{
		[OnChanged(nameof(OnChanged))]
		public readonly Linker<T> Target;

		[OnChanged(nameof(OnChanged))]
		public readonly SyncRef<IValueSource<T>> True;

		[OnChanged(nameof(OnChanged))]
		public readonly SyncRef<IValueSource<T>> False;

		[OnChanged(nameof(OnChanged))]
		public readonly SyncRef<IValueSource<bool>> Condition;

		public readonly Sync<bool> WriteBack;

		private IChangeable _linkedSourceTrue;

		private IChangeable _linkedSourceFalse;

		private IChangeable _linkedSourceCondition;

		private IChangeable _linkedTarget;

		public SyncRef<IValueSource<T>> Source => (Condition.Target?.Value ?? false) ? True : False;

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
			if (_linkedSourceTrue != null) {
				_linkedSourceTrue.Changed -= SourceChange;
			}
			if (_linkedSourceFalse != null) {
				_linkedSourceFalse.Changed -= SourceChange;
			}
			if(_linkedSourceCondition != null) {
				_linkedSourceCondition.Changed -= SourceChange;
			}
			if (_linkedTarget != null) {
				_linkedTarget.Changed -= TargetChange;
			}
			_linkedSourceTrue = True.Target;
			_linkedSourceFalse = True.Target;
			_linkedTarget = Target.Target;
			_linkedSourceCondition = Condition.Target;
			if (_linkedSourceCondition is not null) {
				_linkedSourceCondition.Changed += SourceChange;
			}
			if (_linkedSourceTrue is not null) {
				_linkedSourceTrue.Changed += SourceChange;
			}
			if (_linkedSourceFalse is not null) {
				_linkedSourceFalse.Changed += SourceChange;
			}
			if (_linkedTarget is not null) {
				_linkedTarget.Changed += TargetChange;
			}
			SourceChange(null);
		}
	}
}
