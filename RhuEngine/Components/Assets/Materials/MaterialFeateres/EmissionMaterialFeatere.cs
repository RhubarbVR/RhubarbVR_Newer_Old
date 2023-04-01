using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class EmissionMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<Colorf> Color;

		[Default(1f)]
		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<float> EnergyMultiplier;

		public enum EmissionOperator: byte
		{
			Add,
			Multiply
		}

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<EmissionOperator> Operator;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> UV2;

		[OnAssetLoaded(nameof(UpdateMaterialAsset))]
		public readonly AssetRef<RTexture2D> Texture;

	}
}
