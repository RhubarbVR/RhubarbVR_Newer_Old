using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;

using SixLabors.Fonts;

namespace RhuEngine
{
	public static class FontManager
	{
		public static Vector2f Size(RFont font, Rune item) {
			var textRect = font.GetSizeOfText(item.ToString());
			return new Vector2f(textRect.Width / textRect.Height,1);
		}
	}
}
