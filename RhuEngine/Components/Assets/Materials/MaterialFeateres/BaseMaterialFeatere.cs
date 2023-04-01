using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	public enum TextureChannel : byte
	{
		Red,
		Green,
		Blue,
		Alpha,
		Gray
	}

	[Category(new string[] { "Assets/Materials/MaterialFeateres" })]
	[AllowedOnWorldRoot]
	public abstract partial class BaseMaterialFeatere<T> : AssetProvider<T> where T : BaseMaterialFeatere<T>, new()
	{
		public void UpdateMaterialAsset(RTexture2D _) {
			UpdateMaterial(null);
		}

		public void UpdateMaterial(IChangeable _) {
			RenderThread.ExecuteOnStartOfFrame(this, () => Load((T)this));
		}

		protected override void OnLoaded() {
			base.OnLoaded();
			UpdateMaterial(null);
		}
	}
}
