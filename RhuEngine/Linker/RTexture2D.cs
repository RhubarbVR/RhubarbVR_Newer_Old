using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Linker
{

	public interface IRTexture2D : IRTexture, IDisposable
	{
		long Height { get; }
		long Width { get; }
		bool HasAlpha { get; }

		bool IsPixelOpaque(int x, int y);

		IRImage GetImage();

		void Init(RTexture2D rTexture2D);
	}

	public class RTexture2D : RTexture, IDisposable
	{
		public static new Type Instance { get; set; }

		protected RImage _targetImage;

		public IRTexture2D Texture2D => (IRTexture2D)Inst;

		public RTexture2D(IRTexture2D tex) : base(tex) {
			if (typeof(RTexture2D) == GetType()) {
				Inst = tex ?? (IRTexture2D)Activator.CreateInstance(Instance);
				((IRTexture2D)Inst).Init(this);
			}
		}

		public virtual RImage Image
		{
			get {
				if (_targetImage?.Inst is null) {
					_targetImage = new RImage(Texture2D.GetImage());
				}
				return _targetImage;
			}
		}


		public long Height => Texture2D?.Height ?? 0;
		public long Width => Texture2D?.Width ?? 0;
		public bool HasAlpha => Texture2D?.HasAlpha ?? false;

		public bool IsPixelOpaque(int x, int y) {
			return Texture2D.IsPixelOpaque(x, y);
		}

		public static RTexture2D White { get; set; }
	}
}
