using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public class UserBodyNodeTransform : Component
	{
		public readonly Linker<Vector3f> Pos;

		public readonly Linker<Quaternionf> Rot;

		public readonly Linker<Vector3f> Scale;

		[Default(BodyNode.Head)]
		public readonly Sync<BodyNode> TargetNode;
		[Default(true)]
		public readonly Sync<bool> LocalUserDefault;
		public readonly SyncRef<User> TargetUser;
		
		private User GetUser => LocalUserDefault ? TargetUser.Target??LocalUser : TargetUser.Target;
		
		public override void Step() {
			base.Step();
			var targetUser = GetUser;
			if(targetUser == null) {
				return;
			}
			var endPos = targetUser.GetBodyNodeTrans(TargetNode);
			endPos.Decompose(out var outpos, out var outrot, out var outscale);
			if (Pos.Linked) {
				Pos.LinkedValue = outpos;
			}
			if (Scale.Linked) {
				Scale.LinkedValue = outscale;
			}
			if (Rot.Linked) {
				Rot.LinkedValue = outrot;
			}
		}
	}
}
