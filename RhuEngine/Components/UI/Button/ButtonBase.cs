using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RButtonActionMode {
		Press,
		Relases
	}
	[Flags]
	public enum RButtonMask
	{
		None = 0,
		Primary = 1,
		Secondary = 2,
		Tertiary = 4
	}

	[Category("UI/Button")]
	public class ButtonBase : UIElement
	{
		public readonly Sync<bool> Disabled;
		public readonly Sync<bool> ToggleMode;
		public readonly Sync<bool> ButtonPressed;
		public readonly Sync<RButtonActionMode> ActionMode;
		public readonly Sync<RButtonMask> ButtonMask;
		public readonly Sync<bool> KeepPressedOutside;
	}
}
