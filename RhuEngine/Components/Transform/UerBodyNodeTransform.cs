using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.Movement)]
	[Category(new string[] { "Transform" })]
	public sealed partial class UserBodyNodeTransform : Component
	{
		public readonly Linker<Vector3f> Pos;

		public readonly Linker<Quaternionf> Rot;

		public readonly Linker<Vector3f> Scale;

		public readonly Sync<Quaternionf> OffsetRotate;
		public readonly Sync<Vector3f> OffsetPos;
		public readonly Sync<Vector3f> OffsetScale;

		[Default(BodyNode.Head)]
		public readonly Sync<BodyNode> TargetNode;
		[Default(true)]
		public readonly Sync<bool> LocalUserDefault;
		public readonly SyncRef<User> TargetUser;

		private User GetUser => LocalUserDefault ? TargetUser.Target ?? LocalUser : TargetUser.Target;

		protected override void OnAttach() {
			base.OnAttach();
			Pos.Target = Entity.position;
			Rot.Target = Entity.rotation;
			Scale.Target = Entity.scale;
			OffsetScale.Value = Vector3f.One;
		}

		protected override void Step() {
			base.Step();
			var targetUser = GetUser;
			if (targetUser == null) {
				return;
			}
			var endPos = targetUser.GetBodyNodeTrans(TargetNode);
			endPos *= Entity.InternalParent.GlobalTrans.Inverse;
			endPos.Decompose(out var outpos, out var outrot, out var outscale);
			if (Pos.Linked) {
				Pos.LinkedValue = outpos + OffsetPos;
			}
			if (Scale.Linked) {
				Scale.LinkedValue = outscale * OffsetScale;
			}
			if (Rot.Linked) {
				Rot.LinkedValue = outrot * OffsetRotate;
			}
		}
	}
}
