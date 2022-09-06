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
		public readonly SyncRef<IObserver> CurrentObserver;

		private void ChangeObserverd() {
			if (LocalUser != MasterUser) {
				return;
			}
			CurrentObserver.Target?.Entity.Destroy();
			var type = Observerd.Target?.GetObserver();
			if(type is null) {
				return;
			}
			var addTo = TargetWindow.Target?.PannelRoot.Target;
			if (addTo is null) {
				return;
			}
			var ob = addTo.AddChild("Observer");
			ob.AttachComponent<UIRect>();
			CurrentObserver.Target = ob.AttachComponent<IObserver>(type);
			var mit = TargetWindow.Target?.MainMit.Target;
			if (mit is null) {
				mit =  Entity.AttachComponent<UnlitMaterial>();
				mit.DullSided.Value = true;
				mit.Transparency.Value = Transparency.Blend;
			}
			CurrentObserver.Target.SetUIRectAndMat(mit);
			CurrentObserver.Target.SetObserverd(Observerd.Target);
		}

		protected override void OnAttach() {
			base.OnAttach();
			var newWindow = Entity.AttachComponent<Window>();
			var local = Entity.AttachComponent<StandardLocale>();
			local.Key.Value = "Editor.Observer";
			local.TargetValue.Target = newWindow.NameValue;
			TargetWindow.Target = newWindow;
			newWindow.OnClose.Target = CloseWindow;
			newWindow.PinChanged.Target = PinWindow;
			newWindow.MinimizeButton.Value = false;
			newWindow.Canvas.Target.scale.Value *= new Vector3f(1, 1.5f, 1);
			newWindow.PannelRoot.Target?.GetFirstComponentOrAttach<UIRect>();
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
