using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class UserAudioManager : Component
	{
		public SyncRef<User> user;

		public Linker<float> audioVolume;

		public override void OnAttach() {
			user.Target = World.GetLocalUser();
		}

		public override void Step() {
			if (user.Target is null) {
				return;
			}
			audioVolume.LinkedValue = user.Target == World.GetLocalUser() ? 0f : 1f;
		}
	}
}