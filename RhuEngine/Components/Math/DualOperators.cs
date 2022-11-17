using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category("Math")]
	public sealed class DualOperators<In> : AbDualOperation<bool, In> where In : struct
	{
		[OnChanged(nameof(ComputeOutput))]
		public readonly Sync<DualOperators> Operators;

		protected override bool Compute(In a, In b) {
			switch (Operators.Value) {
				case DualOperators.Equal:
					return ((RDynamic<In>)a) == ((RDynamic<In>)b);
				case DualOperators.NotEqual:
					return ((RDynamic<In>)a) != ((RDynamic<In>)b);
				case DualOperators.LessThan:
					return ((RDynamic<In>)a) < ((RDynamic<In>)b);
				case DualOperators.GreaterThan:
					return ((RDynamic<In>)a) > ((RDynamic<In>)b);
				case DualOperators.LessThanOrEqual:
					return ((RDynamic<In>)a) <= ((RDynamic<In>)b);
				case DualOperators.GreaterThanOrEqual:
					return ((RDynamic<In>)a) >= ((RDynamic<In>)b);
				default:
					break;
			}
			return default;
		}
	}
}
