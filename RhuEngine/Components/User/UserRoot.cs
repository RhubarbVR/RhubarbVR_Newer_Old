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
		public SyncRef<User> user;

		public SyncRef<Entity> head;

		public SyncRef<Entity> leftHand;

		public SyncRef<Entity> rightHand;

		public Linker<Vector3f> pos;

		public Linker<Quaternionf> rot;

		public Linker<Vector3f> scale;

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
					var focusUserRoot = WorldManager.FocusedWorld.GetLocalUser().userRoot.Target;
					Entity.GlobalTrans = focusUserRoot.Entity.GlobalTrans;
				}
				if (Engine.EngineLink.CanRender) {
					RRenderer.CameraRoot = Entity.GlobalTrans;
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