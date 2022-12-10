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
			return target is StandardMaterial3D standard ? standard.AlbedoColor : new Color();
		}

		public static void SetColor(this Material target, Color color) {
			if (target is StandardMaterial3D standard) {
				standard.AlbedoColor = color;
			}
		}
	}


	public sealed class GodotMaterial
	{
		public Material Material;

		public System.Collections.Generic.Dictionary<Color, Material> Others = new();
		public void UpdateColor(Color newColor) {
			Material.SetColor(newColor);
			foreach (var item in Others) {
				item.Value.SetColor(item.Key * newColor);
			}
		}
		public void UpdateData(Action<Material> data) {
			data(Material);
			foreach (var item in Others) {
				data(item.Value);
			}
		}

		public Material GetMatarial(Colorf tint) {
			if (tint == Colorf.White) {
				return Material;
			}
			var target = new Color(tint.r, tint.g, tint.b, tint.a);
			if (Others.ContainsKey(target)) {
				return Others[target];
			}
			var newMat = (Material)Material.Duplicate();
			newMat.SetColor(target * Material.GetColor());
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

	public sealed class GoMat : IRMaterial
	{
		public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex) {
			yield break;
		}

		public int GetRenderPriority(object ex) {
			return ((GodotMaterial)ex).Material.RenderPriority;
		}

		public object Make(RShader rShader) {
			return new GodotMaterial();
		}

		public void Pram(object ex, string tex, object value) {

		}

		public void SetRenderPriority(object ex, int renderPri) {
			((GodotMaterial)ex).UpdateData((mit) => mit.RenderPriority = renderPri);
		}
	}

	public sealed class GodotStaticMats : IStaticMaterialManager
	{
		public IUnlitMaterial CreateUnlitMaterial() {
			return new GodotUnlit();
		}
	}



	public sealed class GodotUnlit : StaticMaterialBase<GodotMaterial>, IUnlitMaterial
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
						material3D.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
						material3D.NoDepthTest = true;
						material3D.DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled;
					}
				});
			}
		}
	}

}
