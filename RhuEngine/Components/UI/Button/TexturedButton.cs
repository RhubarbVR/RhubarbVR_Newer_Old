using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RTexturedButtonStretchMode {
		Scale,
		Tile,
		Keep,
		KeepCenter,
		KeepAsspect,
		KeepAsspectCenter,
		KeepAsspectCovered,

	}

	[Category("UI/Button")]
	public partial class TexturedButton : ButtonBase
	{
		public readonly Sync<bool> IgnoreTextureSize;
		public readonly Sync<RTexturedButtonStretchMode> StretchMode;
		public readonly Sync<bool> FlipHorizontal;
		public readonly Sync<bool> FlipVertical;
		public readonly AssetRef<RTexture2D> Texture_Normal;
		public readonly AssetRef<RTexture2D> Texture_Press;
		public readonly AssetRef<RTexture2D> Texture_Hover;
		public readonly AssetRef<RTexture2D> Texture_Disabled;
		public readonly AssetRef<RTexture2D> Texture_Focused;
		public readonly AssetRef<RTexture2D> Texture_ClickMask;

	}
}
