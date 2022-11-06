using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

using RhuEngine.Linker;

namespace RhubarbVR.Bindings.TextureBindings
{
	public class GodotImageTexture2D : GodotTexture2D, IRImageTexture2D, IRTexture2D, IRTexture
	{
		public RFormat Format => (RFormat)(long)ImageTexture.GetFormat();

		public ImageTexture ImageTexture => (ImageTexture)Texture;

		public RImageTexture2D RImageTexture2D { get; private set; }

		public void Init(RImageTexture2D rImageTexture2D, RImage rImage) {
			RImageTexture2D = rImageTexture2D;
			var target = rImage?.Inst;
			if (target is null) {
				Texture = new ImageTexture();
			}
			else {
				if (target is GodotImage image) {
					if (image.Image.IsEmpty()) {
						throw new Exception("Image Empty");
					}
					Texture = ImageTexture.CreateFromImage(image.Image);
					Texture ??= new ImageTexture();
				}
				else {
					Texture = new ImageTexture();
				}
			}
		}

		public void SetImage(IRImage rImage) {
			if (rImage is GodotImage image) {
				ImageTexture.SetImage(image.Image);
			}
		}

		public void UpdateImage(IRImage rImage) {
			if (rImage is GodotImage image) {
				ImageTexture.Update(image.Image);
			}
		}
	}
}
