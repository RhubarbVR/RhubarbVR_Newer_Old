using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public sealed class IsInVR : Component
	{
		public readonly SyncRef<User> user;

		public readonly Linker<bool> isVR;

		public readonly Linker<bool> isNotVR;

		protected override void OnAttach() {
			base.OnAttach();
			user.Target = LocalUser;
		}

		protected override void Step() {
			if(user.Target != LocalUser) {
				return;
			}
			if (isVR.Linked) {
				isVR.LinkedValue = Engine.IsInVR;
			}
			if (isNotVR.Linked) {
				isNotVR.LinkedValue = !Engine.IsInVR;
			}
		}
	}
}