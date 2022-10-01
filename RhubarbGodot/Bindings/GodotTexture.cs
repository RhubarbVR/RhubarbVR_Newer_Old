using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Linker;

using RNumerics;
using Godot;
using System.Xml.Linq;
using Godot.Collections;
using Array = Godot.Collections.Array;
using SArray = System.Array;
using static RNumerics.Colorf;

namespace RhubarbVR.Bindings
{

	public class GodotTexture : IRTexture2D
	{
		public RTexture2D White { get; set; }

		public void Dispose(object tex) {
		}

		public int GetHeight(object target) {
			return ((Texture2D)target).GetHeight();
		}

		public int GetWidth(object target) {
			return ((Texture2D)target).GetWidth();
		}

		public object Make(TexType dynamic, TexFormat rgba32Linear) {
			return new Texture2D();
		}

		public static IEnumerable<byte> GetColorData(Colorb colorb) {
			yield return colorb.r;
			yield return colorb.g;
			yield return colorb.b;
			yield return colorb.a;
		}

		public object MakeFromColors(Colorb[] colors, int width, int height, bool srgb) {
			var image = new Image();
			image.CreateFromData(width, height, false, Image.Format.Rgba8, colors.SelectMany(GetColorData).ToArray());
			return ImageTexture.CreateFromImage(image);
		}

		public void SetAddressMode(object target, TexAddress value) {

		}

		public void SetAnisoptropy(object target, int value) {

		}

		public void SetColors(object tex, int width, int height, byte[] rgbaData) {
			if(tex is ImageTexture image) {
				var newImage = new Image();
				newImage.CreateFromData(width, height, true, Image.Format.Rgba8, rgbaData);
				image.Update(newImage);
			}
		}

		public void SetColors(object tex, int width, int height, Colorb[] rgbaData) {
			if (tex is ImageTexture image) {
				var newImage = new Image();
				newImage.CreateFromData(width, height, false, Image.Format.Rgba8, rgbaData.SelectMany(GetColorData).ToArray());
				image.Update(newImage);
			}
		}

		public void SetSampleMode(object target, TexSample value) {

		}

		public void SetSize(object tex, int width, int height) {

		}
	}
}
