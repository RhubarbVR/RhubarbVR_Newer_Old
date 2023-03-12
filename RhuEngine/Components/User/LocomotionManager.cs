using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.Components
{

	[UpdateLevel(UpdateEnum.PlayerInput)]
	[Category(new string[] { "User" })]
	public sealed partial class LocomotionManager : Component
	{
		public readonly SyncRef<User> user;

		public readonly Sync<int> selectedModule;

		public readonly SyncObjList<SyncRef<LocomotionModule>> modules;

		protected override void OnAttach() {
			modules.Add().Target = Entity.AttachComponent<NoClipLocomotion>();
		}

		public LocomotionModule CurrentLocomotionModule => selectedModule.Value > (modules.Count - 1) ? null : modules[selectedModule.Value].Target;

		protected override void RenderStep() {
			if (!Engine.EngineLink.CanInput) {
				return;
			}
			if (World.IsPersonalSpace) {
				return;
			}
			if (!Engine.MouseFree && !Engine.IsInVR) {
				return;
			}
			if (user.Target is null || user.Target != World.GetLocalUser()) {
				return;
			}
			if (Engine.HasKeyboard && !Engine.IsInVR) {
				return;
			}
			var locModule = CurrentLocomotionModule;
			if (locModule is null) {
				return;
			}
			locModule.ProcessMovement();
		}

	}
}
