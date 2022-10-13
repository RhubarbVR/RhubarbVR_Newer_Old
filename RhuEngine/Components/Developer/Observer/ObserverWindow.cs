using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;

namespace RhuEngine.Components
{

	[Category(new string[] { "Developer/Observer" })]
	public sealed class ObserverWindow : Component
	{
		[OnChanged(nameof(ChangeObserverd))]
		public readonly SyncRef<IWorldObject> Observerd;

		public readonly SyncRef<Window> TargetWindow;
		public readonly SyncRef<Entity> LastLocation;
		public readonly SyncRef<Viewport> MainViewPort;
		public readonly SyncRef<ScrollContainer> RootUIElement;
		public readonly SyncRef<IObserver> CurrentObserver;

		private void ChangeObserverd() {
			if (LocalUser != MasterUser) {
				return;
			}
			CurrentObserver.Target?.Entity.Destroy();
			var type = Observerd.Target?.GetObserver();
			if (type is null) {
				return;
			}
			var addTo = RootUIElement.Target?.Entity;
			if (addTo is null) {
				return;
			}
			var child = addTo.AddChild(type.GetFormattedName());
			var copyer = child.AttachComponent<ValueCopy<Vector2i>>();
			var boxCon = child.AttachComponent<BoxContainer>();
			boxCon.FocusMode.Value = RFocusMode.Click;
			boxCon.Vertical.Value = true;
			copyer.Target.Target = boxCon.MinSize;
			copyer.Source.Target = RootUIElement.Target.MinSize;
			CurrentObserver.Target = child.AttachComponent<IObserver>(type);
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
			newWindow.PannelRoot.Target?.GetFirstComponentOrAttach<UI3DRect>();
			if(newWindow.PannelRoot.Target is null) {
				return;
			}
			var root2dUI = newWindow.PannelRoot.Target.AddChild("2DUiRoot");
			root2dUI.AttachComponent<UI3DRect>();
			Viewport mainViewPort;
			var input = root2dUI.AttachComponent<UI3DInputInteraction>();
			input.PixelPerMeter.Value = 1024;
			input.InputInterface.Target = mainViewPort = MainViewPort.Target = root2dUI.AttachComponent<Viewport>();
			mainViewPort.Disable3D.Value = true;
			mainViewPort.TransparentBG.Value = true;
			var uiMit = root2dUI.AttachComponent<UnlitMaterial>();
			uiMit.MainTexture.Target = mainViewPort;
			uiMit.Transparency.Value = Transparency.Blend;
			var visual = root2dUI.AttachComponent<UI3DRectangle>();
			visual.Material.Target = uiMit;
			visual.Tint.Value = Colorf.White;
			var root = root2dUI.AddChild("Root").AttachComponent<ScrollContainer>();
			var copyer = root2dUI.AttachComponent<ValueCopy<Vector2i>>();
			copyer.Source.Target = mainViewPort.Size;
			copyer.Target.Target = root.MinSize;
			input.SizeSeter.Target = mainViewPort.Size;
			RootUIElement.Target = root;
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
