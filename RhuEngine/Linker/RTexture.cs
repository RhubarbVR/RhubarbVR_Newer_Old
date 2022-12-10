using System;
using System.Collections.Generic;
using System.Text;

namespace RhuEngine.Linker
{
	public interface IRTexture : IDisposable
	{
		void Init(RTexture rTexture);
	}

	public class RTexture : IDisposable
	{
		public static Type Instance { get; set; }

		public IRTexture Inst { get; set; }

		public RTexture(IRTexture tex) {
			if (typeof(RTexture) == GetType()) {
				Inst = tex ?? (IRTexture)Activator.CreateInstance(Instance);
				Inst.Init(this);
			}
		}

		public void Dispose() {
			Inst.Dispose();
			Inst = null;
			GC.SuppressFinalize(this);
		}
	}
}
