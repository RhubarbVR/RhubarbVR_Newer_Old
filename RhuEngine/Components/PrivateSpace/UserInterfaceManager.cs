using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RhuEngine.WorldObjects.ECS;

using SharedModels;

using RNumerics;
using RhuEngine.Linker;
using RhuEngine.Physics;
using RhuEngine.WorldObjects;
using RhuEngine.Components.PrivateSpace;


namespace RhuEngine.Components
{
	[PrivateSpaceOnly]
	[UpdateLevel(UpdateEnum.Normal)]
	public sealed class UserInterfaceManager : Component
	{
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public PrivateSpaceManager PrivateSpaceManager;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UIElement UserInterface;

		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public Entity VrElements;
		[NoSave]
		[NoSync]
		[NoLoad]
		[NoSyncUpdate]
		public UnlitMaterial UImaterial;
		public bool OpenCloseDash
		{
			get => UserInterface.Enabled.Value;
			set {
				UserInterface.Enabled.Value = value;
				VrElements.enabled.Value = value && Engine.IsInVR;
				PrivateSpaceManager.VRViewPort.UpdateMode.Value = value && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
				if (value) {
					InputManager.screenInput.FreeMouse();
				}
				else {
					InputManager.screenInput.UnFreeMouse();
				}
#if DEBUG
				if (UserInterface.Enabled.Value) {
					RLog.Info("Opened Dash");
				}
				else {
					RLog.Info("Closed Dash");
				}
#endif
			}
		}

		public void ToggleDash() {
			OpenCloseDash = !OpenCloseDash;
		}

		private void EngineLink_VRChange(bool obj) {
			UserInterface.Entity.parent.Target = Engine.IsInVR ? PrivateSpaceManager.VRViewPort.Entity : PrivateSpaceManager.RootScreenElement.Entity;
			PrivateSpaceManager.VRViewPort.UpdateMode.Value = OpenCloseDash && Engine.IsInVR ? RUpdateMode.Always : RUpdateMode.Disable;
			VrElements.enabled.Value = OpenCloseDash && Engine.IsInVR;
			PrivateSpaceManager.VRViewPort.Enabled.Value = Engine.IsInVR;
		}

		public Entity RootUIEntity => UserInterface.Entity;

		internal void LoadInterface() {
			UImaterial = Entity.AttachComponent<UnlitMaterial>();
			UImaterial.DullSided.Value = true;
			UImaterial.Transparency.Value = Transparency.Blend;
			UImaterial.MainTexture.Target = PrivateSpaceManager.VRViewPort;
			VrElements = Entity.AddChild("VrElements");
			var mest = VrElements.AddChild("Stuff").AttachMesh<TrivialBox3Mesh>(UImaterial);
			mest.Entity.scale.Value = Vector3f.One / 3;
			VrElements.enabled.Value = false;
			Engine.EngineLink.VRChange += EngineLink_VRChange;
			var uielementone = RootUIEntity.AddChild("Color").AttachComponent<ColorRect>();
			uielementone.Color.Value = Colorf.SteelBlue;
			uielementone.Max.Value = new Vector2f(0.5, 0.5);
			var uielementtwo = RootUIEntity.AddChild("Color").AttachComponent<ColorRect>();
			uielementtwo.Color.Value = Colorf.Green;
			uielementtwo.Min.Value = new Vector2f(0.5, 0);
			uielementone.Max.Value = new Vector2f(1, 0.5);
			var uielementthree = RootUIEntity.AddChild("Color").AttachComponent<ColorRect>();
			uielementthree.Color.Value = Colorf.Salmon;
			uielementthree.Min.Value = new Vector2f(0, 0.5);
			uielementthree.Max.Value = new Vector2f(0.5, 1);
			var uielementFour = RootUIEntity.AddChild("Color").AttachComponent<ColorRect>();
			uielementFour.Color.Value = Colorf.Gainsboro;
			uielementFour.Min.Value = new Vector2f(0.5, 0.5);
			uielementthree.Max.Value = new Vector2f(1, 1);
			UserInterface.Enabled.Value = false;
			EngineLink_VRChange(Engine.IsInVR);
		}

		protected override void Step() {
			base.Step();
			if (InputManager.OpenDash.JustActivated()) {
				ToggleDash();
			}
		}
	}
}
