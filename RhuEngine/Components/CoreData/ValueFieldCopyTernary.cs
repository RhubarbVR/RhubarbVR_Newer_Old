using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	[GenericTypeConstraint()]
	public sealed partial class ValueFieldCopyTernary<T> : Component
	{
		[OnChanged(nameof(ConditionChange))]
		public readonly Linker<T> Target;

		[OnChanged(nameof(ConditionChange))]
		public readonly Sync<T> True;

		[OnChanged(nameof(ConditionChange))]
		public readonly Sync<T> False;

		[OnChanged(nameof(OnChanged))]
		public readonly SyncRef<IValueSource<bool>> Condition;

		private IChangeable _linkedSource;

		public void OnChanged() {
			if (_linkedSource != null) {
				_linkedSource.Changed -= ConditionChange;
			}
			_linkedSource = Condition.Target;
			if (_linkedSource is not null) {
				_linkedSource.Changed += ConditionChange;
			}
			ConditionChange(null);
		}

		public void ConditionChange(IChangeable val) {
			if (Target.Linked) {
				Target.LinkedValue = (Condition.Target?.Value ?? false) ? True.Value : False.Value;
			}
		}
	}
}
