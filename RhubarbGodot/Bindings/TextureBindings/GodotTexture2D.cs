using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Linker;

namespace RhubarbVR.Bindings.TextureBindings
{
	public class GodotTexture2D : GodotTexture, IRTexture2D
	{
		public GodotTexture2D(Texture2D texture2D) {
			Texture = texture2D;
		}

		public GodotTexture2D() {

		}

		public long Height => Texture2D.GetHeight();

		public long Width => Texture2D.GetWidth();

		public bool HasAlpha => Texture2D.HasAlpha();

		public IRImage GetImage() {
			return new GodotImage(Texture2D.GetImage());
		}

		public Texture2D Texture2D => (Texture2D)Texture;

		public RTexture2D RTexture2D { get; private set; }

		public void Init(RTexture2D rTexture2D) {
			RTexture2D = rTexture2D;
			if (typeof(GodotTexture2D) == GetType()) {
				Texture ??= new Texture2D();
			}
		}

		public bool IsPixelOpaque(int x, int y) {
			return Texture2D._IsPixelOpaque(x, y);
		}
	}
}
