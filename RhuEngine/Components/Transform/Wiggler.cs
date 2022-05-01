using System;
using System.Collections.Generic;
using System.Text;

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

				var noiseVector = new Vector3f(
					SimplexNoise.Generate((time * speed.Value.x) + seed.Value.x) * magnitude.Value.x,
					SimplexNoise.Generate((time * speed.Value.y) + seed.Value.y) * magnitude.Value.y,
					SimplexNoise.Generate((time * speed.Value.z) + seed.Value.z) * magnitude.Value.z
				);

				var nextQ = offset * Quaternionf.CreateFromEuler(noiseVector);

				driver.LinkedValue = nextQ;
			}
		}
	}
}
