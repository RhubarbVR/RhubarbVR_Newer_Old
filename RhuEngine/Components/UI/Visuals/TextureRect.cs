using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RStrechMode {
		Scaled,
		Tile,
		Keep,
		KeepCenter,
		KeepAspect,
		KeepAspectCenter,
		KeepAspectCovered,
	}

	public enum RExpandedMode {
		KeepSize,
		IgnoreSize,
		FitWidth,
		FitWidthProportional,
		FitHeight,
		FitHeightProportional,
	}

	[Category("UI/Container/Visuals")]
	public class TextureRect : UIVisuals
	{
		public readonly AssetRef<RTexture2D> Texture;
		public readonly Sync<RExpandedMode> ExpandedMode;
		public readonly Sync<RStrechMode> StrechMode;
		public readonly Sync<bool> FlipVertical;
		public readonly Sync<bool> FlipHorizontal;

	}
}
