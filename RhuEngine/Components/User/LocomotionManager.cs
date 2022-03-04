using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public class LocomotionManager : Component
	{
		public SyncRef<User> user;

		public Sync<int> selectedModule;

		public SyncObjList<SyncRef<LocomotionModule>> modules;

		public override void OnAttach() {
			modules.Add().Target = Entity.AttachComponent<NoClipLocomotion>();
		}

		public LocomotionModule CurrentLocomotionModule => selectedModule.Value > (modules.Count - 1) ? null : modules[selectedModule.Value].Target;

		public override void Step() {
			if (user.Target is null) {
				return;
			}
			if (user.Target == World.GetLocalUser()) {
				var locModule = CurrentLocomotionModule;
				if (locModule is null) {
					return;
				}
				if (Platform.KeyboardVisible) {
					return;
				}
				locModule.ProcessMovement();
			}
		}

	}
}
