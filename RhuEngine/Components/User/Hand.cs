using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using RhuEngine.Input.XRInput;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public sealed class Hand : Component
	{
		public readonly SyncRef<User> user;

		public readonly Sync<Handed> hand;

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
			if (user.Target is null) {
				return;
			}
			if (user.Target == World.GetLocalUser()) {
				var pos = InputManager.XRInputSystem.GetHand(hand.Value)?[TrackerPos.Default];
				if (pos is not null && pos.HasPos) {
					Entity.position.Value = pos.Position;
					Entity.rotation.Value = pos.Rotation;
				}
				user.Target.FindOrCreateSyncStream<SyncValueStream<Vector3f>>($"HandPos{hand.Value}").Value = Entity.position.Value;
				user.Target.FindOrCreateSyncStream<SyncValueStream<Quaternionf>>($"HandRot{hand.Value}").Value = Entity.rotation.Value;
			}
			else {
				var possition = user.Target.FindSyncStream<SyncValueStream<Vector3f>>($"HandPos{hand.Value}")?.Value ?? Vector3f.Zero;
				var rotation = user.Target.FindSyncStream<SyncValueStream<Quaternionf>>($"HandRot{hand.Value}")?.Value ?? Quaternionf.Identity;
				Entity.LocalTrans = Matrix.TR(possition, rotation);
			}
		}
	}
}