using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.Managers;

using RhuSettings;


namespace RhuEngine.Settings
{
	public class KeyInput : SettingsObject
	{
		public bool BlockWithSpecialkeys = false;

		[SettingsField("", "/")]
		public Key MainKey = Key.None;

		[SettingsField("", "/")]
		public Key SecondKey = Key.None;

		[SettingsField("", "/")]
		public Key DoubleKeyOne = Key.None;

		[SettingsField("", "/")]
		public Key DoubleKeyTwo = Key.None;

		public bool GetInput() {
			return RInput.Key(SecondKey).IsActive() || RInput.Key(MainKey).IsActive() || (RInput.Key(DoubleKeyOne).IsActive() && RInput.Key(DoubleKeyTwo).IsActive());
		}

		public KeyInput(Key mkey, Key dokey = Key.None, Key dtkey = Key.None, Key skey = Key.None) {
			MainKey = mkey;
			DoubleKeyOne = dokey;
			DoubleKeyTwo = dtkey;
			SecondKey = skey;
		}
		public KeyInput() {

		}
	}

	public class KeyboardInputSettingsObject : SettingsObject
	{
		[SettingsField()]
		public InputManager.InputTypes MousePositive = InputManager.InputTypes.ObjectPull;
		[SettingsField()]
		public InputManager.InputTypes MouseNegevitve = InputManager.InputTypes.ObjectPush;

		[SettingsField()]
		public KeyInput Sprint = new(Key.Shift);

		[SettingsField()]
		public KeyInput SlowCraw = new(Key.Ctrl);

		[SettingsField()]
		public KeyInput Forward = new(Key.W);

		[SettingsField()]
		public KeyInput Back = new(Key.S);

		[SettingsField()]
		public KeyInput Right = new(Key.D);

		[SettingsField()]
		public KeyInput Left = new(Key.A);

		[SettingsField()]
		public KeyInput RotateRight = new(Key.X);

		[SettingsField()]
		public KeyInput RotateLeft = new(Key.C);

		[SettingsField()]
		public KeyInput Jump = new(Key.Space);

		[SettingsField()]
		public KeyInput FlyUP = new(Key.E);

		[SettingsField()]
		public KeyInput FlyDown = new(Key.Q);

		[SettingsField()]
		public KeyInput Dash = new(Key.Esc,Key.Ctrl,Key.Space);

		[SettingsField()]
		public KeyInput SwitchWorld = new(Key.F1, Key.Ctrl, Key.Tab);

		[SettingsField()]
		public KeyInput Grab = new(Key.MouseRight);

		[SettingsField()]
		public KeyInput ContextMenu = new(Key.MouseRight);

		[SettingsField()]
		public KeyInput SecondaryPress = new(Key.MouseCenter,Key.T);

		[SettingsField()]
		public KeyInput PrimaryPress = new(Key.MouseLeft);

		[SettingsField()]
		public KeyInput ObjectPull = new();

		[SettingsField()]
		public KeyInput ObjectPush = new();
	}
}
