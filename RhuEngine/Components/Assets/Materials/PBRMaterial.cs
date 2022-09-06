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
		[OnChanged(nameof(AlbedoTextureUVUpdate))]
		public readonly Sync<Vector2f> AlbedoTextureTilling;
		[OnChanged(nameof(AlbedoTextureUVUpdate))]
		public readonly Sync<Vector2f> AlbedoTextureUVOffset;
		[OnChanged(nameof(BasicColorSettings))]
		public readonly Sync<Colorf> AlbedoTint;
		[Default(0.5f)]
		[OnChanged(nameof(BasicColorSettings))]
		public readonly Sync<float> AlphaCutOut;
		[OnAssetLoaded(nameof(MetalicAndSmoothness))]
		public readonly AssetRef<RTexture2D> MetallicTexture;
		[OnChanged(nameof(MetallicTextureUVUpdate))]
		public readonly Sync<Vector2f> MetallicTextureTilling;
		[OnChanged(nameof(MetallicTextureUVUpdate))]
		public readonly Sync<Vector2f> MetallicTextureUVOffset;
		[OnChanged(nameof(MetalicAndSmoothness))]
		public readonly Sync<float> Metallic;
		[Default(0.5f)]
		[OnChanged(nameof(MetalicAndSmoothness))]
		public readonly Sync<float> Smoothness;
		[OnChanged(nameof(MetalicAndSmoothness))]
		public readonly Sync<bool> SmoothnessFromAlbedo;
		[OnAssetLoaded(nameof(MapsUpdate))]
		public readonly AssetRef<RTexture2D> NormalMap;
		[OnChanged(nameof(NormalMapUVUpdate))]
		public readonly Sync<Vector2f> NormalMapTilling;
		[OnChanged(nameof(NormalMapUVUpdate))]
		public readonly Sync<Vector2f> NormalMapUVOffset;
		[OnAssetLoaded(nameof(MapsUpdate))]
		public readonly AssetRef<RTexture2D> HeightMap;
		[OnChanged(nameof(HeightMapUVUpdate))]
		public readonly Sync<Vector2f> HeightMapTilling;
		[OnChanged(nameof(HeightMapUVUpdate))]
		public readonly Sync<Vector2f> HeightMapUVOffset;
		[OnAssetLoaded(nameof(OtherTexturesUpdate))]
		public readonly AssetRef<RTexture2D> Occlusion;
		[OnChanged(nameof(OcclusionUVUpdate))]
		public readonly Sync<Vector2f> OcclusionTilling;
		[OnChanged(nameof(OcclusionUVUpdate))]
		public readonly Sync<Vector2f> OcclusionUVOffset;
		[OnAssetLoaded(nameof(OtherTexturesUpdate))]
		public readonly AssetRef<RTexture2D> DetailMask;
		[OnChanged(nameof(DetailMaskUVUpdate))]
		public readonly Sync<Vector2f> DetailMaskTilling;
		[OnChanged(nameof(DetailMaskUVUpdate))]
		public readonly Sync<Vector2f> DetailMaskUVOffset;
		[OnAssetLoaded(nameof(OtherTexturesUpdate))]
		public readonly AssetRef<RTexture2D> DetailAlbedo;
		[OnChanged(nameof(DetailAlbedoUVUpdate))]
		public readonly Sync<Vector2f> DetailAlbedoTilling;
		[OnChanged(nameof(DetailAlbedoUVUpdate))]
		public readonly Sync<Vector2f> DetailAlbedoUVOffset;
		[OnAssetLoaded(nameof(OtherTexturesUpdate))]
		public readonly AssetRef<RTexture2D> DetailNormal;
		[OnChanged(nameof(DetailNormalUVUpdate))]
		public readonly Sync<Vector2f> DetailNormalTilling;
		[OnChanged(nameof(DetailNormalUVUpdate))]
		public readonly Sync<Vector2f> DetailNormalUVOffset;
		[OnAssetLoaded(nameof(EmissionUpdate))]
		public readonly AssetRef<RTexture2D> EmissionTexture;
		[OnChanged(nameof(EmissionTextureUVUpdate))]
		public readonly Sync<Vector2f> EmissionTextureTilling;
		[OnChanged(nameof(EmissionTextureUVUpdate))]
		public readonly Sync<Vector2f> EmissionTextureUVOffset;
		[OnChanged(nameof(EmissionUpdate))]
		public readonly Sync<Colorf> EmissionTint;
		[OnChanged(nameof(FullUVUpdate))]
		public readonly Sync<Vector2f> Tilling;
		[OnChanged(nameof(FullUVUpdate))]
		public readonly Sync<Vector2f> UVOffset;
		private void RenderModeChanged() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.RenderMode = RenderMode;
				_material.Material?.UpdatePrams();
			});
		}
		private void BasicColorSettings() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.AlbedoTexture = AlbedoTexture.Asset;
				_material.AlbedoTint = AlbedoTint;
				_material.AlphaCutOut = AlphaCutOut;
				_material.Material?.UpdatePrams();
			});
		}
		private void MetalicAndSmoothness() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.MetallicTexture = MetallicTexture.Asset;
				_material.Metallic = Metallic;
				_material.Smoothness = Smoothness;
				_material.SmoothnessFromAlbedo = SmoothnessFromAlbedo;
				_material.Material?.UpdatePrams();
			});
		}
		private void MapsUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.NormalMap = NormalMap.Asset;
				_material.HeightMap = HeightMap.Asset;
				_material.Material?.UpdatePrams();
			});
		}
		private void OtherTexturesUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.Occlusion = Occlusion.Asset;
				_material.DetailMask = DetailMask.Asset;
				_material.DetailAlbedo = DetailAlbedo.Asset;
				_material.DetailNormal = DetailNormal.Asset;
				_material.Material?.UpdatePrams();
			});
		}
		private void EmissionUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.EmissionTexture = EmissionTexture.Asset;
				_material.EmissionTint = EmissionTint;
				_material.Material?.UpdatePrams();
			});
		}
		private void AlbedoTextureUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.AlbedoTextureOffset = UVOffset.Value + AlbedoTextureUVOffset;
				_material.AlbedoTextureTilling = Tilling.Value * AlbedoTextureTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void MetallicTextureUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.MetallicTextureOffset = UVOffset.Value + MetallicTextureUVOffset;
				_material.MetallicTextureTilling = Tilling.Value * MetallicTextureTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void NormalMapUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.NormalMapOffset = UVOffset.Value + NormalMapUVOffset;
				_material.NormalMapTilling = Tilling.Value * NormalMapTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void HeightMapUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.HeightMapOffset = UVOffset.Value + HeightMapUVOffset;
				_material.HeightMapTilling = Tilling.Value * HeightMapTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void OcclusionUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.OcclusionOffset = UVOffset.Value + OcclusionUVOffset;
				_material.OcclusionTilling = Tilling.Value * OcclusionTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void DetailMaskUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.DetailMaskOffset = UVOffset.Value + DetailMaskUVOffset;
				_material.DetailMaskTilling = Tilling.Value * DetailMaskTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void DetailAlbedoUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.DetailAlbedoOffset = UVOffset.Value + DetailAlbedoUVOffset;
				_material.DetailAlbedoTilling = Tilling.Value * DetailAlbedoTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void DetailNormalUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.DetailNormalOffset = UVOffset.Value + DetailNormalUVOffset;
				_material.DetailNormalTilling = Tilling.Value * DetailNormalTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void EmissionTextureUVUpdate() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				if (_material is null) {
					return;
				}
				_material.EmissionTextureOffset = UVOffset.Value + EmissionTextureUVOffset;
				_material.EmissionTextureTilling = Tilling.Value * EmissionTextureTilling;
				_material.Material?.UpdatePrams();
			});
		}

		private void FullUVUpdate() {
			AlbedoTextureUVUpdate();
			MetallicTextureUVUpdate();
			NormalMapUVUpdate();
			HeightMapUVUpdate();
			OcclusionUVUpdate();
			DetailMaskUVUpdate();
			DetailAlbedoUVUpdate();
			DetailNormalUVUpdate();
			EmissionTextureUVUpdate();
		}

		protected override void OnAttach() {
			base.OnAttach();
			AlbedoTint.Value = Colorf.White;
			EmissionTint.Value = Colorf.Blue;
			Tilling.Value = Vector2f.One;
			AlbedoTextureTilling.Value = Vector2f.One;
			DetailAlbedoTilling.Value = Vector2f.One;
			DetailMaskTilling.Value = Vector2f.One;
			DetailNormalTilling.Value = Vector2f.One;
			EmissionTextureTilling.Value = Vector2f.One;
			MetallicTextureTilling.Value = Vector2f.One;
			NormalMapTilling.Value = Vector2f.One;
			HeightMapTilling.Value = Vector2f.One;
			OcclusionTilling.Value = Vector2f.One;
		}

		protected override void UpdateAll() {
			RenderModeChanged();
			BasicColorSettings();
			MetalicAndSmoothness();
			MapsUpdate();
			OtherTexturesUpdate();
			EmissionUpdate();
			FullUVUpdate();
		}
	}
}
