using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class AmbientOcclusionMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		[Default(0f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> LightAffect;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> UV2;


		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<TextureChannel> Channel;
	}
}
