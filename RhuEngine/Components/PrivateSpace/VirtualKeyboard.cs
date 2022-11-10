using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Components.PrivateSpace;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Local" })]
	public sealed class VirtualKeyboard : Component
	{
		public readonly SyncRef<ButtonBase> GrabButton;

		protected override void OnAttach() {
			base.OnAttach();
			var material = Entity.AttachComponent<UnlitMaterial>();
			var inputTexture = Entity.AttachComponent<Viewport>();
			inputTexture.TakeKeyboardFocus.Value = false;
			material.MainTexture.Target = inputTexture;
			var canvas = Entity.AttachMesh<CanvasMesh>(material);
			canvas.FrontBind.Value = false;
			canvas.TopOffset.Value = false;
			canvas.Scale.Value /= 2;
			canvas.InputInterface.Target = inputTexture;
			var e = Entity.AttachComponent<ValueCopy<Vector2i>>();
			e.Target.Target = canvas.Resolution;
			e.Source.Target = inputTexture.Size;


			var grabButton = Entity.AddChild("GrabBUtton").AttachComponent<ButtonBase>();
			grabButton.FocusMode.Value = RFocusMode.None;
			grabButton.InputFilter.Value = RInputFilter.Pass;
			grabButton.ButtonMask.Value = RButtonMask.Secondary;
			grabButton.Pressed.Target = Grab;
			GrabButton.Target = grabButton;

			var button = grabButton.Entity.AddChild("KeyButton").AttachComponent<Button>();
			button.FocusMode.Value = RFocusMode.None;
			button.InputFilter.Value = RInputFilter.Pass;
			button.Pressed.Target = Keyboard;
			button.Text.Value = "E";


		}
		[Exposed]
		public void Grab() {
			Entity.GetFirstComponent<Grabbable>()?.RemoteGrab(GrabButton.Target.LastHanded);
		}

		[Exposed]
		public void Keyboard() {
			Engine.inputManager.KeyboardSystem.virtualKeyboard.PressingKeys.Add(Key.E);
			Engine.inputManager.KeyboardSystem.virtualKeyboard.TypeDelta += "E";
		}
	}
}
