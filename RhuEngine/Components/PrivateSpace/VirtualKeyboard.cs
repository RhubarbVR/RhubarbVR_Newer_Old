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
		protected override void OnAttach() {
			base.OnAttach();
			var material = Entity.AttachComponent<UnlitMaterial>();
			material.MainTexture.Target = Entity.AttachComponent<Viewport>();
			Entity.AttachMesh<CanvasMesh>(material);


			var button = Entity.AddChild("Button").AttachComponent<Button>();
			button.Pressed.Target = Keyboard;
			button.Text.Value = "E";
		}

		[Exposed]
		public void Keyboard() {
			Engine.inputManager.KeyboardSystem.virtualKeyboard.PressingKeys.Add(Key.E);
			Engine.inputManager.KeyboardSystem.virtualKeyboard.TypeDelta += "E";
		}
	}
}
