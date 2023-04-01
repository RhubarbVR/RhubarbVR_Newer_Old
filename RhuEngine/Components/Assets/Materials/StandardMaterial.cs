using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	[AllowedOnWorldRoot]
	public sealed partial class StandardMaterial : BaseMaterial<RStandardMaterial>
	{

		[OnChanged(nameof(MetallicChange))] public readonly Sync<float> Metallic;
		[Default(0.5f)][OnChanged(nameof(SpecularChange))] public readonly Sync<float> Specular;
		[OnAssetLoaded(nameof(MetallicTextureChange))] public readonly AssetRef<RTexture2D> MetallicTexture;
		[OnChanged(nameof(MetallicChannelChange))] public readonly Sync<TextureChannel> MetallicChannel;
		[OnChanged(nameof(RoughnessChange))] public readonly Sync<float> Roughness;
		[OnAssetLoaded(nameof(RoughnessTextureChange))] public readonly AssetRef<RTexture2D> RoughnessTexture;
		[OnChanged(nameof(RoughnessChannelChange))] public readonly Sync<TextureChannel> RoughnessChannel;

		public void MetallicChange(IChangeable _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.Metallic = Metallic.Value);
		}
		public void SpecularChange(IChangeable _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.Specular = Specular.Value);
		}
		public void MetallicTextureChange(RTexture2D _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.MetallicTexture = MetallicTexture.Asset);
		}
		public void MetallicChannelChange(IChangeable _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.MetallicChannel = MetallicChannel.Value);
		}
		public void RoughnessChange(IChangeable _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.Roughness = Roughness.Value);
		}
		public void RoughnessTextureChange(RTexture2D _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.RoughnessTexture = RoughnessTexture.Asset);
		}
		public void RoughnessChannelChange(IChangeable _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.RoughnessChannel = RoughnessChannel.Value);
		}


		protected override void UpdateAll() {
			base.UpdateAll();
			MetallicChange(null);
			SpecularChange(null);
			MetallicTextureChange(null);
			MetallicChannelChange(null);
			RoughnessChange(null);
			RoughnessTextureChange(null);
			RoughnessChannelChange(null);
		}

	}
}
