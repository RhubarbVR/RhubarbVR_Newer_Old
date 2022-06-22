using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class UserRoot : Component
	{
		public readonly SyncRef<User> user;

		public readonly SyncRef<Entity> head;

		public readonly SyncRef<Entity> leftHand;

		public readonly SyncRef<Entity> rightHand;

		public readonly Linker<Vector3f> pos;

		public readonly Linker<Quaternionf> rot;

		public readonly Linker<Vector3f> scale;

		public override void OnAttach() {
			pos.SetLinkerTarget(Entity.position);
			rot.SetLinkerTarget(Entity.rotation);
			scale.SetLinkerTarget(Entity.scale);
		}

		public override void Step() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (user.Target is null) {
				return;
			}
			if (user.Target != World.GetLocalUser()) {
				var userScale = user.Target.FindSyncStream<SyncValueStream<Vector3f>>("UserScale");
				var userPos = user.Target.FindSyncStream<SyncValueStream<Vector3f>>("UserPos");
				var userRot = user.Target.FindSyncStream<SyncValueStream<Quaternionf>>("UserRot");
				Entity.position.Value = userPos?.Value ?? Vector3f.Zero;
				Entity.scale.Value = userScale?.Value ?? Vector3f.Zero;
				Entity.rotation.Value = userRot?.Value ?? Quaternionf.Identity;
				return;
			}
			if (World.IsPersonalSpace) {
				if (WorldManager.FocusedWorld?.GetLocalUser()?.userRoot.Target is not null) {
					RWorld.ExecuteOnStartOfFrame(() => {
						var focusUserRoot = WorldManager.FocusedWorld.GetLocalUser().userRoot.Target;
						Entity.GlobalTrans = focusUserRoot.Entity.GlobalTrans;
					});
				}
				if (Engine.EngineLink.CanRender) {
					if (RWorld.IsInVR) {
						RWorld.ExecuteOnEndOfFrame(() => RRenderer.CameraRoot = Entity.GlobalTrans);
					}
					else {
						RWorld.ExecuteOnEndOfFrame(() => Engine.inputManager.screenInput.CamPos = Entity.GlobalTrans);
					}
				}
			}
			else {
				user.Target.FindOrCreateSyncStream<SyncValueStream<Vector3f>>("UserScale").Value = Entity.scale.Value;
				user.Target.FindOrCreateSyncStream<SyncValueStream<Vector3f>>("UserPos").Value = Entity.position.Value;
				user.Target.FindOrCreateSyncStream<SyncValueStream<Quaternionf>>("UserRot").Value = Entity.rotation.Value;
			}
		}
	}
}