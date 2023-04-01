using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public sealed partial class BillboardMaterialFeatere : BaseMaterialFeatere<EmissionMaterialFeatere>
	{
		public enum BillBoardMode : byte
		{
			Disabled,
			Enabled,
			YBillboard,
			ParticleBillboard,
		}

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<BillBoardMode> Mode;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> KeepScale;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<int> HFrames;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<int> VFrames;

		[OnChanged(nameof(UpdateMaterial))]
		public readonly Sync<bool> Loop;

	}
}
