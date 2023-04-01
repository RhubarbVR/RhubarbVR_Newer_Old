using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class TransformMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> FixedSize;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> UseParticleTrails;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> UsePointSize;

		[OnChanged(nameof(UpdateMaterial))]
		[Default(1f)]
		public readonly Sync<float> PointSize;

	}
}
