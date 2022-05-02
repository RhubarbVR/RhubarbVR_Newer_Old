using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class Head : Component
	{
		public readonly SyncRef<User> user;

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
			if (World.IsPersonalSpace) {
				var handVal = WorldManager.FocusedWorld?.GetLocalUser()?.userRoot.Target?.head.Target;
				if (handVal is not null) {
					var focusUserHand = WorldManager.FocusedWorld?.GetLocalUser()?.userRoot.Target?.head.Target;
					Entity.LocalTrans = focusUserHand?.LocalTrans ?? Matrix.Identity;
				}
			}
			else {
				if (user.Target is null) {
					return;
				}
				if (user.Target == World.GetLocalUser()) {
						Entity.LocalTrans = RInput.Head.HeadMatrix * RRenderer.CameraRoot.Inverse;
						user.Target.FindOrCreateSyncStream<SyncValueStream<Vector3f>>("HeadPos").Value = Entity.position.Value;
						user.Target.FindOrCreateSyncStream<SyncValueStream<Quaternionf>>("HeadRot").Value = Entity.rotation.Value;
				}
				else {
					var position = user.Target.FindSyncStream<SyncValueStream<Vector3f>>("HeadPos")?.Value ?? Vector3f.Zero;
					var rotation = user.Target.FindSyncStream<SyncValueStream<Quaternionf>>("HeadRot")?.Value ?? Quaternionf.Identity;
					Entity.LocalTrans = Matrix.TR(position, rotation);
				}
			}
		}
	}
}