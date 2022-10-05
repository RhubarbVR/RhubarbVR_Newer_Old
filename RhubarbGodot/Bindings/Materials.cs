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
using RhubarbVR.Bindings.TextureBindings;

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
		public ITextMaterial CreateTextMaterial() {
			return new GodotText();
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
				var texture = ((GodotTexture2D)value?.Inst)?.Texture2D;
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
						material3D.NoDepthTest = true;
						material3D.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled;
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
				var texture = ((GodotTexture2D)value?.Inst)?.Texture2D;
				YourData?.UpdateData((data) => {
					if (data is StandardMaterial3D material3D) {
						material3D.AlbedoTexture = texture;
					}
				});
			}
		}
	}

}
