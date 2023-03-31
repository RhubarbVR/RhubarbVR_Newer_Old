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

namespace RhubarbVR.Bindings
{

	public class GodotMaterial : IRMaterial
	{
		public Material Material { get; set; }

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
			Material?.Free();
			Material = null;
			GC.SuppressFinalize(this);
		}
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
					material3D.CullMode = value ? BaseMaterial3D.CullModeEnum.Disabled : BaseMaterial3D.CullModeEnum.Front;
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
