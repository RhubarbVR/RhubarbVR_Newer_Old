using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class HeightMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		[Default(5f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> Scale;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> DeepParallax;

		[OnChanged(nameof(UpdateMaterial))]
		[Default(10)]
		public readonly Sync<int> DeepParallaxMinLayer;

		[OnChanged(nameof(UpdateMaterial))]
		[Default(32)]
		public readonly Sync<int> DeepParallaxMaxLayer;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> FlipTangent;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> FlipBinormal;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> FlipTexture;
	}
}
