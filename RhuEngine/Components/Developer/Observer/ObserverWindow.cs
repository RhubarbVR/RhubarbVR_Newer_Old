using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{

	[Category(new string[] { "Developer/Observer" })]
	public class ObserverWindow : Component
	{
		[OnChanged(nameof(ChangeObserverd))]
		public readonly SyncRef<IWorldObject> Observerd;

		public readonly SyncRef<Window> TargetWindow;
		public readonly SyncRef<Entity> LastLocation;

		private void ChangeObserverd() {
			if (LocalUser != MasterUser) {
				return;
			}

		}

		protected override void OnAttach() {
			base.OnAttach();
			var newWindow = Entity.AttachComponent<Window>();
			TargetWindow.Target = newWindow;
			newWindow.OnClose.Target = CloseWindow;
			newWindow.PinChanged.Target = PinWindow;
			newWindow.MinimizeButton.Value = false;
			newWindow.Canvas.Target.scale.Value *= new Vector3f(1, 1.5f, 1);
		}

		[Exposed]
		private void PinWindow(bool pin) {
			if (pin) {
				LastLocation.Target = Entity.parent.Target;
				var root = LocalUser.userRoot.Target?.Entity;
				Entity.SetParent(root);
			}
			else {
				var last = LastLocation.Target ?? World.RootEntity;
				Entity.SetParent(last);
			}
		}

		[Exposed]
		private void CloseWindow() {
			Entity.Destroy();
		}
	}
}
