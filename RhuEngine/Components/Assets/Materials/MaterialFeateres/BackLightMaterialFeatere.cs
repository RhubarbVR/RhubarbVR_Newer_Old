using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class BackLightMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<Colorf> Color;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;


	}
}
