using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics.Noise;

namespace RhuEngine.Components.Transform
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Wobbler1D : Component
	{
		public Linker<float> driver;
		public Sync<float> speed;
		public Sync<float> magnitude;
		public Sync<float> seed;
		public Sync<float> offset;

		public override void Step() {
			if (driver.Linked) {
				var time = (float)World.WorldTime;
				var noiseValue = SimplexNoise.Generate1D(time, speed.Value, magnitude.Value, seed.Value);
				driver.LinkedValue = offset.Value + noiseValue;
			}
		}
	}
}
