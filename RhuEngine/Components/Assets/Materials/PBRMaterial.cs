using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Materials" })]
	public class PBRMaterial : MaterialBase<IPBRMaterial>
	{
		[Default(BasicRenderMode.Opaque)]
		[OnChanged(nameof(RenderModeChanged))]
		public readonly Sync<BasicRenderMode> RenderMode;
		[OnAssetLoaded(nameof(BasicColorSettings))]
		public readonly AssetRef<RTexture2D> AlbedoTexture;
		[OnChanged(nameof(BasicColorSettings))]
		public readonly Sync<Colorf> AlbedoTint;
		[Default(0.5f)]
		[OnChanged(nameof(BasicColorSettings))]
		public readonly Sync<float> AlphaCutOut;
		[OnAssetLoaded(nameof(MetalicAndSmoothness))]
		public readonly AssetRef<RTexture2D> MetallicTexture;
		[OnChanged(nameof(MetalicAndSmoothness))]
		public readonly Sync<float> Metallic;
		[Default(0.5f)]
		[OnChanged(nameof(MetalicAndSmoothness))]
		public readonly Sync<float> Smoothness;
		[OnChanged(nameof(MetalicAndSmoothness))]
		public readonly Sync<bool> SmoothnessFromAlbedo;
		[OnAssetLoaded(nameof(MapsUpdate))]
		public readonly AssetRef<RTexture2D> NormalMap;
		[OnAssetLoaded(nameof(MapsUpdate))]
		public readonly AssetRef<RTexture2D> HeightMap;
		[OnAssetLoaded(nameof(OtherTexturesUpdate))]
		public readonly AssetRef<RTexture2D> Occlusion;
		[OnAssetLoaded(nameof(OtherTexturesUpdate))]
		public readonly AssetRef<RTexture2D> DetailMask;
		[OnChanged(nameof(EmissionUpdate))]
		public readonly Sync<bool> Emission;
		[OnAssetLoaded(nameof(EmissionUpdate))]
		public readonly AssetRef<RTexture2D> EmissionTexture;
		[OnChanged(nameof(EmissionUpdate))]
		public readonly Sync<Colorf> EmissionTint;
		[OnChanged(nameof(UVUpdate))]
		public readonly Sync<Vector2f> Tilling;
		[OnChanged(nameof(UVUpdate))]
		public readonly Sync<Vector2f> UVOffset;
		private void RenderModeChanged() {
			if (_material is null) {
				return;
			}
			_material.RenderMode = RenderMode;
			_material.Material?.UpdatePrams();
		}
		private void BasicColorSettings() {
			if (_material is null) {
				return;
			}
			_material.AlbedoTexture = AlbedoTexture.Asset;
			_material.AlbedoTint = AlbedoTint;
			_material.AlphaCutOut = AlphaCutOut;
			_material.Material?.UpdatePrams();
		}
		private void MetalicAndSmoothness() {
			if (_material is null) {
				return;
			}
			_material.MetallicTexture = MetallicTexture.Asset;
			_material.Metallic = Metallic;
			_material.Smoothness = Smoothness;
			_material.SmoothnessFromAlbedo = SmoothnessFromAlbedo;
			_material.Material?.UpdatePrams();
		}
		private void MapsUpdate() {
			if (_material is null) {
				return;
			}
			_material.NormalMap = NormalMap.Asset;
			_material.HeightMap = HeightMap.Asset;
			_material.Material?.UpdatePrams();
		}
		private void OtherTexturesUpdate() {
			if (_material is null) {
				return;
			}
			_material.Occlusion = Occlusion.Asset;
			_material.DetailMask = DetailMask.Asset;
			_material.Material?.UpdatePrams();
		}
		private void EmissionUpdate() {
			if (_material is null) {
				return;
			}
			_material.Emission = Emission;
			_material.EmissionTexture = EmissionTexture.Asset;
			_material.EmissionTint = EmissionTint;
			_material.Material?.UpdatePrams();
		}

		private void UVUpdate() {
			if (_material is null) {
				return;
			}
			_material.Tilling = Tilling;
			_material.Offset = UVOffset;
			_material.Material?.UpdatePrams();
		}

		public override void OnAttach() {
			base.OnAttach();
			AlbedoTint.Value = Colorf.White;
			EmissionTint.Value = Colorf.Blue;
			Tilling.Value = Vector2f.One;
		}

		public override void UpdateAll() {
			RenderModeChanged();
			BasicColorSettings();
			MetalicAndSmoothness();
			MapsUpdate();
			OtherTexturesUpdate();
			EmissionUpdate();
			UVUpdate();
		}
	}
}
