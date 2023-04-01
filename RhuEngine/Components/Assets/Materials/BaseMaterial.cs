using System;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Components
{
	public enum BaseMaterialSamplingFilter : byte
	{
		Nearest,
		Linear,
		NearestMipMap,
		LinearMipMap,
		NearestAnisotropic,
		LinearAnisotropic,
	}
	public enum BaseMaterialTransparency : byte
	{
		Disabled,
		Alpha,
		AlphaScissor,
		AlphaHash,
		AlphaDepthPrePass,
	}

	public enum BaseMaterialAlphaAntialiasingMode : byte
	{
		Off,
		AlphaToCoverage,
		AlphaToCoverageAndToOne
	}

	public enum BaseMaterialBlendMode : byte
	{
		Mix,
		Add,
		Subtract,
		Multiply,
	}

	public enum BaseMaterialCullMode : byte
	{
		Back,
		Front,
		Disabled,
	}

	public enum BaseMaterialDepthMode : byte
	{
		OpaqueOnly,
		Always,
		Never,
	}
	public enum BaseMaterialShadingMode : byte
	{
		Unshaded,
		PerPixel,
		PerVertex,
	}

	public enum BaseMaterialDiffuseMode : byte
	{
		Burley,
		Lambert,
		LambertWarp,
		Toon,
	}
	public enum BaseMaterialSpecularMode : byte
	{
		Disabled,
		Toon,
		SchlickGGX,
	}


	[Category(new string[] { "Assets/Materials" })]
	[AllowedOnWorldRoot]
	public abstract partial class BaseMaterial<T> : MaterialBase<T> where T : RBaseMaterial, new()
	{
		[OnChanged(nameof(TransparencyChange))] public readonly Sync<BaseMaterialTransparency> Transparency;
		[Default(0.5f)][OnChanged(nameof(AlphaScissorThresholdChange))] public readonly Sync<float> AlphaScissorThreshold;
		[OnChanged(nameof(AlphaAntialiasingModeChange))]
		public readonly Sync<BaseMaterialAlphaAntialiasingMode> AlphaAntialiasingMode;
		[Default(0.3f)][OnChanged(nameof(AlphaAntialiasingEdgeChange))] public readonly Sync<float> AlphaAntialiasingEdge;
		[Default(BaseMaterialBlendMode.Mix)]
		[OnChanged(nameof(BlendModeChange))] public readonly Sync<BaseMaterialBlendMode> BlendMode;
		[OnChanged(nameof(CullModeChange))] public readonly Sync<BaseMaterialCullMode> CullMode;
		[OnChanged(nameof(DepthModeChange))] public readonly Sync<BaseMaterialDepthMode> DepthMode;
		[OnChanged(nameof(NoDepthTestChange))] public readonly Sync<bool> NoDepthTest;
		[OnChanged(nameof(UseAsAlbedoChange))] public readonly Sync<bool> UseAsAlbedo;
		[OnChanged(nameof(IssRGBChange))] public readonly Sync<bool> IssRGB;
		[Default(BaseMaterialShadingMode.PerPixel)]
		[OnChanged(nameof(ShadingModeChange))] public readonly Sync<BaseMaterialShadingMode> ShadingMode;
		[Default(BaseMaterialDiffuseMode.Burley)]
		[OnChanged(nameof(DiffuseModeChange))] public readonly Sync<BaseMaterialDiffuseMode> DiffuseMode;
		[Default(BaseMaterialSpecularMode.SchlickGGX)]
		[OnChanged(nameof(SpecularModeChange))] public readonly Sync<BaseMaterialSpecularMode> SpecularMode;
		[OnChanged(nameof(DisableAmbientLightChange))] public readonly Sync<bool> DisableAmbientLight;
		[OnChanged(nameof(AlbedoColorChange))] public readonly Sync<Colorf> AlbedoColor;
		[OnAssetLoaded(nameof(AlbedoTextureAsset))] public readonly AssetRef<RTexture2D> AlbedoTexture;
		[OnChanged(nameof(AlbedoTextureForcesRGBChange))] public readonly Sync<bool> AlbedoTextureForcesRGB;
		[OnChanged(nameof(AlbedoTextureMSDFChange))] public readonly Sync<bool> AlbedoTextureMSDF;
		[Default(4)][OnChanged(nameof(MSDFPixelRageChange))] public readonly Sync<int> MSDFPixelRage;
		[Default(1)][OnChanged(nameof(MSDFOutlineSizeChange))] public readonly Sync<int> MSDFOutlineSize;
		[OnChanged(nameof(UV1ScaleChange))] public readonly Sync<Vector3f> UV1Scale;
		[OnChanged(nameof(UV1OffsetChange))] public readonly Sync<Vector3f> UV1Offset;
		[OnChanged(nameof(UV1TriplaneChange))] public readonly Sync<bool> UV1Triplane;
		[OnChanged(nameof(UV2ScaleChange))] public readonly Sync<Vector3f> UV2Scale;
		[OnChanged(nameof(UV2OffsetChange))] public readonly Sync<Vector3f> UV2Offset;
		[OnChanged(nameof(UV2TriplaneChange))] public readonly Sync<bool> UV2Triplane;
		[OnChanged(nameof(ShadowOpacityChange))] public readonly Sync<bool> ShadowOpacity;
		[OnChanged(nameof(ReceiveShadowChange))] public readonly Sync<bool> ReceiveShadow;
		[OnChanged(nameof(ShadowToOpacityChange))] public readonly Sync<bool> ShadowToOpacity;
		[OnChanged(nameof(SamplingRepeatChange))] public readonly Sync<bool> SamplingRepeat;
		[OnChanged(nameof(SamplingFilterChange))] public readonly Sync<BaseMaterialSamplingFilter> SamplingFilter;

		public void TransparencyChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.Transparency = Transparency.Value); }
		public void AlphaScissorThresholdChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlphaScissorThreshold = AlphaScissorThreshold.Value); }
		public void AlphaAntialiasingModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlphaAntialiasingMode = AlphaAntialiasingMode.Value); }
		public void AlphaAntialiasingEdgeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlphaAntialiasingEdge = AlphaAntialiasingEdge.Value); }
		public void BlendModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.BlendMode = BlendMode.Value); }
		public void CullModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.CullMode = CullMode.Value); }
		public void DepthModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.DepthMode = DepthMode.Value); }
		public void NoDepthTestChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.NoDepthTest = NoDepthTest.Value); }
		public void UseAsAlbedoChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UseAsAlbedo = UseAsAlbedo.Value); }
		public void IssRGBChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.IssRGB = IssRGB.Value); }
		public void ShadingModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.ShadingMode = ShadingMode.Value); }
		public void DiffuseModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.DiffuseMode = DiffuseMode.Value); }
		public void SpecularModeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.SpecularMode = SpecularMode.Value); }
		public void DisableAmbientLightChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.DisableAmbientLight = DisableAmbientLight.Value); }
		public void AlbedoColorChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlbedoColor = AlbedoColor.Value); }
		public void AlbedoTextureAsset(RTexture2D _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlbedoTexture = AlbedoTexture.Asset); }
		public void AlbedoTextureForcesRGBChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlbedoTextureForcesRGB = AlbedoTextureForcesRGB.Value); }
		public void AlbedoTextureMSDFChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.AlbedoTextureMSDF = AlbedoTextureMSDF.Value); }
		public void MSDFPixelRageChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.MSDFPixelRage = MSDFPixelRage.Value); }
		public void MSDFOutlineSizeChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.MSDFOutlineSize = MSDFOutlineSize.Value); }
		public void UV1ScaleChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UV1Scale = UV1Scale.Value); }
		public void UV1OffsetChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UV1Offset = UV1Offset.Value); }
		public void UV1TriplaneChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UV1Triplane = UV1Triplane.Value); }
		public void UV2ScaleChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UV2Scale = UV2Scale.Value); }
		public void UV2OffsetChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UV2Offset = UV2Offset.Value); }
		public void UV2TriplaneChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.UV2Triplane = UV2Triplane.Value); }
		public void ShadowOpacityChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.ShadowOpacity = ShadowOpacity.Value); }
		public void ReceiveShadowChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.ReceiveShadow = ReceiveShadow.Value); }
		public void ShadowToOpacityChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.ShadowToOpacity = ShadowToOpacity.Value); }
		public void SamplingRepeatChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.SamplingRepeat = SamplingRepeat.Value); }
		public void SamplingFilterChange(IChangeable _) { RenderThread.ExecuteOnStartOfFrame(() => _material.SamplingFilter = SamplingFilter.Value); }



		[OnAssetLoaded(nameof(EmissionLoad))]
		public readonly AssetRef<EmissionMaterialFeatere> Emission;

		public void EmissionLoad(EmissionMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.EmissionLoad(Emission.Asset));
		}

		[OnAssetLoaded(nameof(NormalMapLoad))]
		public readonly AssetRef<NormalMapMaterialFeatere> NormalMap;

		public void NormalMapLoad(NormalMapMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.NormalMapLoad(NormalMap.Asset));
		}

		[OnAssetLoaded(nameof(RimLoad))]
		public readonly AssetRef<RimMaterialFeatere> Rim;

		public void RimLoad(RimMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.RimLoad(Rim.Asset));
		}

		[OnAssetLoaded(nameof(AmbientOcclusionLoad))]
		public readonly AssetRef<AmbientOcclusionMaterialFeatere> AmbientOcclusion;

		public void AmbientOcclusionLoad(AmbientOcclusionMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.AmbientOcclusionLoad(AmbientOcclusion.Asset));
		}

		[OnAssetLoaded(nameof(AnisotropyLoad))]
		public readonly AssetRef<AnisotropyMaterialFeatere> Anisotropy;

		public void AnisotropyLoad(AnisotropyMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.AnisotropyLoad(Anisotropy.Asset));
		}

		[OnAssetLoaded(nameof(BackLightLoad))]
		public readonly AssetRef<BackLightMaterialFeatere> BackLight;

		public void BackLightLoad(BackLightMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.BackLightLoad(BackLight.Asset));
		}

		[OnAssetLoaded(nameof(BillboardLoad))]
		public readonly AssetRef<BillboardMaterialFeatere> Billboard;

		public void BillboardLoad(BillboardMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.BillboardLoad(Billboard.Asset));
		}


		[OnAssetLoaded(nameof(ClearcoatLoad))]
		public readonly AssetRef<ClearcoatMaterialFeatere> Clearcoat;

		public void ClearcoatLoad(ClearcoatMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.ClearcoatLoad(Clearcoat.Asset));
		}

		[OnAssetLoaded(nameof(DetailLoad))]
		public readonly AssetRef<DetailMaterialFeatere> Detail;

		public void DetailLoad(DetailMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.DetailLoad(Detail.Asset));
		}

		[OnAssetLoaded(nameof(DistanceFadeLoad))]
		public readonly AssetRef<DistanceFadeMaterialFeatere> DistanceFade;

		public void DistanceFadeLoad(DistanceFadeMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.DistanceFadeLoad(DistanceFade.Asset));
		}

		[OnAssetLoaded(nameof(GrowLoad))]
		public readonly AssetRef<GrowMaterialFeatere> Grow;

		public void GrowLoad(GrowMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.GrowLoad(Grow.Asset));
		}

		[OnAssetLoaded(nameof(HeightLoad))]
		public readonly AssetRef<HeightMaterialFeatere> Height;

		public void HeightLoad(HeightMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.HeightLoad(Height.Asset));
		}

		[OnAssetLoaded(nameof(ProximityFadeLoad))]
		public readonly AssetRef<ProximityFadeMaterialFeatere> ProximityFade;

		public void ProximityFadeLoad(ProximityFadeMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.ProximityFadeLoad(ProximityFade.Asset));
		}

		[OnAssetLoaded(nameof(RefractionLoad))]
		public readonly AssetRef<RefractionMaterialFeatere> Refraction;

		public void RefractionLoad(RefractionMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.RefractionLoad(Refraction.Asset));
		}

		[OnAssetLoaded(nameof(SubsurfaceScatteringLoad))]
		public readonly AssetRef<SubsurfaceScatteringMaterialFeatere> SubsurfaceScattering;

		public void SubsurfaceScatteringLoad(SubsurfaceScatteringMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.SubsurfaceScatteringLoad(SubsurfaceScattering.Asset));
		}

		[OnAssetLoaded(nameof(TransformLoad))]
		public readonly AssetRef<TransformMaterialFeatere> Transform;

		public void TransformLoad(TransformMaterialFeatere _) {
			RenderThread.ExecuteOnStartOfFrame(() => _material.TransformLoad(Transform.Asset));
		}

		protected override void FirstCreation() {
			base.FirstCreation();
			AlbedoColor.Value = Colorf.White;
			UV1Scale.Value = Vector3f.One;
			UV2Scale.Value = Vector3f.One;
		}

		protected override void UpdateAll() {
			EmissionLoad(null);
			NormalMapLoad(null);
			RimLoad(null);
			AmbientOcclusionLoad(null);
			AnisotropyLoad(null);
			BackLightLoad(null);
			BillboardLoad(null);
			DistanceFadeLoad(null);
			GrowLoad(null);
			HeightLoad(null);
			ProximityFadeLoad(null);
			RefractionLoad(null);
			SubsurfaceScatteringLoad(null);
			SubsurfaceScatteringLoad(null);
			TransformLoad(null);
			TransparencyChange(null);
			AlphaScissorThresholdChange(null);
			AlphaAntialiasingModeChange(null);
			AlphaAntialiasingEdgeChange(null);
			BlendModeChange(null);
			CullModeChange(null);
			DepthModeChange(null);
			NoDepthTestChange(null);
			UseAsAlbedoChange(null);
			IssRGBChange(null);
			ShadingModeChange(null);
			DiffuseModeChange(null);
			SpecularModeChange(null);
			DisableAmbientLightChange(null);
			AlbedoColorChange(null);
			AlbedoTextureAsset(null);
			AlbedoTextureForcesRGBChange(null);
			AlbedoTextureMSDFChange(null);
			MSDFPixelRageChange(null);
			MSDFOutlineSizeChange(null);
			UV1ScaleChange(null);
			UV1OffsetChange(null);
			UV1TriplaneChange(null);
			UV2ScaleChange(null);
			UV2OffsetChange(null);
			UV2TriplaneChange(null);
			ShadowOpacityChange(null);
			ReceiveShadowChange(null);
			ShadowToOpacityChange(null);
			SamplingRepeatChange(null);
			SamplingFilterChange(null);
		}

	}
}
