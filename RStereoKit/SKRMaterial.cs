using System;
using System.Collections.Generic;
using System.Text;
using StereoKit;
using RhuEngine.Linker;
using RNumerics;
using System.Runtime.InteropServices;
using System.Numerics;

namespace RStereoKit
{
	public class SKShader : IRShader
	{
		public RShader UnlitClip => new RShader(Shader.UnlitClip);

		public RShader PBRClip => new RShader(Shader.PBRClip);

		public RShader PBR => new RShader(Shader.PBR);

		public RShader Unlit => new RShader(Shader.Unlit);
	}

	public class SKMitStactic : IRMitConsts
	{
		public string FaceCull => "FaceCall";

		public string Transparency => "Transparency";

		public string MainTexture => "diffuse";

		public string WireFrame => "WireFrame";

		public string MainColor => "color";
	}

	public class SKRMaterial : IRMaterial
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

		public object Make(RShader rShader) {
			return new Material((Shader)rShader.e);
		}

		public void Pram(object ex, string tex, object value) {
			if(tex == "WIREFRAME_RHUBARB_CUSTOM") {
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
				if(texer is null) {
					((Material)ex)[tex] = value;
				}
				if(texer.Tex is null) {
					return;
				}
				((Material)ex)[tex] = (Tex)texer.Tex;
				return;
			}

			((Material)ex)[tex] = value;
		}
	}
}
