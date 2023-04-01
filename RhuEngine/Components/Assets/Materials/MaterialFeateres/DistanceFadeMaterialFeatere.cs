using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class DistanceFadeMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		public enum DistanceFadeMode : byte
		{
			Disabled,
			PixelAlpha,
			PixelDither,
			ObjectDither
		}

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<DistanceFadeMode> Mode;

		[OnChanged(nameof(UpdateMaterial))]
		[Default(0f)]
		public readonly Sync<float> MinDistance;

		[OnChanged(nameof(UpdateMaterial))]
		[Default(10f)]
		public readonly Sync<float> MaxDistance;
	}
}
