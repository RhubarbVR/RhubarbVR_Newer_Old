using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class UserRoot : Component
	{
		public SyncRef<User> user;

		public SyncRef<Entity> head;

		public SyncRef<Entity> leftHand;

		public SyncRef<Entity> rightHand;

		public Linker<Vec3> pos;

		public Linker<Quat> rot;

		public override void OnAttach() {
			pos.SetLinkerTarget(Entity.position);
			rot.SetLinkerTarget(Entity.rotation);
		}

		public override void Step() {
			if (user.Target is null) {
				return;
			}
			if (user.Target != World.GetLocalUser()) {
				var userPos = user.Target.FindSyncStream<SyncValueStream<Vec3>>("UserPos");
				var userRot = user.Target.FindSyncStream<SyncValueStream<Quat>>("UserRot");
				Entity.position.Value = userPos?.Value ?? Vec3.Zero;
				Entity.rotation.Value = userRot?.Value ?? Quat.Identity;
				return;
			}
			if (World.IsPersonalSpace) {
				if (WorldManager.FocusedWorld?.GetLocalUser()?.userRoot.Target is not null) {
					var focusUserRoot = WorldManager.FocusedWorld.GetLocalUser().userRoot.Target;
					Entity.GlobalTrans = focusUserRoot.Entity.GlobalTrans;
				}
				
				Renderer.CameraRoot = ((SK.ActiveDisplayMode == DisplayMode.Flatscreen & SK.Settings.disableFlatscreenMRSim) ? head.Target?.GlobalTrans ?? Matrix.S(1) : Entity.GlobalTrans) * Matrix.T(new Vec3(0, StereoKit.World.BoundsSize.y, 0));
			}
			else {
				user.Target.FindOrCreateSyncStream<SyncValueStream<Vec3>>("UserPos").Value = Entity.position.Value;
				user.Target.FindOrCreateSyncStream<SyncValueStream<Quat>>("UserRot").Value = Entity.rotation.Value;
			}
		}
	}
}