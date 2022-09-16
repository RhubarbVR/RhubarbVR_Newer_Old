using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRMaterial
	{
		public void SetRenderQueueOffset(object mit, int tex);

		public object Make(RShader rShader);
		public void Pram(object ex, string tex, object value);
		public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex);
	}

	public interface IRMitConsts
	{
		public string FaceCull { get; }

		public string Transparency { get; }

		public string MainTexture { get; }

		public string WireFrame { get; }
		public string MainColor { get; }
	}

	public partial class RMaterial : IDisposable
	{

		public int RenderOrderOffset
		{
			set { Instance.SetRenderQueueOffset(Target, value); PramChanged?.Invoke(this); }
		}

		public void UpdatePrams() {
			RenderThread.ExecuteOnEndOfFrame(this, () =>
				PramChanged?.Invoke(this)
			);
		}

		public event Action<RMaterial> PramChanged;

		public static IRMitConsts ConstInstance { get; set; }

		public static string FaceCull => ConstInstance?.FaceCull;

		public static string Transparency => ConstInstance?.Transparency;

		public static string MainTexture => ConstInstance?.MainTexture;

		public static string WireFrame => ConstInstance?.WireFrame;

		public static string MainColor => ConstInstance?.MainColor;

		public static IRMaterial Instance { get; set; }

		public object Target;

		public RMaterial(object target) {
			Target = target;
		}

		public RMaterial(RShader rShader) {
			Target = Instance.Make(rShader);
		}
		public object this[string parameterName]
		{
			set { Instance.Pram(Target, parameterName, value); PramChanged?.Invoke(this); }
		}

		public static string RenameFromEngine(string newName) {
			if (newName == FaceCull) {
				return "FACECULL_RHUBARB_CUSTOM";
			}
			else if (newName == Transparency) {
				return "TRANSPARENCY_RHUBARB_CUSTOM";
			}
			else if (newName == MainTexture) {
				return "MAINTEXTURE_RHUBARB_CUSTOM";
			}
			else if (newName == WireFrame) {
				return "WIREFRAME_RHUBARB_CUSTOM";
			}
			return newName;
		}
		public static string RenameFromRhubarb(string newName) {
			if (newName == "FACECULL_RHUBARB_CUSTOM") {
				return FaceCull;
			}
			else if (newName == "TRANSPARENCY_RHUBARB_CUSTOM") {
				return Transparency;
			}
			else if (newName == "MAINTEXTURE_RHUBARB_CUSTOM") {
				return MainTexture;
			}
			else if (newName == "WIREFRAME_RHUBARB_CUSTOM") {
				return WireFrame;
			}
			return newName;
		}
		public IEnumerable<RMatParamInfo> GetAllParamInfo() {
			return Instance.GetAllParamInfo(Target);
		}

		public event Action<RMaterial> OnDispose;

		public void Dispose() {
			OnDispose?.Invoke(this);
			Target = null;
		}

		/// <summary>Information and details about the shader parameters
		/// available on a Material. Currently only the text name and data type
		/// of the parameter are surfaced here. This contains textures as well as
		/// global constant buffer variables.</summary>
		public struct RMatParamInfo
		{
			/// <summary>The name of the shader parameter, as provided inside of
			/// the shader file itself. These are the 'global' variables defined
			/// in the shader, as well as textures. The shader compiler will
			/// output a list of parameter names when compiling, so check the
			/// output window after building if you're uncertain what you'll
			/// find.
			/// 
			/// See `MatParamName` for a list of "standardized" parameter names.
			/// </summary>
			public string name;

			/// <summary>This is the data type that StereoKit recognizes the
			/// parameter to be.</summary>
			public MaterialParam type;

			public Type HardType = null;

			internal RMatParamInfo(string name, MaterialParam type, Type hardtype) {
				this.name = name;
				this.type = type;
				HardType = hardtype;
			}
		}
	}
}
