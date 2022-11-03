using System;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	public abstract class ProceduralTexture : ImageTexture
	{
		protected RImageTexture2D RImageTexture2D;

		protected void TextValueChanged() {
			if (Value is null) {
				return;
			}
		}

		protected void UpdateTexture(Colorf[][] colors) {
			var fullcolors = new Colorb[colors[0].Length * colors.Length];
			for (var y = 0; y < colors.Length; y++) {
				for (var x = 0; x < colors[0].Length; x++) {
					fullcolors[(y * colors[0].Length) + x] = colors[y][x].ToBytes();
				}
			}
			_image.SetColors(colors[0].Length, colors.Length, fullcolors);
			RImageTexture2D.UpdateImageAutoSet(_image);
		}

		protected void UpdateTexture(Colorb[][] colors) {
			var fullcolors = new Colorb[colors[0].Length * colors.Length];
			for (var y = 0; y < colors.Length; y++) {
				for (var x = 0; x < colors[0].Length; x++) {
					fullcolors[(y * colors[0].Length) + x] = colors[y][x];
				}
			}
			_image.SetColors(colors[0].Length, colors.Length, fullcolors);
			RImageTexture2D.UpdateImageAutoSet(_image);
		}
		protected void UpdateTexture(Colorf[] colors, int width, int height) {
			var fullColors = new Colorb[colors.Length];
			for (var i = 0; i < colors.Length; i++) {
				fullColors[i] = colors[i].ToBytes();
			}
			_image.SetColors(width, height, fullColors);
			RImageTexture2D.UpdateImageAutoSet(_image);
		}

		protected void UpdateTexture(Colorb[] colors, int width, int height) {
			_image.SetColors(width, height, colors);
			RImageTexture2D.UpdateImageAutoSet(_image);
		}

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Vector2i> Size;

		protected override void OnAttach() {
			base.OnAttach();
			Size.Value = new Vector2i(128);
		}

		protected abstract void Generate();

		protected override void OnLoaded() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_image = new RImage(null);
			_image.Create(Math.Max(Size.Value.x, 2), Math.Max(Size.Value.y, 2), true, RFormat.Rgba8);
			RImageTexture2D = new RImageTexture2D(Image);
			Load(RImageTexture2D);
			ComputeTexture();
		}

		protected void ComputeTexture() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}

			RenderThread.ExecuteOnStartOfFrame(this, () => {
				try {
					if (_image is null) {
						return;
					}
					Generate();
				}
				catch (Exception e) {
#if DEBUG
					RLog.Err(e.ToString());
#endif
				}
			});
		}
	}
}
