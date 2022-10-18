using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public sealed class Head : Component
	{
		public readonly SyncRef<User> user;

		public readonly Linker<Vector3f> pos;

		public readonly Linker<Quaternionf> rot;

		public readonly Linker<Vector3f> scale;

		protected override void OnAttach() {
			pos.SetLinkerTarget(Entity.position);
			rot.SetLinkerTarget(Entity.rotation);
			scale.SetLinkerTarget(Entity.scale);
		}

		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (World.IsPersonalSpace) {
				Entity.LocalTrans = InputManager.HeadMatrix;
			}
			else {
				if (user.Target is null) {
					return;
				}
				if (user.Target == LocalUser) {
					Entity.LocalTrans = InputManager.HeadMatrix;
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