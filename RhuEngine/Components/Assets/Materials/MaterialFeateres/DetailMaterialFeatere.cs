using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{

	public enum UVLayer : byte
	{
		UV1,
		UV2,
	}

	public enum BlendMode : byte
	{
		Mix,
		Add,
		Subtract,
		Multiply
	}

	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class DetailMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Mask;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<UVLayer> Layer;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<BlendMode> BlendMode;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Albedo;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Normal;
	}
}
