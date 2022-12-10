using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Math" })]
	public sealed class SmoothLerp<T> : Component
	{
		public readonly Linker<T> driver;
		public readonly Sync<T> to;
		public readonly Sync<double> Multiply;
		
		[Exposed]
		public void StartSmoothLerp(ILinkerMember<T> target,T targetpos,double Multiplyf) {
			driver.SetLinkerTarget(target);
			to.Value = targetpos;
			Multiply.Value = Multiplyf;
		}

		protected override void Step() {
			if (driver.Linked) {
				try {
					driver.LinkedValue = typeof(T) == typeof(Quaternionf)
						? (T)Quaternionf.Slerp((dynamic)driver.LinkedValue, (dynamic)to.Value, (float)(Multiply.Value * RTime.Elapsed))
						: typeof(T) == typeof(Quaterniond)
							? (T)Quaterniond.Slerp((dynamic)driver.LinkedValue, (dynamic)to.Value, Multiply.Value * RTime.Elapsed)
							: MathUtil.DynamicLerp(driver.LinkedValue, to.Value, Multiply.Value * RTime.Elapsed);
				}
				catch {
					driver.LinkedValue = default;
				}
			}
		}
	}
}
