using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	//Todo no more dynamic use interfaces instead
	[Category("Math")]
	public sealed class MultiOperators<In,Out> : AbMultiOperation<Out,In> where In : struct where Out : struct
	{
		public readonly Sync<bool> Unchecked;

		[OnChanged(nameof(ComputeOutput))]
		public readonly Sync<MultiOperators> Operators;

		public override Out Compute(In[] ins) {
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
				return (dynamic)temp;
			}
			catch {
				return default;
			}
		}

		public In Operation(In a, In b) {
			switch (Operators.Value) {
				case MultiOperators.Addition:
					return ((dynamic)a) + ((dynamic)b);
				case MultiOperators.Subtraction:
					return ((dynamic)a) - ((dynamic)b);
				case MultiOperators.Multiplication:
					return ((dynamic)a) * ((dynamic)b);
				case MultiOperators.Division:
					return ((dynamic)a) / ((dynamic)b);
				case MultiOperators.Modulus:
					return ((dynamic)a) % ((dynamic)b);
				case MultiOperators.ConditionallogicalAND:
					return ((dynamic)a) && ((dynamic)b);
				case MultiOperators.ConditionallogicalOR:
					return ((dynamic)a) || ((dynamic)b);
				case MultiOperators.LeftShift:
					return ((dynamic)a) << ((dynamic)b);
				case MultiOperators.RightShift:
					return ((dynamic)a) >> ((dynamic)b);
				case MultiOperators.LogicalOR:
					return ((dynamic)a) | ((dynamic)b);
				case MultiOperators.LogicalAND:
					return ((dynamic)a) & ((dynamic)b);
				case MultiOperators.LogicalExclusiveOR:
					return ((dynamic)a) ^ ((dynamic)b);
				default:
					break;
			}
			return default;
		}
	}
}
