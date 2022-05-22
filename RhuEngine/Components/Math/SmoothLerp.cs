using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Math" })]
	public class SmoothLerp<T> : Component
	{
		public readonly Linker<T> driver;
		public readonly Sync<T> to;
		public readonly Sync<double> Multiply;
		
		[Exsposed]
		public void StartSmoothLerp(ILinkerMember<T> target,T targetpos,double Multiplyf) {
			driver.SetLinkerTarget(target);
			to.Value = targetpos;
			Multiply.Value = Multiplyf;
		}

		public override void Step() {
			if (driver.Linked) {
				try {
					driver.LinkedValue = MathUtil.DynamicLerp(driver.LinkedValue, to.Value, Multiply.Value * RTime.Elapsedf);
				}
				catch {
					if (typeof(T) == typeof(Quaternionf)) {
						driver.LinkedValue = Quaternionf.Slerp((dynamic)driver.LinkedValue, (dynamic)to.Value, (float)Multiply.Value * RTime.Elapsedf);
					}
					else if (typeof(T) == typeof(Quaterniond)) {
						driver.LinkedValue = Quaterniond.Slerp((dynamic)driver.LinkedValue, (dynamic)to.Value, Multiply.Value * RTime.Elapsedf);
					}
					else {
						driver.LinkedValue = default;
					}
				}
			}
		}
	}
}
