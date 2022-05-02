using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components.Transform
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Wobbler3D : Component
	{
		public readonly Linker<Vector3f> driver;
		public readonly Sync<Vector3f> speed;
		public readonly Sync<Vector3f> magnitude;
		public readonly Sync<Vector3f> seed;
		public readonly Sync<Vector3f> offset;

		public override void Step() {
			if (driver.Linked) {
				var time = (float)World.WorldTime;
				var noiseValue = SimplexNoise.Generate3D(time, speed.Value, magnitude.Value, seed.Value);
				driver.LinkedValue = offset.Value + noiseValue;
			}
		}
	}
}
