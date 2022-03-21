using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class Hand : Component
	{
		public SyncRef<User> user;

		public Sync<Handed> hand;

		public Linker<Vec3> pos;

		public Linker<Quat> rot;

		public Linker<Vec3> scale;

		public override void OnAttach() {
			pos.SetLinkerTarget(Entity.position);
			rot.SetLinkerTarget(Entity.rotation);
			scale.SetLinkerTarget(Entity.scale);
		}

		public override void Step() {
			if (user.Target is null) {
				return;
			}
			if (user.Target == World.GetLocalUser()) {
				Entity.LocalTrans = Input.Hand(hand.Value).wrist.ToMatrix() * Renderer.CameraRoot.Inverse;
				user.Target.FindOrCreateSyncStream<SyncValueStream<Vec3>>($"HandPos{hand.Value}").Value = Entity.position.Value;
				user.Target.FindOrCreateSyncStream<SyncValueStream<Quat>>($"HandRot{hand.Value}").Value = Entity.rotation.Value;
			}
			else {
				var possition = user.Target.FindSyncStream<SyncValueStream<Vec3>>($"HandPos{hand.Value}")?.Value ?? Vec3.Zero;
				var rotation = user.Target.FindSyncStream<SyncValueStream<Quat>>($"HandRot{hand.Value}")?.Value ?? Quat.Identity;
				Entity.LocalTrans = Matrix.TR(possition, rotation);
			}
		}
	}
}