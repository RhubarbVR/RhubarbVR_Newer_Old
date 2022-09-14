using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics.Noise;

namespace RhuEngine.Components
{
	[GenericTypeConstraint()]
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Math" })]
	public sealed class Wobbler<T> : Component where T : struct
	{
		public readonly Linker<T> driver;
		public readonly Sync<T> speed;
		public readonly Sync<T> magnitude;
		public readonly Sync<T> seed;
		public readonly Sync<T> offset;

		protected override void Step() {
			if (driver.Linked) {
				try {
					var time = (float)World.WorldTime;
					var noiseValue = SimplexNoise.Generate(time, speed.Value, magnitude.Value, seed.Value);
					driver.LinkedValue = (dynamic)offset.Value + noiseValue;
				}
				catch {
					driver.LinkedValue = default;
				}
			}
		}
	}
}
