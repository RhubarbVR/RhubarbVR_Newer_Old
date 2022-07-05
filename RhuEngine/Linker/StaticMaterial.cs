using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

namespace RhuEngine.Linker
{
	public interface ITextMaterial:IStaticMaterial {
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
		public Colorf AlbedoTint { set; }
		public float AlphaCutOut { set; }
		public RTexture2D MetallicTexture { set; }
		public float Metallic { set; }
		public float Smoothness { set; }
		public bool SmoothnessFromAlbedo { set; }
		public RTexture2D NormalMap { set; }
		public RTexture2D HeightMap { set; }
		public RTexture2D Occlusion { set; }
		public RTexture2D DetailMask { set; }
		public bool Emission { set; }
		public RTexture2D EmissionTexture { set; }
		public Colorf EmissionTint { set; }
		public Vector2f Tilling { set; }
		public Vector2f Offset { set; }
	}

	

	public interface IToonMaterial : IStaticMaterial
	{
		public BasicRenderMode RenderMode { set; }
		public Cull CullMode { set; }
		public float AlphaCutOut { set; }

		public RTexture2D LitColorTexture { set; }
		public Colorf LitColorTint { set; }
		public RTexture2D ShadeColorTexture { set; }
		public Colorf ShadeColorTint { set; }
		public float ShadingToony { set; }
		public RTexture2D NormalMap { set; }
		public float Normal { set; }
		public float ShadingShift { set; }
		public RTexture2D ShadowReceiveMultiplierTexture { set; }
		public float ShadowReceiveMultiplier { set; }
		public RTexture2D LitShadeMixingMultiplierTexture { set; }
		public float LitShadeMixingMultiplier { set; }
		public float LightColorAttenuation { set; }
		public float GLIntensity { set; }
		public RTexture2D EmissionColorTexture { set; }
		public Colorf EmissionColorTint { set; }
		public RTexture2D MatCap { set; }
		public RTexture2D RimColorTexture { set; }
		public Colorf RimColorTint { set; }
		public float LightingMix { set; }
		public float FresnelPower { set; }
		public float Lift { set; }
		public enum OutLineType { 
			WorldCords,
			ScreenCords,
			Off,
		}
		public OutLineType OutLineMode { set; }
		public RTexture2D OutLineWidthTexture { set; }
		public float OutLineWidth { set; }
		public float WidthScaledMaxDistance { set; }
		public bool FixedColor { set; }
		public Colorf OutLineColor { set; }
		public float OutLineLightingMix { set; }
		public Vector2f Tilling { set; }
		public Vector2f Offset { set; }
		public RTexture2D AnimationMask { set; }
		public Vector2f ScrollAnimation { set; }
		public float RotationAnimation { set; }
	}

	public interface IUnlitMaterial : IStaticMaterial
	{
		public Transparency Transparency { set; }

		public RTexture2D Texture { set; }

		public Colorf Tint { set; }
	}
	public interface IStaticMaterialManager {
		public ITextMaterial CreateTextMaterial();
		public IUnlitMaterial CreateUnlitMaterial();
		public IPBRMaterial CreatePBRMaterial();
		public IToonMaterial CreateToonMaterial();
	}


	public static class StaticMaterialManager
	{
		public static IStaticMaterialManager Instanances;

		public static T GetMaterial<T>() where T: IStaticMaterial {
			if(typeof(T) == typeof(IUnlitMaterial)) {
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

	public interface IStaticMaterial:IDisposable
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
