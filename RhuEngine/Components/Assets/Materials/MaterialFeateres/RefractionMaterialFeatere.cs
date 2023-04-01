using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class RefractionMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{

		[OnChanged(nameof(UpdateMaterial))]
		[Default(0.05f)]
		public readonly Sync<float> Scale;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<TextureChannel> Channel;

	}
}
