using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components.Transform
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Wobbler2D : Component
	{
		public readonly Linker<Vector2f> driver;
		public readonly Sync<Vector2f> speed;
		public readonly Sync<Vector2f> magnitude;
		public readonly Sync<Vector2f> seed;
		public readonly Sync<Vector2f> offset;

		public override void Step() {
			if (driver.Linked) {
				var time = (float)World.WorldTime;
				var noiseValue = SimplexNoise.Generate2D(time, speed.Value, magnitude.Value, seed.Value);
				driver.LinkedValue = offset.Value + noiseValue;
			}
		}
	}
}
