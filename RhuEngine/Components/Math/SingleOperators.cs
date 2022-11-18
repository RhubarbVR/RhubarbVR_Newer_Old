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
					return (!(RDynamic<In>)a).CastTo<Out>();
				case SingleOperators.BitwiseComplement:
					return (~(RDynamic<In>)a).CastTo<Out>();
				case SingleOperators.UnaryPlus:
					return (+(RDynamic<In>)a).CastTo<Out>();
				case SingleOperators.UnaryNegation:
					return (-(RDynamic<In>)a).CastTo<Out>();
				case SingleOperators.Increment:
					var data = (RDynamic<In>)a;
					data++;
					return data.CastTo<Out>();
				case SingleOperators.Decrement:
					var deata = (RDynamic<In>)a;
					deata--;
					return deata.CastTo<Out>();
				default:
					break;
			}
			return default;
		}
	}
}
