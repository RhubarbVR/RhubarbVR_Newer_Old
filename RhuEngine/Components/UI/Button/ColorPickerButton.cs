using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	[Category("UI/Button")]
	public class ColorPickerButton : Button
	{
		public readonly Sync<Colorf> Color;
		public readonly Sync<bool> EditAlpha;
	}
}
