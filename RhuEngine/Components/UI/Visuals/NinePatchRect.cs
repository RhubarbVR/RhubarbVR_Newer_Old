using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public enum RNinePatchRectStretch {
		Stretch,
		Tile,
		TileFit,
	}

	[Category("UI/Container/Visuals")]
	public partial class NinePatchRect : UIVisuals
	{
		public readonly AssetRef<RTexture2D> Texture;
		[Default(true)]public readonly Sync<bool> DrawCenter;
		public readonly Sync<Vector2i> RegionMin;
		public readonly Sync<Vector2i> RegionMax;
		public readonly Sync<Vector2i> MarginMin;
		public readonly Sync<Vector2i> MarginMax;
		public readonly Sync<RNinePatchRectStretch> Horizontal;
		public readonly Sync<RNinePatchRectStretch> Vertical;
	}
}
