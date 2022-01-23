using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class FollowUser : Component
	{
		public Linker<Quat> driver;

		public Sync<Quat> offset;

		public Linker<Vec3> posDriver;

		public Sync<Vec3> posOffset;

		public SyncRef<User> user;

		[Default(TrackPos.Root)]
		public Sync<TrackPos> pos;

		public enum TrackPos
		{
			None,
			Root,
			Head,
			LeftHand,
			RightHand
		}

		public override void OnAttach() {
			base.OnAttach();
			user.Target = World.GetLocalUser();
			offset.Value = Entity.rotation.Value;
			driver.SetLinkerTarget(Entity.rotation);

			posOffset.Value = Entity.position.Value;
			posDriver.SetLinkerTarget(Entity.position);
		}

		public override void Step() {
			if (user.Target is null) {
				return;
			}
			if (driver.Linked) {
				var fromPos = Matrix.Identity;
				switch (pos.Value) {
					case TrackPos.None:
						break;
					case TrackPos.Root:
						fromPos = user.Target.userRoot.Target?.Entity.GlobalTrans ?? fromPos;
						break;
					case TrackPos.Head:
						fromPos = user.Target.userRoot.Target?.head.Target?.GlobalTrans ?? fromPos;
						break;
					case TrackPos.LeftHand:
						fromPos = user.Target.userRoot.Target?.leftHand.Target?.GlobalTrans ?? fromPos;
						break;
					case TrackPos.RightHand:
						fromPos = user.Target.userRoot.Target?.rightHand.Target?.GlobalTrans ?? fromPos;
						break;
					default:
						break;
				}
				var newval = Matrix.TR(Entity.GlobalPointToLocal(fromPos.Translation, false), Entity.GlobalRotToLocal(fromPos.Rotation, false));
				newval.Decompose(out var trans, out var newRotation, out _);
				posDriver.LinkedValue = trans;
				driver.LinkedValue = newRotation;
			}
		}
	}
}
