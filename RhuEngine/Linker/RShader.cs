using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRShader
	{
	}

	public class RShader
	{
		public RShader(object e) {
			this.e = e;
		}
		public static IRShader Instance { get; set; }
		public object e;
	}
}
