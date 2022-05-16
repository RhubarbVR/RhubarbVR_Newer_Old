using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category("Math")]
	public class DualOperators<In,Out> : AbDualOperation<Out,In> where In : struct where Out : struct
	{
		[OnChanged(nameof(ComputeOutput))]
		public readonly Sync<DualOperators> Operators;

		public override Out Compute(In a, In b) {
			switch (Operators.Value) {
				case DualOperators.Equal:
					return ((dynamic)a) == ((dynamic)b);
				case DualOperators.NotEqual:
					return ((dynamic)a) != ((dynamic)b);
				case DualOperators.LessThan:
					return ((dynamic)a) < ((dynamic)b);
				case DualOperators.GreaterThan:
					return ((dynamic)a) > ((dynamic)b);
				case DualOperators.LessThanOrEqual:
					return ((dynamic)a) <= ((dynamic)b);
				case DualOperators.GreaterThanOrEqual:
					return ((dynamic)a) >= ((dynamic)b);
				default:
					break;
			}
			return default;
		}
	}
}
