using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;


namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public sealed partial class UserAudioManager : Component
	{
		public readonly SyncRef<User> user;

		public readonly Linker<bool> audioMute;

		protected override void OnAttach() {
			user.Target = World.GetLocalUser();
		}

		protected override void RenderStep() {
			if (user.Target is null) {
				return;
			}
			audioMute.LinkedValue = user.Target != World.GetLocalUser();
		}
	}
}