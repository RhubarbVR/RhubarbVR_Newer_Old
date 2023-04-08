using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public sealed partial class Spinner : Component
	{
		public readonly Linker<Quaternionf> driver;

		public readonly Sync<Vector3f> speed;

		public readonly Sync<Quaternionf> offset;

		protected override void OnAttach() {
			base.OnAttach();
			offset.Value = Entity.rotation.Value;
			driver.SetLinkerTarget(Entity.rotation);
			speed.Value = new Vector3f(35, 35, 0);
		}

		protected override void RenderStep() {
			var time = World.WorldTime;
			if (driver.Linked) {
				var newval = Matrix.R(offset.Value) * Matrix.R((Quaternionf)Quaterniond.CreateFromEuler(speed.Value.x * time, speed.Value.y * time, speed.Value.z * time));
				newval.Decompose(out _, out var newrotation, out _);
				driver.LinkedValue = newrotation;
			}
		}
	}
}
