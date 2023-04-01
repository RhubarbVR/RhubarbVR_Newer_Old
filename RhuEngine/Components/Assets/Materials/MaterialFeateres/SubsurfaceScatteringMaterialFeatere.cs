using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class SubsurfaceScatteringMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		[Default(0f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> Strength;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> SkinMode;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> TransmittanceEnabled;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<Colorf> TransmittanceColor;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> TransmittanceTexture;

		[Default(0.1f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> TransmittanceDepth;

		[Default(0f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> TransmittanceBoost;
	}
}
