using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RColorMode {
		RGB,
		HSV,
		HSL,
		RAW,
	}
	public enum RColorPickerShape
	{
		HSV_Rect,
		HSV_Rect_Wheel,
		VHS_Color,
		HSL_Circle,
	}
	[Category("UI/Container/BoxContainer")]
	public partial class ColorPickerContainer : BoxContainer
	{
		public readonly Sync<Colorf> Color;
		public readonly Sync<bool> EditAlpha;
		public readonly Sync<RColorMode> ColorMode;
		public readonly Sync<bool> DeferredMode;
		public readonly Sync<RColorPickerShape> ColorPickerShape;
		[Default(true)]
		public readonly Sync<bool> PressetsEnabled;
		[Default(true)]
		public readonly Sync<bool> PressetsVisible;


	}
}
