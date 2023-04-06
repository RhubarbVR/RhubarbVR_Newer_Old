using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;

using RNumerics;
using Godot;
using System.Xml.Linq;
using Godot.Collections;
using Array = Godot.Collections.Array;
using SArray = System.Array;
using RhubarbVR.Bindings.TextureBindings;
using RhuEngine.Components;
using Newtonsoft.Json.Linq;

namespace RhubarbVR.Bindings
{

	public class GodotMaterial : IRMaterial
	{
		public Material Material { get; set; }
		public RMaterial NextPass { set => Material.NextPass = value is null ? null : ((GodotMaterial)value.Inst).Material; }

		public GodotMaterial(Material material) {
			Material = material;
		}

		public int GetRenderPriority() {
			return Material.RenderPriority;
		}

		public void SetRenderPriority(int renderPri) {
			Material.RenderPriority = renderPri;
		}

		public void Dispose() {
			Material = null;
			GC.SuppressFinalize(this);
		}
	}

	public abstract class GodotBaseMaterial<T> : GodotMaterial, IBaseMaterial where T : BaseMaterial3D, new()
	{
		public BaseMaterial3D BaseMaterial3D => Material as BaseMaterial3D;

		public BaseMaterialSamplingFilter SamplingFilter
		{
			set {
				BaseMaterial3D.TextureFilter = value switch {
					BaseMaterialSamplingFilter.Linear => BaseMaterial3D.TextureFilterEnum.Linear,
					BaseMaterialSamplingFilter.NearestMipMap => BaseMaterial3D.TextureFilterEnum.NearestWithMipmaps,
					BaseMaterialSamplingFilter.LinearMipMap => BaseMaterial3D.TextureFilterEnum.LinearWithMipmaps,
					BaseMaterialSamplingFilter.NearestAnisotropic => BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic,
					BaseMaterialSamplingFilter.LinearAnisotropic => BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic,
					_ => BaseMaterial3D.TextureFilterEnum.Nearest,
				};
			}
		}
		public bool SamplingRepeat { set => BaseMaterial3D.TextureRepeat = value; }
		public bool ShadowToOpacity { set => BaseMaterial3D.ShadowToOpacity = value; }
		public bool ReceiveShadow { set => BaseMaterial3D.DisableReceiveShadows = value; }
		public bool ShadowOpacity { set => BaseMaterial3D.ShadowToOpacity = value; }
		public bool UV2Triplane { set => BaseMaterial3D.UV2Triplanar = value; }
		public Vector3f UV2Offset { set => BaseMaterial3D.UV2Offset = new Vector3(value.X, value.Y, value.Z); }
		public Vector3f UV2Scale { set => BaseMaterial3D.UV2Scale = new Vector3(value.X, value.Y, value.Z); }
		public bool UV1Triplane { set => BaseMaterial3D.Uv1Triplanar = value; }
		public Vector3f UV1Offset { set => BaseMaterial3D.Uv1Offset = new Vector3(value.X, value.Y, value.Z); }
		public Vector3f UV1Scale { set => BaseMaterial3D.Uv1Scale = new Vector3(value.X, value.Y, value.Z); }
		public int MSDFOutlineSize { set => BaseMaterial3D.MsdfOutlineSize = value; }
		public int MSDFPixelRage { set => BaseMaterial3D.MsdfPixelRange = value; }
		public bool AlbedoTextureMSDF { set => BaseMaterial3D.AlbedoTextureMsdf = value; }
		public bool AlbedoTextureForcesRGB { set => BaseMaterial3D.AlbedoTextureForceSrgb = value; }
		public RTexture2D AlbedoTexture { set => BaseMaterial3D.AlbedoTexture = value is null ? null : ((GodotTexture2D)value.Texture2D).Texture2D; }
		public Colorf AlbedoColor { set => BaseMaterial3D.AlbedoColor = new Color(value.r, value.g, value.b, value.a); }
		public bool DisableAmbientLight { set => BaseMaterial3D.DisableAmbientLight = value; }
		public BaseMaterialSpecularMode SpecularMode
		{
			set {
				BaseMaterial3D.SpecularMode = value switch {
					BaseMaterialSpecularMode.Toon => BaseMaterial3D.SpecularModeEnum.Toon,
					BaseMaterialSpecularMode.SchlickGGX => BaseMaterial3D.SpecularModeEnum.SchlickGgx,
					_ => BaseMaterial3D.SpecularModeEnum.Disabled,
				};
			}
		}
		public BaseMaterialDiffuseMode DiffuseMode
		{
			set {
				BaseMaterial3D.DiffuseMode = value switch {
					BaseMaterialDiffuseMode.Lambert => BaseMaterial3D.DiffuseModeEnum.Lambert,
					BaseMaterialDiffuseMode.LambertWarp => BaseMaterial3D.DiffuseModeEnum.LambertWrap,
					BaseMaterialDiffuseMode.Toon => BaseMaterial3D.DiffuseModeEnum.Toon,
					_ => BaseMaterial3D.DiffuseModeEnum.Burley,
				};
			}
		}
		public BaseMaterialShadingMode ShadingMode
		{
			set {
				BaseMaterial3D.ShadingMode = value switch {
					BaseMaterialShadingMode.PerPixel => BaseMaterial3D.ShadingModeEnum.PerPixel,
					BaseMaterialShadingMode.PerVertex => BaseMaterial3D.ShadingModeEnum.PerVertex,
					_ => BaseMaterial3D.ShadingModeEnum.Unshaded,
				};
			}
		}
		public bool IssRGB { set => BaseMaterial3D.AlbedoTextureForceSrgb = value; }
		public bool UseAsAlbedo { set => BaseMaterial3D.VertexColorUseAsAlbedo = value; }
		public bool NoDepthTest { set => BaseMaterial3D.NoDepthTest = value; }
		public BaseMaterialDepthMode DepthMode
		{
			set {
				BaseMaterial3D.DepthDrawMode = value switch {
					BaseMaterialDepthMode.Always => BaseMaterial3D.DepthDrawModeEnum.Always,
					BaseMaterialDepthMode.Never => BaseMaterial3D.DepthDrawModeEnum.Disabled,
					_ => BaseMaterial3D.DepthDrawModeEnum.OpaqueOnly,
				};
			}
		}
		public BaseMaterialCullMode CullMode
		{
			set {
				BaseMaterial3D.CullMode = value switch {
					BaseMaterialCullMode.Front => BaseMaterial3D.CullModeEnum.Front,
					BaseMaterialCullMode.Disabled => BaseMaterial3D.CullModeEnum.Disabled,
					_ => BaseMaterial3D.CullModeEnum.Back,
				};
			}
		}
		public BaseMaterialBlendMode BlendMode
		{
			set {
				BaseMaterial3D.BlendMode = value switch {
					BaseMaterialBlendMode.Add => BaseMaterial3D.BlendModeEnum.Add,
					BaseMaterialBlendMode.Subtract => BaseMaterial3D.BlendModeEnum.Sub,
					BaseMaterialBlendMode.Multiply => BaseMaterial3D.BlendModeEnum.Mul,
					_ => BaseMaterial3D.BlendModeEnum.Mix,
				};
			}
		}
		public float AlphaAntialiasingEdge { set => BaseMaterial3D.AlphaAntialiasingEdge = value; }
		public BaseMaterialAlphaAntialiasingMode AlphaAntialiasingMode
		{
			set {
				BaseMaterial3D.AlphaAntialiasingMode = value switch {
					BaseMaterialAlphaAntialiasingMode.AlphaToCoverage => BaseMaterial3D.AlphaAntiAliasing.AlphaToCoverage,
					BaseMaterialAlphaAntialiasingMode.AlphaToCoverageAndToOne => BaseMaterial3D.AlphaAntiAliasing.AlphaToCoverageAndToOne,
					_ => BaseMaterial3D.AlphaAntiAliasing.Off,
				};
			}
		}
		public float AlphaScissorThreshold { set => BaseMaterial3D.AlphaScissorThreshold = value; }
		public BaseMaterialTransparency Transparency
		{
			set {
				BaseMaterial3D.Transparency = value switch {
					BaseMaterialTransparency.Alpha => BaseMaterial3D.TransparencyEnum.Alpha,
					BaseMaterialTransparency.AlphaScissor => BaseMaterial3D.TransparencyEnum.AlphaScissor,
					BaseMaterialTransparency.AlphaHash => BaseMaterial3D.TransparencyEnum.AlphaHash,
					BaseMaterialTransparency.AlphaDepthPrePass => BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass,
					_ => BaseMaterial3D.TransparencyEnum.Disabled,
				};
			}
		}

		public GodotBaseMaterial() : base(new T()) {
		}

		public void EmissionLoad(EmissionMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.EmissionEnabled = false;
				return;
			}
			BaseMaterial3D.EmissionEnabled = true;
			BaseMaterial3D.Emission = new Color(asset.Color.Value.r, asset.Color.Value.g, asset.Color.Value.b, asset.Color.Value.a);
			BaseMaterial3D.EmissionOnUV2 = asset.UV2.Value;
			BaseMaterial3D.EmissionEnergyMultiplier = asset.EnergyMultiplier.Value;
			BaseMaterial3D.EmissionOperator = asset.Operator.Value switch {
				EmissionMaterialFeatere.EmissionOperator.Multiply => BaseMaterial3D.EmissionOperatorEnum.Multiply,
				_ => BaseMaterial3D.EmissionOperatorEnum.Add,
			};
			BaseMaterial3D.EmissionEnergyMultiplier = asset.EnergyMultiplier.Value;
			BaseMaterial3D.EmissionTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
		}

		public void NormalMapLoad(NormalMapMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.NormalEnabled = false;
				return;
			}
			BaseMaterial3D.NormalEnabled = true;
			BaseMaterial3D.NormalScale = asset.Scale.Value;
			BaseMaterial3D.NormalTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
		}

		public void RimLoad(RimMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.RimEnabled = false;
				return;
			}
			BaseMaterial3D.RimEnabled = true;
			BaseMaterial3D.Rim = asset.Rim.Value;
			BaseMaterial3D.RimTint = asset.Tint.Value;
			BaseMaterial3D.RimTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
		}

		public void AmbientOcclusionLoad(AmbientOcclusionMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.AOEnabled = false;
				return;
			}
			BaseMaterial3D.AOEnabled = true;
			BaseMaterial3D.AOLightAffect = asset.LightAffect;
			BaseMaterial3D.AOTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
			BaseMaterial3D.AOOnUV2 = asset.UV2;
			BaseMaterial3D.AOTextureChannel = asset.Channel.Value switch {
				TextureChannel.Red => BaseMaterial3D.TextureChannel.Red,
				TextureChannel.Green => BaseMaterial3D.TextureChannel.Green,
				TextureChannel.Blue => BaseMaterial3D.TextureChannel.Blue,
				TextureChannel.Alpha => BaseMaterial3D.TextureChannel.Alpha,
				_ => BaseMaterial3D.TextureChannel.Grayscale,
			};
		}

		public void AnisotropyLoad(AnisotropyMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.AnisotropyEnabled = false;
				return;
			}
			BaseMaterial3D.AnisotropyEnabled = true;
			BaseMaterial3D.Anisotropy = asset.Anisotropy;
			BaseMaterial3D.AnisotropyFlowmap = asset.Flowmap.Asset is null ? null : ((GodotTexture2D)asset.Flowmap.Asset.Texture2D).Texture2D;
		}

		public void BackLightLoad(BackLightMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.BacklightEnabled = false;
				return;
			}
			BaseMaterial3D.BacklightEnabled = true;
			BaseMaterial3D.Backlight = new Color(asset.Color.Value.r, asset.Color.Value.g, asset.Color.Value.b, asset.Color.Value.a);
			BaseMaterial3D.BacklightTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
		}

		public void BillboardLoad(BillboardMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.BillboardMode = BaseMaterial3D.BillboardModeEnum.Disabled;
				return;
			}
			BaseMaterial3D.BillboardMode = asset.Mode.Value switch {
				BillboardMaterialFeatere.BillBoardMode.Enabled => BaseMaterial3D.BillboardModeEnum.Enabled,
				BillboardMaterialFeatere.BillBoardMode.YBillboard => BaseMaterial3D.BillboardModeEnum.FixedY,
				BillboardMaterialFeatere.BillBoardMode.ParticleBillboard => BaseMaterial3D.BillboardModeEnum.Particles,
				_ => BaseMaterial3D.BillboardModeEnum.Disabled,
			};
			BaseMaterial3D.BillboardKeepScale = asset.KeepScale.Value;
			BaseMaterial3D.ParticlesAnimHFrames = asset.HFrames.Value;
			BaseMaterial3D.ParticlesAnimVFrames = asset.VFrames.Value;
			BaseMaterial3D.ParticlesAnimLoop = asset.Loop.Value;
		}

		public void ClearcoatLoad(ClearcoatMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.ClearcoatEnabled = false;
				return;
			}
			BaseMaterial3D.ClearcoatEnabled = true;
			BaseMaterial3D.Clearcoat = asset.Clearcoat.Value;
			BaseMaterial3D.Roughness = asset.Roughness.Value;
			BaseMaterial3D.ClearcoatTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
		}

		public void DetailLoad(DetailMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.DetailEnabled = false;
				return;
			}
			BaseMaterial3D.DetailEnabled = true;
			BaseMaterial3D.DetailAlbedo = asset.Albedo.Asset is null ? null : ((GodotTexture2D)asset.Albedo.Asset.Texture2D).Texture2D;
			BaseMaterial3D.DetailNormal = asset.Normal.Asset is null ? null : ((GodotTexture2D)asset.Normal.Asset.Texture2D).Texture2D;
			BaseMaterial3D.DetailMask = asset.Mask.Asset is null ? null : ((GodotTexture2D)asset.Mask.Asset.Texture2D).Texture2D;
			BaseMaterial3D.DetailUVLayer = asset.Layer.Value switch {
				UVLayer.UV2 => BaseMaterial3D.DetailUV.UV2,
				_ => BaseMaterial3D.DetailUV.UV1,
			};
			BaseMaterial3D.BlendMode = asset.BlendMode.Value switch {
				RhuEngine.Components.BlendMode.Add => BaseMaterial3D.BlendModeEnum.Add,
				RhuEngine.Components.BlendMode.Subtract => BaseMaterial3D.BlendModeEnum.Sub,
				RhuEngine.Components.BlendMode.Multiply => BaseMaterial3D.BlendModeEnum.Mul,
				_ => BaseMaterial3D.BlendModeEnum.Mix,
			};
		}

		public void DistanceFadeLoad(DistanceFadeMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.DistanceFadeMode = BaseMaterial3D.DistanceFadeModeEnum.Disabled;
				return;
			}
			BaseMaterial3D.DistanceFadeMode = asset.Mode.Value switch {
				DistanceFadeMaterialFeatere.DistanceFadeMode.PixelAlpha => BaseMaterial3D.DistanceFadeModeEnum.PixelAlpha,
				DistanceFadeMaterialFeatere.DistanceFadeMode.PixelDither => BaseMaterial3D.DistanceFadeModeEnum.PixelDither,
				DistanceFadeMaterialFeatere.DistanceFadeMode.ObjectDither => BaseMaterial3D.DistanceFadeModeEnum.ObjectDither,
				_ => BaseMaterial3D.DistanceFadeModeEnum.Disabled,
			};
			BaseMaterial3D.DistanceFadeMaxDistance = asset.MaxDistance;
			BaseMaterial3D.DistanceFadeMinDistance = asset.MinDistance;
		}

		public void GrowLoad(GrowMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.Grow = false;
				return;
			}
			BaseMaterial3D.Grow = true;
			BaseMaterial3D.GrowAmount = asset.Amount;
		}

		public void HeightLoad(HeightMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.HeightmapEnabled = false;
				return;
			}
			BaseMaterial3D.HeightmapEnabled = true;
			BaseMaterial3D.HeightmapScale = asset.Scale;
			BaseMaterial3D.HeightmapDeepParallax = asset.DeepParallax;
			BaseMaterial3D.HeightmapMaxLayers = asset.DeepParallaxMaxLayer;
			BaseMaterial3D.HeightmapMinLayers = asset.DeepParallaxMinLayer;
			BaseMaterial3D.HeightmapFlipTangent = asset.FlipTangent;
			BaseMaterial3D.HeightmapFlipBinormal = asset.FlipBinormal;
			BaseMaterial3D.HeightmapFlipTexture = asset.FlipTexture;
			BaseMaterial3D.HeightmapTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
		}

		public void ProximityFadeLoad(ProximityFadeMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.ProximityFadeEnabled = false;
				return;
			}
			BaseMaterial3D.ProximityFadeEnabled = true;
			BaseMaterial3D.ProximityFadeDistance = asset.Distance;
		}

		public void RefractionLoad(RefractionMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.RefractionEnabled = false;
				return;
			}
			BaseMaterial3D.RefractionEnabled = true;
			BaseMaterial3D.RefractionScale = asset.Scale;
			BaseMaterial3D.RefractionTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
			BaseMaterial3D.RefractionTextureChannel = asset.Channel.Value switch {
				TextureChannel.Red => BaseMaterial3D.TextureChannel.Red,
				TextureChannel.Green => BaseMaterial3D.TextureChannel.Green,
				TextureChannel.Blue => BaseMaterial3D.TextureChannel.Blue,
				TextureChannel.Alpha => BaseMaterial3D.TextureChannel.Alpha,
				_ => BaseMaterial3D.TextureChannel.Grayscale,
			};
		}

		public void SubsurfaceScatteringLoad(SubsurfaceScatteringMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.SubsurfScatterEnabled = false;
				return;
			}
			BaseMaterial3D.SubsurfScatterEnabled = true;
			BaseMaterial3D.SubsurfScatterStrength = asset.Strength;
			BaseMaterial3D.SubsurfScatterSkinMode = asset.SkinMode;
			BaseMaterial3D.SubsurfScatterTexture = asset.Texture.Asset is null ? null : ((GodotTexture2D)asset.Texture.Asset.Texture2D).Texture2D;
			BaseMaterial3D.SubsurfScatterTransmittanceEnabled = asset.TransmittanceEnabled;
			BaseMaterial3D.SubsurfScatterTransmittanceColor = new Color(asset.TransmittanceColor.Value.r, asset.TransmittanceColor.Value.g, asset.TransmittanceColor.Value.b, asset.TransmittanceColor.Value.a);
			BaseMaterial3D.SubsurfScatterTransmittanceTexture = asset.TransmittanceTexture.Asset is null ? null : ((GodotTexture2D)asset.TransmittanceTexture.Asset.Texture2D).Texture2D;
			BaseMaterial3D.SubsurfScatterTransmittanceDepth = asset.TransmittanceDepth;
			BaseMaterial3D.SubsurfScatterTransmittanceBoost = asset.TransmittanceBoost;
		}

		public void TransformLoad(TransformMaterialFeatere asset) {
			if (asset is null) {
				BaseMaterial3D.FixedSize = false;
				BaseMaterial3D.UseParticleTrails = false;
				BaseMaterial3D.UsePointSize = false;
				BaseMaterial3D.PointSize = 1f;
				return;
			}
			BaseMaterial3D.FixedSize = asset.FixedSize.Value;
			BaseMaterial3D.UseParticleTrails = asset.UseParticleTrails.Value;
			BaseMaterial3D.UsePointSize = asset.UsePointSize.Value;
			BaseMaterial3D.PointSize = asset.PointSize.Value;
		}
	}

	public sealed class GodotStandardMaterial : GodotBaseMaterial<StandardMaterial3D>, IStandardMaterial
	{
		public StandardMaterial3D StandardMaterial3D => Material as StandardMaterial3D;

		public float Metallic { set => StandardMaterial3D.Metallic = value; }
		public float Specular { set => StandardMaterial3D.MetallicSpecular = value; }
		public RTexture2D MetallicTexture { set => StandardMaterial3D.MetallicTexture = value is null ? null : ((GodotTexture2D)value.Texture2D).Texture2D; }
		public TextureChannel MetallicChannel
		{
			set {
				StandardMaterial3D.MetallicTextureChannel = value switch {
					TextureChannel.Red => BaseMaterial3D.TextureChannel.Red,
					TextureChannel.Green => BaseMaterial3D.TextureChannel.Green,
					TextureChannel.Blue => BaseMaterial3D.TextureChannel.Blue,
					TextureChannel.Alpha => BaseMaterial3D.TextureChannel.Alpha,
					_ => BaseMaterial3D.TextureChannel.Grayscale,
				};
			}
		}
		public float Roughness { set => StandardMaterial3D.Roughness = value; }
		public RTexture2D RoughnessTexture { set => StandardMaterial3D.RoughnessTexture = value is null ? null : ((GodotTexture2D)value.Texture2D).Texture2D; }
		public TextureChannel RoughnessChannel
		{
			set {
				StandardMaterial3D.RoughnessTextureChannel = value switch {
					TextureChannel.Red => BaseMaterial3D.TextureChannel.Red,
					TextureChannel.Green => BaseMaterial3D.TextureChannel.Green,
					TextureChannel.Blue => BaseMaterial3D.TextureChannel.Blue,
					TextureChannel.Alpha => BaseMaterial3D.TextureChannel.Alpha,
					_ => BaseMaterial3D.TextureChannel.Grayscale,
				};
			}
		}
	}

	public sealed class GodotOrmMaterial : GodotBaseMaterial<OrmMaterial3D>, IORMMaterial
	{
		public OrmMaterial3D OrmMaterial3D => Material as OrmMaterial3D;

		public RTexture2D ORMTexture { set => OrmMaterial3D.OrmTexture = (value is null ? null : ((GodotTexture2D)value.Texture2D).Texture2D); }
	}


	public sealed class GodotUnlit : GodotMaterial, IUnlitMaterial
	{
		public GodotUnlit() : base(new StandardMaterial3D {
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			CullMode = BaseMaterial3D.CullModeEnum.Front,
		}) {
		}

		public bool DullSided
		{
			set {
				if (Material is StandardMaterial3D material3D) {
					material3D.CullMode = value ? BaseMaterial3D.CullModeEnum.Disabled : BaseMaterial3D.CullModeEnum.Back;
				}
			}
		}
		public Transparency Transparency
		{
			set {
				if (Material is StandardMaterial3D material3D) {
					switch (value) {
						case Transparency.Blend:
							material3D.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
							material3D.BlendMode = BaseMaterial3D.BlendModeEnum.Mix;
							break;
						case Transparency.Add:
							material3D.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;
							material3D.BlendMode = BaseMaterial3D.BlendModeEnum.Add;
							break;
						default:
							material3D.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;
							material3D.BlendMode = BaseMaterial3D.BlendModeEnum.Mix;
							break;
					}
				}
			}
		}
		public RTexture2D Texture
		{
			set {
				var texture = ((GodotTexture2D)value?.Inst)?.Texture2D;
				if (Material is StandardMaterial3D material3D) {
					material3D.AlbedoTexture = texture;
				}
			}
		}
		public Colorf Tint
		{
			set {
				if (Material is StandardMaterial3D material3D) {
					material3D.AlbedoColor = new Color(value.r, value.g, value.b, value.a);
				}
			}
		}
		public bool NoDepthTest
		{
			set {
				if (Material is StandardMaterial3D material3D) {
					material3D.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
					material3D.NoDepthTest = true;
					material3D.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled;
				}
			}
		}
	}

}
