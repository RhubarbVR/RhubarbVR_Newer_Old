using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class LocomotionManager : Component
	{
		public SyncRef<User> user;

		public Sync<int> selectedModule;

		public SyncObjList<SyncRef<ILocomotionModule>> modules;

		public override void OnAttach() {
			modules.Add().Target = Entity.AttachComponent<NoClipLocomotion>();
		}

		public ILocomotionModule CurrentLocomotionModule => selectedModule.Value > (modules.Count - 1) ? null : modules[selectedModule.Value].Target;

		public override void Step() {
			if (user.Target is null) {
				return;
			}
			if (user.Target == World.GetLocalUser()) {
				var headMovement = true;
				var locModule = CurrentLocomotionModule;
				if (locModule is null) {
					return;
				}
				// Todo: this is very broken needs to be redone
				var headDir = Vec3.Zero;
				headDir += Input.Key(Key.W).IsActive() & !Input.Key(Key.Shift).IsActive() ? Vec3.Forward : Vec3.Zero;
				headDir += Input.Key(Key.S).IsActive() & !Input.Key(Key.Shift).IsActive() ? -Vec3.Forward : Vec3.Zero;
				headDir += Input.Key(Key.A).IsActive() & !Input.Key(Key.Shift).IsActive() ? Vec3.Right : Vec3.Zero;
				headDir += Input.Key(Key.D).IsActive() & !Input.Key(Key.Shift).IsActive() ? -Vec3.Right : Vec3.Zero;
				headDir += Input.Key(Key.Q).IsActive() & !Input.Key(Key.Shift).IsActive() ? -Vec3.Up : Vec3.Zero;
				headDir += Input.Key(Key.E).IsActive() & !Input.Key(Key.Shift).IsActive() ? Vec3.Up : Vec3.Zero;
				if (headMovement) {
					headDir += (Input.Controller(Handed.Left).stick.X0Y * new Vec3(-1, 0, -1)) + (Vec3.Forward * Input.Controller(Handed.Right).stick.y);
				}
				var headLocal = user.Target.userRoot.Target.head.Target.GlobalTrans.GetLocal(user.Target.userRoot.Target.Entity.parent.Target.GlobalTrans);
				var moveVec = Quat.FromAngles(0f, 0f, 180f) * headLocal.Rotation * headDir;
				if (!headMovement) {
					moveVec += Quat.FromAngles(0f, 0f, 180f) * Input.Controller(Handed.Left).pose.ToMatrix().GetLocal(user.Target.userRoot.Target.Entity.parent.Target.GlobalTrans).Rotation * Input.Controller(Handed.Left).stick.X0Y;
					moveVec += Quat.FromAngles(0f, 0f, 180f) * Input.Controller(Handed.Right).pose.ToMatrix().GetLocal(user.Target.userRoot.Target.Entity.parent.Target.GlobalTrans).Rotation * (Vec3.UnitZ * Input.Controller(Handed.Right).stick.y);
				}
				locModule.ProcessMovement(moveVec * Time.Elapsedf, (Input.Controller(Handed.Right).stick.x + (Input.Key(Key.Z).IsActive() ? 1 : 0) + (Input.Key(Key.X).IsActive() ? -1 : 0)) * Time.Elapsedf, user.Target.userRoot.Target, Input.Key(Key.Ctrl).IsActive() | (Input.Controller(Handed.Left).trigger > 0.9f) | (Input.Controller(Handed.Right).trigger > 0.9f));
			}
		}

	}
}
