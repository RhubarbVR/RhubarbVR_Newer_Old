using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics.Noise;

namespace RhuEngine.Components.Transform
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Wobbler1D : Component
	{
		public readonly Linker<float> driver;
		public readonly Sync<float> speed;
		public readonly Sync<float> magnitude;
		public readonly Sync<float> seed;
		public readonly Sync<float> offset;

		public override void Step() {
			if (driver.Linked) {
				var time = (float)World.WorldTime;
				var noiseValue = SimplexNoise.Generate1D(time, speed.Value, magnitude.Value, seed.Value);
				driver.LinkedValue = offset.Value + noiseValue;
			}
		}
	}
}
