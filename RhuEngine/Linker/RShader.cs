using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRShader
	{
		public RShader UnlitClip { get; }
		public RShader PBRClip { get; }
		public RShader PBR { get; }
		public RShader Unlit { get; }
	}

	public class RShader
	{
		public RShader(object e) {
			this.e = e;
		}
		public static IRShader Instance { get; set; }
		public object e;
		public static RShader UnlitClip => Instance.UnlitClip;
		public static RShader PBRClip => Instance.PBRClip;
		public static RShader PBR => Instance.PBR;
		public static RShader Unlit => Instance.Unlit;
	}
}
