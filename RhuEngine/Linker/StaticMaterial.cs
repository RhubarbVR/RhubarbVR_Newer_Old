using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface ITextMaterial : IStaticMaterial
	{
		public RTexture2D Texture { set; }
	}

	public enum BasicRenderMode
	{
		Opaque,
		CutOut,
		Transparent
	}

	public interface IPBRMaterial : IStaticMaterial
	{
		public BasicRenderMode RenderMode { set; }
		public RTexture2D AlbedoTexture { set; }
		public Vector2f AlbedoTextureTilling { set; }
		public Vector2f AlbedoTextureOffset { set; }

		public Colorf AlbedoTint { set; }
		public float AlphaCutOut { set; }
		public RTexture2D MetallicTexture { set; }
		public Vector2f MetallicTextureTilling { set; }
		public Vector2f MetallicTextureOffset { set; }
		public float Metallic { set; }
		public float Smoothness { set; }
		public bool SmoothnessFromAlbedo { set; }
		public RTexture2D NormalMap { set; }
		public Vector2f NormalMapTilling { set; }
		public Vector2f NormalMapOffset { set; }
		public RTexture2D HeightMap { set; }
		public Vector2f HeightMapTilling { set; }
		public Vector2f HeightMapOffset { set; }
		public RTexture2D Occlusion { set; }
		public Vector2f OcclusionTilling { set; }
		public Vector2f OcclusionOffset { set; }
		public RTexture2D DetailMask { set; }
		public Vector2f DetailMaskTilling { set; }
		public Vector2f DetailMaskOffset { set; }

		public RTexture2D DetailAlbedo { set; }
		public Vector2f DetailAlbedoTilling { set; }
		public Vector2f DetailAlbedoOffset { set; }
		public RTexture2D DetailNormal { set; }
		public Vector2f DetailNormalTilling { set; }
		public Vector2f DetailNormalOffset { set; }
		public float DetailNormalMapScale { set; }

		public RTexture2D EmissionTexture { set; }
		public Vector2f EmissionTextureTilling { set; }
		public Vector2f EmissionTextureOffset { set; }
		public Colorf EmissionTint { set; }
	}



	public interface IToonMaterial : IStaticMaterial
	{
		public BasicRenderMode RenderMode { set; }
		public Cull CullMode { set; }
		public float AlphaCutOut { set; }

		public RTexture2D LitColorTexture { set; }
		public Vector2f LitColorTextureTilling { set; }
		public Vector2f LitColorTextureOffset { set; }
		public Colorf LitColorTint { set; }
		public RTexture2D ShadeColorTexture { set; }
		public Vector2f ShadeColorTextureTilling { set; }
		public Vector2f ShadeColorTextureOffset { set; }
		public Colorf ShadeColorTint { set; }
		public float ShadingToony { set; }
		public RTexture2D NormalMap { set; }
		public Vector2f NormalMapTilling { set; }
		public Vector2f NormalMapOffset { set; }
		public float Normal { set; }
		public float ShadingShift { set; }
		public RTexture2D ShadowReceiveMultiplierTexture { set; }
		public Vector2f ShadowReceiveMultiplierTextureTilling { set; }
		public Vector2f ShadowReceiveMultiplierTextureOffset { set; }
		public float ShadowReceiveMultiplier { set; }
		public RTexture2D LitShadeMixingMultiplierTexture { set; }
		public Vector2f LitShadeMixingMultiplierTextureTilling { set; }
		public Vector2f LitShadeMixingMultiplierTextureOffset { set; }
		public float LitShadeMixingMultiplier { set; }
		public float LightColorAttenuation { set; }
		public float GLIntensity { set; }
		public RTexture2D EmissionColorTexture { set; }
		public Vector2f EmissionColorTextureTilling { set; }
		public Vector2f EmissionColorTextureOffset { set; }
		public Colorf EmissionColorTint { set; }
		public RTexture2D MatCap { set; }
		public Vector2f MatCapTilling { set; }
		public Vector2f MatCapOffset { set; }
		public RTexture2D RimColorTexture { set; }
		public Vector2f RimColorTextureTilling { set; }
		public Vector2f RimColorTextureOffset { set; }
		public Colorf RimColorTint { set; }
		public float LightingMix { set; }
		public float FresnelPower { set; }
		public float Lift { set; }
		public enum OutLineType
		{
			WorldCords,
			ScreenCords,
			Off,
		}
		public OutLineType OutLineMode { set; }
		public RTexture2D OutLineWidthTexture { set; }
		public Vector2f OutLineWidthTextureTilling { set; }
		public Vector2f OutLineWidthTextureOffset { set; }
		public float OutLineWidth { set; }
		public float WidthScaledMaxDistance { set; }
		public bool FixedColor { set; }
		public Colorf OutLineColor { set; }
		public float OutLineLightingMix { set; }
		public RTexture2D AnimationMask { set; }
		public Vector2f AnimationMaskTilling { set; }
		public Vector2f AnimationMaskOffset { set; }
		public Vector2f ScrollAnimation { set; }
		public float RotationAnimation { set; }
	}

	public interface IUnlitMaterial : IStaticMaterial
	{
		public bool NoDepthTest { set; }

		public bool DullSided { set; }

		public Transparency Transparency { set; }

		public RTexture2D Texture { set; }

		public Colorf Tint { set; }
	}
	public interface IStaticMaterialManager
	{
		public ITextMaterial CreateTextMaterial();
		public IUnlitMaterial CreateUnlitMaterial();
		public IPBRMaterial CreatePBRMaterial();
		public IToonMaterial CreateToonMaterial();
	}


	public static class StaticMaterialManager
	{
		public static IStaticMaterialManager Instanances;

		public static T GetMaterial<T>() where T : IStaticMaterial {
			if (typeof(T) == typeof(IUnlitMaterial)) {
				return (T)Instanances.CreateUnlitMaterial();
			}
			if (typeof(T) == typeof(ITextMaterial)) {
				return (T)Instanances.CreateTextMaterial();
			}
			if (typeof(T) == typeof(IPBRMaterial)) {
				return (T)Instanances.CreatePBRMaterial();
			}
			if (typeof(T) == typeof(IToonMaterial)) {
				return (T)Instanances.CreateToonMaterial();
			}
			return default;
		}
	}

	public interface IStaticMaterial : IDisposable
	{
		public RMaterial Material { get; }

	}


	public abstract class StaticMaterialBase<T> : IStaticMaterial
	{
		public RMaterial Material { get; } = new(null);

		public T YourData;

		public void UpdateMaterial(T rMaterial) {
			YourData = rMaterial;
			Material.Target = rMaterial;
		}

		public void Dispose() {
			Material.Dispose();
		}
	}
}
