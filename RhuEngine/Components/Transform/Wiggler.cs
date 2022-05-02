using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components.Transform
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Wiggler: Component
	{
		public Linker<Quaternionf> driver;

		public Sync<Vector3f> speed;
		public Sync<Vector3f> magnitude;
		public Sync<Vector3f> seed;
		public Sync<Quaternionf> offset;

		public override void OnInitialize() {
			base.OnInitialize();
			offset.Value = Quaternionf.Identity;
		}

		public override void Step() {
			if (driver.Linked) {
				var time = (float)World.WorldTime;
				var noiseValue = SimplexNoise.Generate3D(time, speed.Value, magnitude.Value, seed.Value);
				driver.LinkedValue = offset * Quaternionf.CreateFromEuler(noiseValue);
			}
		}
	}
}
