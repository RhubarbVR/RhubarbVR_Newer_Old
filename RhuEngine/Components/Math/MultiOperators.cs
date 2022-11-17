using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category("Math")]
	public sealed class MultiOperators<In> : AbMultiOperation<In, In> where In : struct
	{
		public readonly Sync<bool> Unchecked;

		[OnChanged(nameof(ComputeOutput))]
		public readonly Sync<MultiOperators> Operators;

		public override In Compute(In[] ins) {
			try {
				var temp = ins[0];
				if (Unchecked) {
					unchecked {
						for (var i = 0; i < ins.Length - 1; i++) {
							temp = Operation(temp, ins[i + 1]);
						}
					}
				}
				else {
					for (var i = 0; i < ins.Length - 1; i++) {
						temp = Operation(temp, ins[i + 1]);
					}
				}
				return temp;
			}
			catch {
				return default;
			}
		}

		public In Operation(In a, In b) {
			switch (Operators.Value) {
				case MultiOperators.Addition:
					return ((RDynamic<In>)a) + ((RDynamic<In>)b);
				case MultiOperators.Subtraction:
					return ((RDynamic<In>)a) - ((RDynamic<In>)b);
				case MultiOperators.Multiplication:
					return ((RDynamic<In>)a) * ((RDynamic<In>)b);
				case MultiOperators.Division:
					return ((RDynamic<In>)a) / ((RDynamic<In>)b);
				case MultiOperators.Modulus:
					return ((RDynamic<In>)a) % ((RDynamic<In>)b);
				case MultiOperators.ConditionallogicalAND:
					return (In)(((dynamic)a) && ((dynamic)b));
				case MultiOperators.ConditionallogicalOR:
					return (In)(((dynamic)a) || ((dynamic)b));
				case MultiOperators.LeftShift:
					return ((RDynamic<In>)a) << ((RDynamic<In>)b);
				case MultiOperators.RightShift:
					return ((RDynamic<In>)a) >> ((RDynamic<In>)b);
				case MultiOperators.LogicalOR:
					return ((RDynamic<In>)a) | ((RDynamic<In>)b);
				case MultiOperators.LogicalAND:
					return ((RDynamic<In>)a) & ((RDynamic<In>)b);
				case MultiOperators.LogicalExclusiveOR:
					return ((RDynamic<In>)a) ^ ((RDynamic<In>)b);
				default:
					break;
			}
			return default;
		}
	}
}
