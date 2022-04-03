using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRMaterial {
		public Transparency GetTransparency(object tex);
		public void SetTransparency(object tex, Transparency value);
		public bool GetWireframe(object tex);
		public void SetWireframe(object tex, bool value);
		public int GetQueueOffset(object tex);
		public void SetQueueOffset(object tex, int value);
		public Cull GetFaceCull(object tex);
		public void SetFaceCull(object tex, Cull value);
		public bool GetDepthWrite(object tex);
		public void SetDepthWrite(object tex, bool value);
		public DepthTest GetDepthTest(object tex);
		public void SetDepthTest(object tex, DepthTest value);

		public object Make(RShader rShader);
		public void Pram(object ex,string tex,object value);
		public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex);
	}

	public class RMaterial
	{
		public static IRMaterial Instance { get; set; }

		public object Target;

		public bool Wireframe
		{
			get=>Instance.GetWireframe(Target);
			set=>Instance.SetWireframe(Target,value);
		}
		public Transparency Transparency
		{
			get => Instance.GetTransparency(Target);
			set => Instance.SetTransparency(Target, value);
		}
		public int QueueOffset
		{
			get => Instance.GetQueueOffset(Target);
			set => Instance.SetQueueOffset(Target, value);
		}
		public Cull FaceCull
		{
			get => Instance.GetFaceCull(Target);
			set => Instance.SetFaceCull(Target, value);
		}
		public bool DepthWrite
		{
			get => Instance.GetDepthWrite(Target);
			set => Instance.SetDepthWrite(Target, value);
		}
		public DepthTest DepthTest
		{
			get => Instance.GetDepthTest(Target);
			set => Instance.SetDepthTest(Target, value);
		}

		public RMaterial(object target) {
			Target = target;
		}

		public RMaterial(RShader rShader) {
			Target = Instance.Make(rShader);
		}
		public object this[string parameterName]
		{
			set => Instance.Pram(Target, parameterName, value);
		}

		public IEnumerable<RMatParamInfo> GetAllParamInfo() {
			return Instance.GetAllParamInfo(Target);
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

			internal RMatParamInfo(string name, MaterialParam type) {
				this.name = name;
				this.type = type;
			}
		}
	}
}
