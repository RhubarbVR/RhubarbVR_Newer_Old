using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category("Math")]
	public sealed class SingleOperators<In,Out> : AbSingleOperation<Out,In> where In : struct where Out : struct
	{
		[OnChanged(nameof(ComputeOutput))]
		public readonly Sync<SingleOperators> Operators;

		protected override Out Compute(In a) {
			switch (Operators.Value) {
				case SingleOperators.Cast:
					return RDynamic<In>.CastTo<Out>(a);
				case SingleOperators.LogicalNegation:
					return !(dynamic)a;
				case SingleOperators.BitwiseComplement:
					return ~(dynamic)a;
				default:
					break;
			}
			return default;
		}
	}
}
