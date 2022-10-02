﻿using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine.Linker;
using RNumerics;
using System.Runtime.InteropServices;
using System.Numerics;
using RhuEngine;

namespace RStereoKit
{
	public class StaticMitsManager : IStaticMaterialManager
	{
		public sealed class TextMaterial : StaticMaterialBase<Material>, ITextMaterial
		{
			public TextMaterial() {
				UpdateMaterial(StereoKit.Material.Unlit.Copy());
				YourData.Transparency = StereoKit.Transparency.Blend;
				YourData.DepthWrite = false;
				YourData.FaceCull = StereoKit.Cull.None;
				YourData.QueueOffset = 4990;
			}

			public RTexture2D Texture
			{
				set {
					if (value is null) {
						YourData[MatParamName.DiffuseTex] = Tex.White;
					}
					if (value.Tex is null) {
						YourData[MatParamName.DiffuseTex] = Tex.DevTex;
						return;
					}
					YourData[MatParamName.DiffuseTex] = (Tex)value.Tex;
					return;
				}
			}
		}
		public sealed class PBRMaterial : StaticMaterialBase<Material>, IPBRMaterial
		{
			public PBRMaterial() {
				UpdateMaterial(StereoKit.Material.PBR.Copy());
			}
			public BasicRenderMode RenderMode
			{
				set {
					switch (value) {
						case BasicRenderMode.Opaque:
							UpdateMaterial(StereoKit.Material.PBR.Copy());
							YourData.Transparency = StereoKit.Transparency.None;
							break;
						case BasicRenderMode.CutOut:
							UpdateMaterial(StereoKit.Material.PBRClip.Copy());
							break;
						case BasicRenderMode.Transparent:
							UpdateMaterial(StereoKit.Material.PBR.Copy());
							YourData.Transparency = StereoKit.Transparency.Blend;
							break;
						default:
							break;
					}
				}
			}

			public RTexture2D AlbedoTexture
			{
				set {
					if (value is null) {
						YourData[MatParamName.DiffuseTex] = Tex.White;
						return;
					}
					if (value.Tex is null) {
						YourData[MatParamName.DiffuseTex] = Tex.DevTex;
						return;
					}
					YourData[MatParamName.DiffuseTex] = (Tex)value.Tex;
				}
			}

			public Colorf AlbedoTint { set => YourData[MatParamName.ColorTint] = new Color(value.r, value.g, value.b, value.a); }
			public float AlphaCutOut { set => YourData[MatParamName.ClipCutoff] = value; }
			public RTexture2D MetallicTexture
			{
				set {
					if (value is null) {
						YourData[MatParamName.MetalTex] = Tex.White;
						return;
					}
					if (value.Tex is null) {
						YourData[MatParamName.MetalTex] = Tex.DevTex;
						return;
					}
					YourData[MatParamName.MetalTex] = (Tex)value.Tex;
				}
			}
			public float Metallic { set => YourData[MatParamName.MetallicAmount] = value; }
			public float Smoothness { get; set; }
			public bool SmoothnessFromAlbedo { get; set; }
			public RTexture2D NormalMap { get; set; }
			public RTexture2D HeightMap { get; set; }
			public RTexture2D Occlusion
			{
				set {
					if (value is null) {
						YourData[MatParamName.OcclusionTex] = Tex.White;
						return;
					}
					if (value.Tex is null) {
						YourData[MatParamName.OcclusionTex] = Tex.DevTex;
						return;
					}
					YourData[MatParamName.OcclusionTex] = (Tex)value.Tex;
				}
			}
			public RTexture2D DetailMask { get; set; }
			public RTexture2D EmissionTexture
			{
				set {
					if (value is null) {
						YourData[MatParamName.EmissionTex] = Tex.White;
						return;
					}
					if (value.Tex is null) {
						YourData[MatParamName.EmissionTex] = Tex.DevTex;
						return;
					}
					YourData[MatParamName.EmissionTex] = (Tex)value.Tex;
				}
			}
			public Colorf EmissionTint { set => YourData[MatParamName.EmissionFactor] = new Color(value.r, value.g, value.b, value.a); }
			public Vector2f AlbedoTextureTilling { get; set; }
			public Vector2f AlbedoTextureOffset { get; set; }
			public Vector2f MetallicTextureTilling { get; set; }
			public Vector2f MetallicTextureOffset { get; set; }
			public Vector2f NormalMapTilling { get; set; }
			public Vector2f NormalMapOffset { get; set; }
			public Vector2f HeightMapTilling { get; set; }
			public Vector2f HeightMapOffset { get; set; }
			public Vector2f OcclusionTilling { get; set; }
			public Vector2f OcclusionOffset { get; set; }
			public Vector2f DetailMaskTilling { get; set; }
			public Vector2f DetailMaskOffset { get; set; }
			public RTexture2D DetailAlbedo { get; set; }
			public Vector2f DetailAlbedoTilling { get; set; }
			public Vector2f DetailAlbedoOffset { get; set; }
			public RTexture2D DetailNormal { get; set; }
			public Vector2f DetailNormalTilling { get; set; }
			public Vector2f DetailNormalOffset { get; set; }
			public float DetailNormalMapScale { get; set; }
			public Vector2f EmissionTextureTilling { get; set; }
			public Vector2f EmissionTextureOffset { get; set; }
		}

		public class ToonMaterial : StaticMaterialBase<Material>, IToonMaterial
		{
			public ToonMaterial() {
				UpdateMaterial(StereoKit.Material.Unlit.Copy());
			}

			public BasicRenderMode RenderMode  { get; set; }
			public RhuEngine.Linker.Cull CullMode  { get; set; }
			public float AlphaCutOut  { get; set; }
			public RTexture2D LitColorTexture  { get; set; }
			public Colorf LitColorTint  { get; set; }
			public RTexture2D ShadeColorTexture  { get; set; }
			public Colorf ShadeColorTint  { get; set; }
			public float ShadingToony  { get; set; }
			public RTexture2D NormalMap  { get; set; }
			public float Normal  { get; set; }
			public float ShadingShift  { get; set; }
			public RTexture2D ShadowReceiveMultiplierTexture  { get; set; }
			public float ShadowReceiveMultiplier  { get; set; }
			public RTexture2D LitShadeMixingMultiplierTexture  { get; set; }
			public float LitShadeMixingMultiplier  { get; set; }
			public float LightColorAttenuation  { get; set; }
			public float GLIntensity  { get; set; }
			public RTexture2D EmissionColorTexture  { get; set; }
			public Colorf EmissionColorTint  { get; set; }
			public RTexture2D MatCap  { get; set; }
			public RTexture2D RimColorTexture  { get; set; }
			public Colorf RimColorTint  { get; set; }
			public float LightingMix  { get; set; }
			public float FresnelPower  { get; set; }
			public float Lift  { get; set; }
			public IToonMaterial.OutLineType OutLineMode  { get; set; }
			public RTexture2D OutLineWidthTexture  { get; set; }
			public float OutLineWidth  { get; set; }
			public float WidthScaledMaxDistance  { get; set; }
			public bool FixedColor  { get; set; }
			public Colorf OutLineColor  { get; set; }
			public float OutLineLightingMix  { get; set; }
			public Vector2f Tilling  { get; set; }
			public Vector2f Offset  { get; set; }
			public RTexture2D AnimationMask  { get; set; }
			public Vector2f ScrollAnimation  { get; set; }
			public float RotationAnimation  { get; set; }
			public Vector2f LitColorTextureTilling  { get; set; }
			public Vector2f LitColorTextureOffset  { get; set; }
			public Vector2f ShadeColorTextureTilling  { get; set; }
			public Vector2f ShadeColorTextureOffset  { get; set; }
			public Vector2f NormalMapTilling  { get; set; }
			public Vector2f NormalMapOffset  { get; set; }
			public Vector2f ShadowReceiveMultiplierTextureTilling  { get; set; }
			public Vector2f ShadowReceiveMultiplierTextureOffset  { get; set; }
			public Vector2f LitShadeMixingMultiplierTextureTilling  { get; set; }
			public Vector2f LitShadeMixingMultiplierTextureOffset  { get; set; }
			public Vector2f EmissionColorTextureTilling  { get; set; }
			public Vector2f EmissionColorTextureOffset  { get; set; }
			public Vector2f MatCapTilling  { get; set; }
			public Vector2f MatCapOffset  { get; set; }
			public Vector2f RimColorTextureTilling  { get; set; }
			public Vector2f RimColorTextureOffset  { get; set; }
			public Vector2f OutLineWidthTextureTilling  { get; set; }
			public Vector2f OutLineWidthTextureOffset  { get; set; }
			public Vector2f AnimationMaskTilling  { get; set; }
			public Vector2f AnimationMaskOffset { get; set; }
		}

		public class UnlitMaterial : StaticMaterialBase<Material>, IUnlitMaterial
		{
			public UnlitMaterial() {
				UpdateMaterial(StereoKit.Material.Unlit.Copy());
			}

			public RTexture2D Texture
			{
				set {
					if (value is null) {
						YourData[MatParamName.DiffuseTex] = Tex.White;
						return;
					}
					if (value.Tex is null) {
						YourData[MatParamName.DiffuseTex] = Tex.DevTex;
						return;
					}
					YourData[MatParamName.DiffuseTex] = (Tex)value.Tex;
				}
			}

			public RhuEngine.Linker.Transparency Transparency { set => YourData.Transparency = (StereoKit.Transparency)(int)value; }
			public Colorf Tint { set => YourData[MatParamName.ColorTint] = new Color(value.r, value.g, value.b, value.a); }
			public bool DullSided { set => YourData.FaceCull = (value) ? StereoKit.Cull.None : StereoKit.Cull.Back; }
			public bool NoDepthTest { get; set; }
		}
		public IPBRMaterial CreatePBRMaterial() {
			return new PBRMaterial();
		}
		public IToonMaterial CreateToonMaterial() {
			return new ToonMaterial();
		}

		public ITextMaterial CreateTextMaterial() {
			return new TextMaterial();
		}

		public IUnlitMaterial CreateUnlitMaterial() {
			return new UnlitMaterial();
		}
	}

	public sealed class SKShader : IRShader
	{
	}

	public sealed class SKMitStactic : IRMitConsts
	{
		public string FaceCull => "FaceCall";

		public string Transparency => "Transparency";

		public string MainTexture => "diffuse";

		public string WireFrame => "WireFrame";

		public string MainColor => "color";
	}

	public sealed class SKRMaterial : IRMaterial
	{
		public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex) {
			yield return new RMaterial.RMatParamInfo { name = "FACECULL_RHUBARB_CUSTOM", type = RhuEngine.Linker.MaterialParam.Cull };
			yield return new RMaterial.RMatParamInfo { name = "TRANSPARENCY_RHUBARB_CUSTOM", type = RhuEngine.Linker.MaterialParam.Transparency };
			yield return new RMaterial.RMatParamInfo { name = "WIREFRAME_RHUBARB_CUSTOM", type = RhuEngine.Linker.MaterialParam.Bool };
			foreach (var item in ((Material)tex).GetAllParamInfo()) {
				var main = new RMaterial.RMatParamInfo { name = RMaterial.RenameFromEngine(item.name), type = (RhuEngine.Linker.MaterialParam)item.type };
				if (main.name != RMaterial.MainColor) {
					yield return main;
				}
			}
		}

		public int GetRenderQueueOffset(object mit) {
			return ((Material)mit).QueueOffset;
		}

		public object Make(RShader rShader) {
			return rShader == null ? null : (object)new Material((Shader)rShader.e);
		}

		public void Pram(object ex, string tex, object value) {
			if (tex == "WIREFRAME_RHUBARB_CUSTOM") {
				((Material)ex).Wireframe = (bool)value;
				return;
			}
			if (tex == "FACECULL_RHUBARB_CUSTOM") {
				((Material)ex).FaceCull = (StereoKit.Cull)(int)value;
				return;
			}
			if (tex == "TRANSPARENCY_RHUBARB_CUSTOM") {
				((Material)ex).Transparency = (StereoKit.Transparency)(int)value;
				return;
			}
			tex = RMaterial.RenameFromRhubarb(tex);
			try {
				if (value is Colorb value1) {
					try {
						var colorGamma = new Color(value1.r, value1.g, value1.b, value1.a);
						((Material)ex)[tex] = colorGamma;
					}
					catch {

					}
					return;
				}
			}
			catch { }
			try {
				if (value is Colorf value2) {
					try {
						var colorGamma = new Color(value2.r, value2.g, value2.b, value2.a);
						((Material)ex)[tex] = colorGamma;
					}
					catch {
						((Material)ex)[tex] = (Vec4)(Vector4)value2.ToRGBA();
					}
					return;
				}
			}
			catch { }
			try {
				if (value is ColorHSV color) {
					try {
						var temp = color.ConvertToRGB();
						var colorGamma = new Color(temp.r, temp.g, temp.b, temp.a);
						((Material)ex)[tex] = colorGamma;
					}
					catch {
						((Material)ex)[tex] = (Vec4)(Vector4)color.ConvertToRGB().ToRGBA();
					}
					return;
				}
			}
			catch { }

			if (value is Vec4) {
				((Material)ex)[tex] = (Vec4)(Vector4)(Vector4f)value;
				return;
			}

			if (value is Vec3) {
				((Material)ex)[tex] = (Vec3)(Vector3)(Vector3f)value;
				return;
			}

			if (value is Vec2) {
				((Material)ex)[tex] = (Vec2)(Vector2)(Vector2f)value;
				return;
			}

			if (value is RNumerics.Matrix me) {
				((Material)ex)[tex] = new StereoKit.Matrix(me.m);
				return;
			}

			if (value is RTexture2D texer) {
				if (texer is null) {
					((Material)ex)[tex] = value;
				}
				if (texer.Tex is null) {
					((Material)ex)[tex] = null;
					return;
				}
				((Material)ex)[tex] = (Tex)texer.Tex;
				return;
			}

			((Material)ex)[tex] = value;
		}

		public void SetRenderQueueOffset(object mit, int tex) {
			((Material)mit).QueueOffset = tex;
		}
	}
}
