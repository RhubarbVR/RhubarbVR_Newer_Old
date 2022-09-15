using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RhuEngine.Linker;

using RhuSettings;

namespace RhuEngine.Settings
{
	public class InputAction : SettingsObject
	{
		[SettingsField("", "/")]
		public string[][] Input = Array.Empty<string[]>();

		public InputAction(string[][] trains) {
			Input = trains;
		}
		public InputAction() {

		}
	}

	public class MovmentSettings : SettingsObject
	{
		[SettingsField("Head Movemnet use head as bases of movemnt")]
		public bool HeadBasedMovement = false;
	}

	public class InputSettingsObject : SettingsObject
	{
		[SettingsField("If your primary hand is the right hand")]
		public bool RightHanded = true;

		[SettingsField("Movment Settings")]
		public MovmentSettings MovmentSettings = new();

		[SettingsField("Sprint")]
		public InputAction Sprint = new(new[] { new[] { "Key.Shift" } });

		[SettingsField("Back")]
		public InputAction Back = new(new[] { new[] { "Key.S" } });

		[SettingsField("Forward")]
		public InputAction Forward = new(new[] { new[] { "Key.W" } });

		[SettingsField("Left")]
		public InputAction Left = new(new[] { new[] { "Key.A" } });

		[SettingsField("Right")]
		public InputAction Right = new(new[] { new[] { "Key.D" } });

		[SettingsField("Jump")]
		public InputAction Jump = new(new[] { new[] { "Key.Space" } });

		[SettingsField("FlyUp")]
		public InputAction FlyUp = new(new[] { new[] { "Key.E" } });

		[SettingsField("FlyDown")]
		public InputAction FlyDown = new(new[] { new[] { "Key.Q" } });

		[SettingsField("RotateLeft")]
		public InputAction RotateLeft = new(new[] { new[] { "Key.Z" } });

		[SettingsField("RotateRight")]
		public InputAction RotateRight = new(new[] { new[] { "Key.X" } });

		[SettingsField("ObjectPull")]
		public InputAction ObjectPull = new(new[] { new[] { "Mouse.Scroll.y-" } });

		[SettingsField("ObjectPush")]
		public InputAction ObjectPush = new(new[] { new[] { "Mouse.Scroll.y" } });

		[SettingsField("OpenDash")]
		public InputAction OpenDash = new(new[] { new[] { "Key.Esc" }, new[] { "Key.Ctrl", "Key.Space" } });

		[SettingsField("ChangeWorld")]
		public InputAction ChangeWorld = new(new[] { new[] { "Key.Ctrl", "Key.Tab" } });

		[SettingsField("ContextMenu")]
		public InputAction ContextMenu = new(new[] { new[] { "Key.T" } });

		[SettingsField("Primary")]
		public InputAction Primary = new(new[] { new[] { "Key.MouseLeft" } });

		[SettingsField("Secondary")]
		public InputAction Secondary = new(new[] { new[] { "Key.MouseCenter" } });

		[SettingsField("Grab")]
		public InputAction Grab = new(new[] { new[] { "Key.MouseRight" } });

		[SettingsField("UnlockMouse")]
		public InputAction UnlockMouse = new(new[] { new[] { "Key.R" } });

		[SettingsField("ObserverOpen")]
		public InputAction ObserverOpen = new(new[] { new[] { "Key.I" } });

	}
}
