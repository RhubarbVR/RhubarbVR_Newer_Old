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

	public class SKRMaterial : IRMaterial
	{
		public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex) {
			foreach (var item in ((Material)tex).GetAllParamInfo()) {
				yield return new RMaterial.RMatParamInfo { name = item.name, type = (RhuEngine.Linker.MaterialParam)item.type };
			}
		}

		public RhuEngine.Linker.DepthTest GetDepthTest(object tex) {
			return (RhuEngine.Linker.DepthTest)((Material)tex).DepthTest;
		}

		public bool GetDepthWrite(object tex) {
			return ((Material)tex).DepthWrite;
		}

		public RhuEngine.Linker.Cull GetFaceCull(object tex) {
			return (RhuEngine.Linker.Cull)((Material)tex).FaceCull;
		}

		public int GetQueueOffset(object tex) {
			return ((Material)tex).QueueOffset;
		}

		public RhuEngine.Linker.Transparency GetTransparency(object tex) {
			return (RhuEngine.Linker.Transparency)((Material)tex).Transparency;
		}

		public bool GetWireframe(object tex) {
			return ((Material)tex).Wireframe;
		}

		public object Make(RShader rShader) {
			return new Material((Shader)rShader.e);
		}

		public void Pram(object ex, string tex, object value) {
			if (value is Colorb value1) {
				var colorGamma = new Color(value1.r, value1.g, value1.b, value1.a);
				((Material)ex)[tex] = colorGamma;
				return;
			}
			if (value is Colorf value2) {
				var colorGamma = new Color(value2.r, value2.g, value2.b, value2.a);
				((Material)ex)[tex] = colorGamma;
				return;
			}
			if (value is ColorHSV color) {
				var temp = color.ConvertToRGB();
				var colorGamma = new Color(temp.r, temp.g, temp.b, temp.a);
				((Material)ex)[tex] = colorGamma;
				return;
			}

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

		public void SetDepthTest(object tex, RhuEngine.Linker.DepthTest value) {
			((Material)tex).DepthTest = (StereoKit.DepthTest)value;
		}

		public void SetDepthWrite(object tex, bool value) {
			((Material)tex).DepthWrite = value;
		}

		public void SetFaceCull(object tex, RhuEngine.Linker.Cull value) {
			((Material)tex).FaceCull = (StereoKit.Cull)value;
		}

		public void SetQueueOffset(object tex, int value) {
			((Material)tex).QueueOffset = value;
		}

		public void SetTransparency(object tex, RhuEngine.Linker.Transparency value) {
			((Material)tex).Transparency = (StereoKit.Transparency)value;
		}

		public void SetWireframe(object tex, bool value) {
			((Material)tex).Wireframe = value;
		}
	}
}
