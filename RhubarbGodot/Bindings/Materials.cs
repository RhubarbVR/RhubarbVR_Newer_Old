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
using NAudio.Wave;

namespace RhubarbVR.Bindings
{
	public static class GodotMaterialHelper
	{
		public static Color GetColor(this Material target) {
			if (target is StandardMaterial3D standard) {
				return standard.AlbedoColor;
			}
			return new Color();
		}

		public static void SetColor(this Material target, Color color) {
			if (target is StandardMaterial3D standard) {
				standard.AlbedoColor = color;
			}
		}
	}


	public class GodotMaterial
	{
		public Material Material;

		public System.Collections.Generic.Dictionary<(Color, int), Material> Others = new();
		public void UpdateColor(Color newColor) {
			Material.SetColor(newColor);
			foreach (var item in Others) {
				item.Value.SetColor(item.Key.Item1 * newColor);
			}
		}
		public void UpdateData(Action<Material> data) {
			data(Material);
			foreach (var item in Others) {
				data(item.Value);
			}
		}

		private int _offset;
		public int RenderPriorityOffset
		{
			get => _offset;
			set {
				Material.RenderPriority = Material.RenderPriority - _offset + value;
				foreach (var item in Others) {
					item.Value.RenderPriority = Material.RenderPriority + item.Key.Item2;
				}
				_offset = value;
			}
		}

		public Material GetMatarial(Colorf tint, int zDepth) {
			if (tint == Colorf.White && zDepth == 0) {
				return Material;
			}
			var target = (new Color(tint.r, tint.g, tint.b, tint.a), zDepth);
			if (Others.ContainsKey(target)) {
				return Others[target];
			}
			var newMat = (Material)Material.Duplicate();
			newMat.RenderPriority += zDepth;
			newMat.SetColor(target.Item1 * Material.GetColor());
			Others.Add(target, newMat);
			return newMat;
		}

		public GodotMaterial(Material material) {
			Material = material;
		}
		public GodotMaterial() {
			var newMat = new StandardMaterial3D {
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				CullMode = BaseMaterial3D.CullModeEnum.Disabled
			};
			Material = newMat;
		}
	}

	public class GoMat : IRMaterial
	{
		public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex) {
			yield break;
		}

		public object Make(RShader rShader) {
			return new GodotMaterial();
		}

		public void Pram(object ex, string tex, object value) {

		}

		public void SetRenderQueueOffset(object mit, int tex) {
			if(mit is GodotMaterial material) {
				material.RenderPriorityOffset = tex;
			}
		}
	}

	public class GodotStaticMats : IStaticMaterialManager
	{
		public IPBRMaterial CreatePBRMaterial() {
			return new GodotPBR();
		}

		public ITextMaterial CreateTextMaterial() {
			return new GodotText();
		}

		public IToonMaterial CreateToonMaterial() {
			return new GodotToon();
		}

		public IUnlitMaterial CreateUnlitMaterial() {
			return new GodotUnlit();
		}
	}



	public class GodotUnlit : StaticMaterialBase<GodotMaterial>, IUnlitMaterial
	{
		public GodotUnlit() {
			var newUnlit = new StandardMaterial3D {
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				CullMode = BaseMaterial3D.CullModeEnum.Front,
			};
			UpdateMaterial(new GodotMaterial(newUnlit));
		}

		public bool DullSided
		{
			set {
				YourData?.UpdateData((data) => {
					if(data is StandardMaterial3D material3D) {
						material3D.CullMode = value ? BaseMaterial3D.CullModeEnum.Disabled : BaseMaterial3D.CullModeEnum.Front;
					}
				});
			}
		}
		public Transparency Transparency
		{
			set {
				YourData?.UpdateData((data) => {
					if (data is StandardMaterial3D material3D) {
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
				});
			}
		}
		public RTexture2D Texture
		{
			set {
				var texture = (Texture2D)value?.Tex;
				YourData?.UpdateData((data) => {
					if (data is StandardMaterial3D material3D) {
						material3D.AlbedoTexture = texture;
					}
				});
			}
		}
		public Colorf Tint
		{
			set => YourData?.UpdateColor(new Color(value.r, value.g, value.b, value.a));
		}
		public bool NoDepthTest
		{
			set {
				YourData?.UpdateData((data) => {
					if (data is StandardMaterial3D material3D) {
						material3D.NoDepthTest = value;
					}
				});
			}
		}
	}

	public class GodotText : StaticMaterialBase<GodotMaterial>, ITextMaterial
	{
		public GodotText() {
			var newUnlit = new StandardMaterial3D {
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				BlendMode = BaseMaterial3D.BlendModeEnum.Mix,
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				CullMode = BaseMaterial3D.CullModeEnum.Disabled,
			};
			UpdateMaterial(new GodotMaterial(newUnlit));
		}

		public RTexture2D Texture
		{
			set {
				var texture = (Texture2D)value?.Tex;
				YourData?.UpdateData((data) => {
					if (data is StandardMaterial3D material3D) {
						material3D.AlbedoTexture = texture;
					}
				});
			}
		}
	}

	public class GodotPBR : StaticMaterialBase<GodotMaterial>, IPBRMaterial
	{
		public GodotPBR() {
			var newPBR = new StandardMaterial3D {
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				CullMode = BaseMaterial3D.CullModeEnum.Back
			};
			UpdateMaterial(new GodotMaterial(newPBR));
		}

		public BasicRenderMode RenderMode { set; get; }
		public RTexture2D AlbedoTexture { set; get; }
		public Vector2f AlbedoTextureTilling { set; get; }
		public Vector2f AlbedoTextureOffset { set; get; }
		public Colorf AlbedoTint { set; get; }
		public float AlphaCutOut { set; get; }
		public RTexture2D MetallicTexture { set; get; }
		public Vector2f MetallicTextureTilling { set; get; }
		public Vector2f MetallicTextureOffset { set; get; }
		public float Metallic { set; get; }
		public float Smoothness { set; get; }
		public bool SmoothnessFromAlbedo { set; get; }
		public RTexture2D NormalMap { set; get; }
		public Vector2f NormalMapTilling { set; get; }
		public Vector2f NormalMapOffset { set; get; }
		public RTexture2D HeightMap { set; get; }
		public Vector2f HeightMapTilling { set; get; }
		public Vector2f HeightMapOffset { set; get; }
		public RTexture2D Occlusion { set; get; }
		public Vector2f OcclusionTilling { set; get; }
		public Vector2f OcclusionOffset { set; get; }
		public RTexture2D DetailMask { set; get; }
		public Vector2f DetailMaskTilling { set; get; }
		public Vector2f DetailMaskOffset { set; get; }
		public RTexture2D DetailAlbedo { set; get; }
		public Vector2f DetailAlbedoTilling { set; get; }
		public Vector2f DetailAlbedoOffset { set; get; }
		public RTexture2D DetailNormal { set; get; }
		public Vector2f DetailNormalTilling { set; get; }
		public Vector2f DetailNormalOffset { set; get; }
		public float DetailNormalMapScale { set; get; }
		public RTexture2D EmissionTexture { set; get; }
		public Vector2f EmissionTextureTilling { set; get; }
		public Vector2f EmissionTextureOffset { set; get; }
		public Colorf EmissionTint { set; get; }
	}

	public class GodotToon : StaticMaterialBase<GodotMaterial>, IToonMaterial
	{
		public GodotToon() {
			var newToon = new StandardMaterial3D {
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				CullMode = BaseMaterial3D.CullModeEnum.Back
			};
			UpdateMaterial(new GodotMaterial(newToon));
		}

		public BasicRenderMode RenderMode { set; get; }
		public Cull CullMode { set; get; }
		public float AlphaCutOut { set; get; }
		public RTexture2D LitColorTexture { set; get; }
		public Vector2f LitColorTextureTilling { set; get; }
		public Vector2f LitColorTextureOffset { set; get; }
		public Colorf LitColorTint { set; get; }
		public RTexture2D ShadeColorTexture { set; get; }
		public Vector2f ShadeColorTextureTilling { set; get; }
		public Vector2f ShadeColorTextureOffset { set; get; }
		public Colorf ShadeColorTint { set; get; }
		public float ShadingToony { set; get; }
		public RTexture2D NormalMap { set; get; }
		public Vector2f NormalMapTilling { set; get; }
		public Vector2f NormalMapOffset { set; get; }
		public float Normal { set; get; }
		public float ShadingShift { set; get; }
		public RTexture2D ShadowReceiveMultiplierTexture { set; get; }
		public Vector2f ShadowReceiveMultiplierTextureTilling { set; get; }
		public Vector2f ShadowReceiveMultiplierTextureOffset { set; get; }
		public float ShadowReceiveMultiplier { set; get; }
		public RTexture2D LitShadeMixingMultiplierTexture { set; get; }
		public Vector2f LitShadeMixingMultiplierTextureTilling { set; get; }
		public Vector2f LitShadeMixingMultiplierTextureOffset { set; get; }
		public float LitShadeMixingMultiplier { set; get; }
		public float LightColorAttenuation { set; get; }
		public float GLIntensity { set; get; }
		public RTexture2D EmissionColorTexture { set; get; }
		public Vector2f EmissionColorTextureTilling { set; get; }
		public Vector2f EmissionColorTextureOffset { set; get; }
		public Colorf EmissionColorTint { set; get; }
		public RTexture2D MatCap { set; get; }
		public Vector2f MatCapTilling { set; get; }
		public Vector2f MatCapOffset { set; get; }
		public RTexture2D RimColorTexture { set; get; }
		public Vector2f RimColorTextureTilling { set; get; }
		public Vector2f RimColorTextureOffset { set; get; }
		public Colorf RimColorTint { set; get; }
		public float LightingMix { set; get; }
		public float FresnelPower { set; get; }
		public float Lift { set; get; }
		public IToonMaterial.OutLineType OutLineMode { set; get; }
		public RTexture2D OutLineWidthTexture { set; get; }
		public Vector2f OutLineWidthTextureTilling { set; get; }
		public Vector2f OutLineWidthTextureOffset { set; get; }
		public float OutLineWidth { set; get; }
		public float WidthScaledMaxDistance { set; get; }
		public bool FixedColor { set; get; }
		public Colorf OutLineColor { set; get; }
		public float OutLineLightingMix { set; get; }
		public RTexture2D AnimationMask { set; get; }
		public Vector2f AnimationMaskTilling { set; get; }
		public Vector2f AnimationMaskOffset { set; get; }
		public Vector2f ScrollAnimation { set; get; }
		public float RotationAnimation { set; get; }
	}
}
