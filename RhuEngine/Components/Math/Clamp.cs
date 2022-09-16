using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category("Math")]
	public sealed class Clamp<T> : Component where T : struct
	{
		public readonly Linker<T> Output;

		[OnChanged(nameof(BindOne))]
		public readonly SyncRef<IValueSource<T>> Min;

		[OnChanged(nameof(BindTwo))]
		public readonly SyncRef<IValueSource<T>> Max;

		[OnChanged(nameof(BindThree))]
		public readonly SyncRef<IValueSource<T>> Value;

		IValueSource<T> _lastInputOne;
		IValueSource<T> _lastInputTwo;
		IValueSource<T> _lastInputThree;

		private void BindOne() {
			if (_lastInputOne is not null) {
				_lastInputOne.Changed -= Compute;
			}
			if (Min.Target is not null) {
				_lastInputOne.Changed += Compute;
				Compute(null);
			}
			_lastInputOne = Min.Target;
		}

		private void BindTwo() {
			if (_lastInputTwo is not null) {
				_lastInputTwo.Changed -= Compute;
			}
			if (Max.Target is not null) {
				_lastInputTwo.Changed += Compute;
				Compute(null);
			}
			_lastInputTwo = Max.Target;
		}

		private void BindThree() {
			if (_lastInputThree is not null) {
				_lastInputThree.Changed -= Compute;
			}
			if (Value.Target is not null) {
				_lastInputThree.Changed += Compute;
				Compute(null);
			}
			_lastInputThree = Value.Target;
		}
		public void ComputeOutput() {
			Compute(null);
		}

		private void Compute(IChangeable changeable) {
			if (Output.Linked) {
				var a = Min.Target?.Value ?? default;
				var b = Max.Target?.Value ?? default;
				var c = Value.Target?.Value ?? default;
				try {
					Output.LinkedValue = MathUtil.Clamp(c, a, b);
				}
				catch {
					Output.LinkedValue = c;
				}
			}
		}
	}
}
