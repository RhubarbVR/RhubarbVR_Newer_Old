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
	public class GodotMaterial
	{
		public Material Material;

		public System.Collections.Generic.Dictionary<(Colorf, int), Material> Others = new();


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
			if(tint == Colorf.White && zDepth == 0) {
				return Material;
			}
			return null;
		}

		public GodotMaterial( Material material) {
			Material = material;
		}
		public GodotMaterial() {
			Material = new StandardMaterial3D();
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
			UpdateMaterial(new GodotMaterial());
		}

		public bool DullSided { set; get; }
		public Transparency Transparency { set; get; }
		public RTexture2D Texture { set; get; }
		public Colorf Tint { set; get; }
	}

	public class GodotText : StaticMaterialBase<GodotMaterial>, ITextMaterial
	{
		public GodotText() {
			UpdateMaterial(new GodotMaterial());
		}

		public RTexture2D Texture { set; get; }
	}

	public class GodotPBR : StaticMaterialBase<GodotMaterial>, IPBRMaterial
	{
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
