using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;

using RNumerics;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RhuEngine.TextRendering
{
	public class FontAtlisPart
	{
		public const int ATLISSIZE = 2048;
		public Image<Rgba32> _image;
		public ITextMaterial _material;
		public RTexture2D _texture;
		public ImageSharpTexture textureManager;
		public RFont _font;
		public FontAtlisPart(RFont font) {
			//RenderThread.ExecuteOnEndOfFrame(() => {
				_image = new Image<Rgba32>(ATLISSIZE, ATLISSIZE);
				textureManager = new ImageSharpTexture(_image);
				_texture = textureManager.CreateTexture();
				_material = StaticMaterialManager.GetMaterial<ITextMaterial>();
				_material.Texture = _texture;
				_font = font;
			//});
		}
		public Dictionary<Rune, (Vector2f bottomleft, Vector2f topright)> runes = new();
		public void UpdateMit() {
			RenderThread.ExecuteOnEndOfFrame(this,() => textureManager.UpdateTexture());
		}
		public bool HasRune(Rune rune) {
			return runes.ContainsKey(rune);
		}
		private int _xpos = 0;
		private int _ypos = 0;
		private int _yposMax = 0;

		public (RMaterial mit, RTexture2D texture, Vector2f bottomleft, Vector2f topright)? GetGlygh(Rune rune) {
			if (HasRune(rune)) {
				var target = runes[rune];
				return (_material.Material, _texture, target.bottomleft, target.topright);
			}
			var size = _font.GetSizeOfRune(rune);
			if(size.Width + _xpos > ATLISSIZE) {
				if (((int)size.Height + _yposMax) >= ATLISSIZE) {
					return null;//Has no room
				}
				else {
					_ypos = _yposMax;
					_xpos = 0;
				}
			}
			var bottomLeft = new Vector2f(_xpos, _ypos + (int)size.Height);
			var topRight = new Vector2f(_xpos + (int)size.Width, _ypos);
			var topLeft = new Vector2f(_xpos, _ypos);

			var tempOptions = new TextOptions(_font.TextOptions) {
				Origin = new System.Numerics.Vector2(topLeft.x, topLeft.y),
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			_image.Mutate(x => x.DrawText(tempOptions, rune.ToString(), Color.White));
			UpdateMit();
			_xpos += (int)size.Width;
			_yposMax = Math.Max(_yposMax, _ypos + (int)size.Height);
			bottomLeft /= ATLISSIZE;
			topRight /= ATLISSIZE;

			runes.Add(rune, (bottomLeft, topRight));
			return (_material.Material, _texture, bottomLeft, topRight);
		}
	}
}
