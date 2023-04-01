using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	[AllowedOnWorldRoot]
	public sealed partial class ORMMaterial : BaseMaterial<RORMMaterial>
	{
		[OnAssetLoaded(nameof(ORMTextureChange))]
		public readonly AssetRef<RTexture2D> ORMTexture;

		public void ORMTextureChange(RTexture2D _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.ORMTexture = ORMTexture.Asset);
		}

		protected override void UpdateAll() {
			base.UpdateAll();
			ORMTextureChange(null);
		}

	}
}
