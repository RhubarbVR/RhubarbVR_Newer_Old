using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class ClearcoatMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		[Default(1f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> Clearcoat;

		[Default(0.5f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> Roughness;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;
	}
}
