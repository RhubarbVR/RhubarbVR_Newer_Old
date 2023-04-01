using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Components;

using RNumerics;

using WebAssembly;

using static System.Net.Mime.MediaTypeNames;

namespace RhuEngine.Linker
{
	public interface IRMaterial : IDisposable
	{
		public RMaterial NextPass { set; }
		public int GetRenderPriority();
		public void SetRenderPriority(int renderPri);
	}


	public interface IUnlitMaterial : IRMaterial
	{
		public bool NoDepthTest { set; }

		public bool DullSided { set; }

		public Transparency Transparency { set; }

		public RTexture2D Texture { set; }

		public Colorf Tint { set; }
	}


	public class RUnlitMaterial : RMaterial
	{
		public IUnlitMaterial UnlitMaterial => (IUnlitMaterial)Inst;

		public static Type Instance { get; set; }

		public RUnlitMaterial() : this(null) {

		}

		public RUnlitMaterial(IRMaterial target) : base(target) {
			if (typeof(RUnlitMaterial) == GetType()) {
				Inst = target ?? (IUnlitMaterial)Activator.CreateInstance(Instance);
			}
		}

		public bool NoDepthTest
		{
			set => UnlitMaterial.NoDepthTest = value;
		}

		public bool DullSided
		{
			set => UnlitMaterial.DullSided = value;
		}


		public Transparency Transparency
		{
			set => UnlitMaterial.Transparency = value;
		}


		public RTexture2D Texture
		{
			set => UnlitMaterial.Texture = value;
		}


		public Colorf Tint
		{
			set => UnlitMaterial.Tint = value;
		}

	}

	public class RMaterial : IDisposable
	{
		private RMaterial _nextPass;
		public RMaterial NextPass
		{
			get => _nextPass;
			set {
				_nextPass = value;
				Inst.NextPass = value;
			}
		}


		public int RenderPriority
		{
			set {
				lock (this) {
					Inst.SetRenderPriority(value);
				}
			}
			get {
				lock (this) {
					return Inst.GetRenderPriority();
				}
			}
		}

		public IRMaterial Inst { get; set; }

		public RMaterial(IRMaterial target) {
			if (typeof(RMaterial) == GetType()) {
				Inst = target;
			}
		}

		public event Action<RMaterial> OnDispose;

		public void Dispose() {
			OnDispose?.Invoke(this);
			Inst?.Dispose();
			Inst = null;
			GC.SuppressFinalize(this);
		}
	}

	public interface IBaseMaterial : IRMaterial
	{
		BaseMaterialSamplingFilter SamplingFilter { set; }
		bool SamplingRepeat { set; }
		bool ShadowToOpacity { set; }
		bool ReceiveShadow { set; }
		bool ShadowOpacity { set; }
		bool UV2Triplane { set; }
		Vector3f UV2Offset { set; }
		Vector3f UV2Scale { set; }
		bool UV1Triplane { set; }
		Vector3f UV1Offset { set; }
		Vector3f UV1Scale { set; }
		int MSDFOutlineSize { set; }
		int MSDFPixelRage { set; }
		bool AlbedoTextureMSDF { set; }
		bool AlbedoTextureForcesRGB { set; }
		RTexture2D AlbedoTexture { set; }
		Colorf AlbedoColor { set; }
		bool DisableAmbientLight { set; }
		BaseMaterialSpecularMode SpecularMode { set; }
		BaseMaterialDiffuseMode DiffuseMode { set; }
		BaseMaterialShadingMode ShadingMode { set; }
		bool IssRGB { set; }
		bool UseAsAlbedo { set; }
		bool NoDepthTest { set; }
		BaseMaterialDepthMode DepthMode { set; }
		BaseMaterialCullMode CullMode { set; }
		BaseMaterialBlendMode BlendMode { set; }
		float AlphaAntialiasingEdge { set; }
		BaseMaterialAlphaAntialiasingMode AlphaAntialiasingMode { set; }
		float AlphaScissorThreshold { set; }
		BaseMaterialTransparency Transparency { set; }

		public void EmissionLoad(EmissionMaterialFeatere asset);
		public void NormalMapLoad(NormalMapMaterialFeatere asset);
		public void RimLoad(RimMaterialFeatere asset);
		public void AmbientOcclusionLoad(AmbientOcclusionMaterialFeatere asset);
		public void AnisotropyLoad(AnisotropyMaterialFeatere asset);
		public void BackLightLoad(BackLightMaterialFeatere asset);
		public void BillboardLoad(BillboardMaterialFeatere asset);
		public void ClearcoatLoad(ClearcoatMaterialFeatere asset);
		public void DetailLoad(DetailMaterialFeatere asset);
		public void DistanceFadeLoad(DistanceFadeMaterialFeatere asset);
		public void GrowLoad(GrowMaterialFeatere asset);
		public void HeightLoad(HeightMaterialFeatere asset);
		public void ProximityFadeLoad(ProximityFadeMaterialFeatere asset);
		public void RefractionLoad(RefractionMaterialFeatere asset);
		public void SubsurfaceScatteringLoad(SubsurfaceScatteringMaterialFeatere asset);
		public void TransformLoad(TransformMaterialFeatere asset);
	}

	public class RBaseMaterial : RMaterial
	{
		public IBaseMaterial BaseMaterial => (IBaseMaterial)Inst;

		public BaseMaterialSamplingFilter SamplingFilter { set => BaseMaterial.SamplingFilter = value; }
		public bool SamplingRepeat { set => BaseMaterial.SamplingRepeat = value; }
		public bool ShadowToOpacity { set => BaseMaterial.ShadowToOpacity = value; }
		public bool ReceiveShadow { set => BaseMaterial.ReceiveShadow = value; }
		public bool ShadowOpacity { set => BaseMaterial.ShadowOpacity = value; }
		public bool UV2Triplane { set => BaseMaterial.UV2Triplane = value; }
		public Vector3f UV2Offset { set => BaseMaterial.UV2Offset = value; }
		public Vector3f UV2Scale { set => BaseMaterial.UV2Scale = value; }
		public bool UV1Triplane { set => BaseMaterial.UV1Triplane = value; }
		public Vector3f UV1Offset { set => BaseMaterial.UV1Offset = value; }
		public Vector3f UV1Scale { set => BaseMaterial.UV1Scale = value; }
		public int MSDFOutlineSize { set => BaseMaterial.MSDFOutlineSize = value; }
		public int MSDFPixelRage { set => BaseMaterial.MSDFPixelRage = value; }
		public bool AlbedoTextureMSDF { set => BaseMaterial.AlbedoTextureMSDF = value; }
		public bool AlbedoTextureForcesRGB { set => BaseMaterial.AlbedoTextureForcesRGB = value; }
		public RTexture2D AlbedoTexture { set => BaseMaterial.AlbedoTexture = value; }
		public Colorf AlbedoColor { set => BaseMaterial.AlbedoColor = value; }
		public bool DisableAmbientLight { set => BaseMaterial.DisableAmbientLight = value; }
		public BaseMaterialSpecularMode SpecularMode { set => BaseMaterial.SpecularMode = value; }
		public BaseMaterialDiffuseMode DiffuseMode { set => BaseMaterial.DiffuseMode = value; }
		public BaseMaterialShadingMode ShadingMode { set => BaseMaterial.ShadingMode = value; }
		public bool IssRGB { set => BaseMaterial.IssRGB = value; }
		public bool UseAsAlbedo { set => BaseMaterial.UseAsAlbedo = value; }
		public bool NoDepthTest { set => BaseMaterial.NoDepthTest = value; }
		public BaseMaterialDepthMode DepthMode { set => BaseMaterial.DepthMode = value; }
		public BaseMaterialCullMode CullMode { set => BaseMaterial.CullMode = value; }
		public BaseMaterialBlendMode BlendMode { set => BaseMaterial.BlendMode = value; }
		public float AlphaAntialiasingEdge { set => BaseMaterial.AlphaAntialiasingEdge = value; }
		public BaseMaterialAlphaAntialiasingMode AlphaAntialiasingMode { set => BaseMaterial.AlphaAntialiasingMode = value; }
		public float AlphaScissorThreshold { set => BaseMaterial.AlphaScissorThreshold = value; }
		public BaseMaterialTransparency Transparency { set => BaseMaterial.Transparency = value; }

		public RBaseMaterial(IRMaterial target) : base(target) {

		}

		public void EmissionLoad(EmissionMaterialFeatere asset) {
			BaseMaterial.EmissionLoad(asset);
		}

		public void NormalMapLoad(NormalMapMaterialFeatere asset) {
			BaseMaterial.NormalMapLoad(asset);
		}

		public void RimLoad(RimMaterialFeatere asset) {
			BaseMaterial.RimLoad(asset);
		}

		public void AmbientOcclusionLoad(AmbientOcclusionMaterialFeatere asset) {
			BaseMaterial.AmbientOcclusionLoad(asset);
		}

		public void AnisotropyLoad(AnisotropyMaterialFeatere asset) {
			BaseMaterial.AnisotropyLoad(asset);
		}

		public void BackLightLoad(BackLightMaterialFeatere asset) {
			BaseMaterial.BackLightLoad(asset);
		}

		public void BillboardLoad(BillboardMaterialFeatere asset) {
			BaseMaterial.BillboardLoad(asset);
		}

		public void ClearcoatLoad(ClearcoatMaterialFeatere asset) {
			BaseMaterial.ClearcoatLoad(asset);
		}

		public void DetailLoad(DetailMaterialFeatere asset) {
			BaseMaterial.DetailLoad(asset);
		}

		public void DistanceFadeLoad(DistanceFadeMaterialFeatere asset) {
			BaseMaterial.DistanceFadeLoad(asset);
		}

		public void GrowLoad(GrowMaterialFeatere asset) {
			BaseMaterial.GrowLoad(asset);
		}

		public void HeightLoad(HeightMaterialFeatere asset) {
			BaseMaterial.HeightLoad(asset);
		}

		public void ProximityFadeLoad(ProximityFadeMaterialFeatere asset) {
			BaseMaterial.ProximityFadeLoad(asset);
		}

		public void RefractionLoad(RefractionMaterialFeatere asset) {
			BaseMaterial.RefractionLoad(asset);
		}

		public void SubsurfaceScatteringLoad(SubsurfaceScatteringMaterialFeatere asset) {
			BaseMaterial.SubsurfaceScatteringLoad(asset);
		}

		public void TransformLoad(TransformMaterialFeatere asset) {
			BaseMaterial.TransformLoad(asset);
		}
	}

	public interface IStandardMaterial : IBaseMaterial
	{
		float Metallic { set; }
		float Specular { set; }
		RTexture2D MetallicTexture { set; }
		TextureChannel MetallicChannel { set; }
		float Roughness { set; }
		RTexture2D RoughnessTexture { set; }
		TextureChannel RoughnessChannel { set; }
	}

	public class RStandardMaterial : RBaseMaterial
	{
		public IStandardMaterial StandardMaterial => (IStandardMaterial)Inst;
		public static Type Instance { get; set; }

		public float Metallic { set => StandardMaterial.Metallic = value; }
		public float Specular { set => StandardMaterial.Specular = value; }
		public RTexture2D MetallicTexture { set => StandardMaterial.MetallicTexture = value; }
		public TextureChannel MetallicChannel { set => StandardMaterial.MetallicChannel = value; }
		public float Roughness { set => StandardMaterial.Roughness = value; }
		public RTexture2D RoughnessTexture { set => StandardMaterial.RoughnessTexture = value; }
		public TextureChannel RoughnessChannel { set => StandardMaterial.RoughnessChannel = value; }

		public RStandardMaterial() : this(null) {

		}

		public RStandardMaterial(IRMaterial target) : base(target) {
			if (typeof(RStandardMaterial) == GetType()) {
				Inst = target ?? (IStandardMaterial)Activator.CreateInstance(Instance);
			}
		}

	}


	public interface IORMMaterial : IBaseMaterial
	{
		RTexture2D ORMTexture { set; }
	}

	public class RORMMaterial : RBaseMaterial
	{
		public IORMMaterial ORMMaterial => (IORMMaterial)Inst;
		public static Type Instance { get; set; }

		public RTexture2D ORMTexture { set => ORMMaterial.ORMTexture = value; }

		public RORMMaterial() : this(null) {

		}

		public RORMMaterial(IRMaterial target) : base(target) {
			if (typeof(RORMMaterial) == GetType()) {
				Inst = target ?? (IORMMaterial)Activator.CreateInstance(Instance);
			}
		}

	}
}
