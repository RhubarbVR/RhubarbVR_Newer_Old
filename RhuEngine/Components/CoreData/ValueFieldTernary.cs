using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "CoreData" })]
	[GenericTypeConstraint()]
	public sealed partial class ValueFieldTernary<T> : Component
	{
		[OnChanged(nameof(ConditionChange))]
		public readonly Linker<T> Target;

		[OnChanged(nameof(ConditionChange))]
		public readonly Sync<T> True;

		[OnChanged(nameof(ConditionChange))]
		public readonly Sync<T> False;

		[OnChanged(nameof(ConditionChange))]
		public readonly Sync<bool> Condition;

		public void ConditionChange(IChangeable val) {
			if (Target.Linked) {
				Target.LinkedValue = Condition.Value ? True.Value : False.Value;
			}
		}
	}
}
