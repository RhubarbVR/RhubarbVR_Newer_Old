using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class RimMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		[Default(1f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> Rim;

		[Default(0.5f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> Tint;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;
	}
}
