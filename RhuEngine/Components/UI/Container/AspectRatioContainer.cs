using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RAspectRatioStretchMode {
		Fit,
		Cover,
		Width_Controls_Height,
		Height_Controls_Width,

	}

	[Category("UI/Container")]
	public partial class AspectRatioContainer : Container
	{
		[Default(1f)]
		public readonly Sync<float> Ratio;
		public readonly Sync<RAspectRatioStretchMode> StretchMode;
		public readonly Sync<RHorizontalAlignment> HorizontalAlignment;
		public readonly Sync<RVerticalAlignment> VerticalAlignment;

	}
}
