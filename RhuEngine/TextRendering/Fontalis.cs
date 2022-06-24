using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;

using RNumerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RhuEngine.TextRendering
{
	public class FontAtlisPart
	{
		public const int ATLISSIZE = 4096;
		public Image<Rgba32> _image;

		public FontAtlisPart() {
			_image = new Image<Rgba32>(ATLISSIZE, ATLISSIZE);
		}
	}

	public class FontAtlis
	{
		public SafeList<FontAtlisPart> _parts = new();

	}
}
