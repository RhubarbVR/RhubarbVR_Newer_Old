using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;
using RNumerics.Noise;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Wiggler: Component
	{
		public readonly Linker<Quaternionf> driver;

		public readonly Sync<Vector3f> speed;
		public readonly Sync<Vector3f> magnitude;
		public readonly Sync<Vector3f> seed;
		public readonly Sync<Quaternionf> offset;

		protected override void OnInitialize() {
			base.OnInitialize();
			offset.Value = Quaternionf.Identity;
		}

		protected override void RenderStep() {
			if (driver.Linked) {
				var time = (float)World.WorldTime;
				var noiseValue = SimplexNoise.Generate3D(time, speed.Value, magnitude.Value, seed.Value);
				driver.LinkedValue = offset * Quaternionf.CreateFromEuler(noiseValue);
			}
		}
	}
}
