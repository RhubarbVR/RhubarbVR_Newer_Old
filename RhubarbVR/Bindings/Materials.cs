using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;

using RNumerics;
using GDExtension;
using System.Xml.Linq;
using Array = GDExtension.Array;
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
				ShadingModeValue = BaseMaterial3D.ShadingMode.Unshaded,
				CullModeValue = BaseMaterial3D.CullMode.Disabled
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
			return (int)((GodotMaterial)ex).Material.RenderPriority;
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
				ShadingModeValue = BaseMaterial3D.ShadingMode.Unshaded,
				CullModeValue = BaseMaterial3D.CullMode.Front,
			};
			UpdateMaterial(new GodotMaterial(newUnlit));
		}

		public bool DullSided
		{
			set {
				YourData?.UpdateData((data) => {
					if(data is StandardMaterial3D material3D) {
						material3D.CullModeValue = value ? BaseMaterial3D.CullMode.Disabled : BaseMaterial3D.CullMode.Front;
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
								material3D.TransparencyValue = BaseMaterial3D.Transparency.Alpha;
								material3D.BlendModeValue = BaseMaterial3D.BlendMode.Mix;
								break;
							case Transparency.Add:
								material3D.TransparencyValue = BaseMaterial3D.Transparency.Disabled;
								material3D.BlendModeValue = BaseMaterial3D.BlendMode.Add;
								break;
							default:
								material3D.TransparencyValue = BaseMaterial3D.Transparency.Disabled;
								material3D.BlendModeValue = BaseMaterial3D.BlendMode.Mix;
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
						material3D.TransparencyValue = BaseMaterial3D.Transparency.Alpha;
						material3D.NoDepthTest = true;
						material3D.DepthDrawModeValue = BaseMaterial3D.DepthDrawMode.Disabled;
					}
				});
			}
		}
	}

}
