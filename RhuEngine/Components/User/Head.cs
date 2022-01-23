using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class Head : Component
	{
		public SyncRef<User> user;

		public Linker<Vec3> pos;

		public Linker<Quat> rot;

		public override void OnAttach() {
			pos.SetLinkerTarget(Entity.position);
			rot.SetLinkerTarget(Entity.rotation);
		}

		public float headRotY;
		public float headRotX;

		public override void Step() {
			if (World.IsPersonalSpace) {
				var handVal = WorldManager.FocusedWorld?.GetLocalUser()?.userRoot.Target?.head.Target;
				if (handVal is not null) {
					var focusUserHand = WorldManager.FocusedWorld?.GetLocalUser()?.userRoot.Target.head.Target;
					Entity.LocalTrans = focusUserHand?.LocalTrans ?? Matrix.Identity;
				}
			}
			else {
				if (user.Target is null) {
					return;
				}
				if (user.Target == World.GetLocalUser()) {
					if (SK.ActiveDisplayMode == DisplayMode.Flatscreen & SK.Settings.disableFlatscreenMRSim) {
						var mousePos = Input.Mouse.posChange;
						headRotX += mousePos.y / 50;
						headRotY += mousePos.x / 50;
						Entity.LocalTrans = Matrix.TR(new Vec3(0, 0.75f, 0), Quat.FromAngles(headRotX, headRotY, 0));
						user.Target.FindOrCreateSyncStream<SyncValueStream<Vec3>>("HeadPos").Value = Entity.position.Value;
						user.Target.FindOrCreateSyncStream<SyncValueStream<Quat>>("HeadRot").Value = Entity.rotation.Value;
					}
					else {
						Entity.LocalTrans = Input.Head.ToMatrix(1) * Renderer.CameraRoot.Inverse;
						user.Target.FindOrCreateSyncStream<SyncValueStream<Vec3>>("HeadPos").Value = Entity.position.Value;
						user.Target.FindOrCreateSyncStream<SyncValueStream<Quat>>("HeadRot").Value = Entity.rotation.Value;
					}
				}
				else {
					var position = user.Target.FindSyncStream<SyncValueStream<Vec3>>("HeadPos")?.Value ?? Vec3.Zero;
					var rotation = user.Target.FindSyncStream<SyncValueStream<Quat>>("HeadRot")?.Value ?? Quat.Identity;
					Entity.LocalTrans = Matrix.TR(position, rotation);
				}
			}
		}
	}
}