using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RhuEngine.WorldObjects;

using RNumerics;


namespace RhuEngine.Linker
{
	public interface IRImageTexture2D : IRTexture2D {
		void Init(RImageTexture2D rImageTexture2D,RImage rImage);

		void SetImage(IRImage rImage);

		void UpdateImage(IRImage rImage);

		RFormat Format { get; }
	}


	public class RImageTexture2D : RTexture2D, IDisposable
	{
		public static new Type Instance { get; set; }
		public IRImageTexture2D ImageTexture2D => (IRImageTexture2D)Inst;

		public RFormat Format => ImageTexture2D.Format;

		public RImageTexture2D(RImage rImage):this(null,rImage) {
		}

		public RImageTexture2D(IRImageTexture2D tex,RImage rImage) : base(tex) {
			Inst = tex ?? (IRImageTexture2D)Activator.CreateInstance(Instance);
			((IRImageTexture2D)Inst).Init(this, rImage);
			_targetImage = rImage;
		}

		public static RImageTexture2D Create(int width, int height, bool mipmaps, RFormat format) {
			var image = new RImage(null);
			image.Create(width, height, mipmaps, format);
			return new RImageTexture2D(image);
		}
		public static RImageTexture2D CreateWithData(int width, int height, bool mipmaps, RFormat format, byte[] data) {
			var image = new RImage(null);
			image.CreateWithData(width, height, mipmaps, format,data);
			return new RImageTexture2D(image);
		}

		public void SetImage(RImage rImage) {
			ImageTexture2D.SetImage(rImage.Inst);
			_targetImage = rImage;
		}

		public void UpdateImage(RImage rImage) {
			ImageTexture2D.UpdateImage(rImage.Inst);
			_targetImage = rImage;
		}

	}
}
