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

	[Category("UI/Container/Visuals")]
	public class TextureRect : UIVisuals
	{
		public readonly AssetRef<RTexture2D> Texture;
		public readonly Sync<bool> IgnoreTextureSize;
		public readonly Sync<RStrechMode> StrechMode;
		public readonly Sync<bool> FlipVertical;
		public readonly Sync<bool> FlipHorizontal;

	}
}
