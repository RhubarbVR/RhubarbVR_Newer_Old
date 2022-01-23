using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class Spinner : Component
	{
		public Linker<Quat> driver;

		public Sync<Vec3> speed;

		public Sync<Quat> offset;

		public override void OnAttach() {
			base.OnAttach();
			offset.Value = Entity.rotation.Value;
			driver.SetLinkerTarget(Entity.rotation);
			speed.Value = new Vec3(35, 35, 0);
		}

		public override void Step() {
			var deltaSeconds = Time.Elapsedf;
			if (driver.Linked) {
				var newval = Entity.LocalTrans * Matrix.R(offset.Value) * Matrix.R(Quat.FromAngles(speed.Value.x * deltaSeconds, speed.Value.y * deltaSeconds, speed.Value.z * deltaSeconds));
				newval.Decompose(out _, out var newrotation, out _);
				driver.LinkedValue = newrotation;
			}
			else {
				driver.Target = Entity.rotation;
			}
		}
	}
}
