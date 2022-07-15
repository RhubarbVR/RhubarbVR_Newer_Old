using System;
using RNumerics;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	public abstract class ProceduralTexture : AssetProvider<RTexture2D>
	{
		[Default(TexSample.Anisotropic)]
		[OnChanged(nameof(TextValueChanged))]
		public readonly Sync<TexSample> sampleMode;

		[Default(TexAddress.Wrap)]
		[OnChanged(nameof(TextValueChanged))]
		public readonly Sync<TexAddress> addressMode;

		[Default(3)]
		[OnChanged(nameof(TextValueChanged))]
		public readonly Sync<int> anisoptropy;

		public void TextValueChanged() {
			if (Value is null) {
				return;
			}
			Value.Anisoptropy = anisoptropy;
			Value.AddressMode = addressMode;
			Value.SampleMode = sampleMode;
		}

		public void UpdateTexture(Colorf[][] colors) {
			var fullcolors = new Colorb[colors[0].Length * colors.Length];
			for (var y = 0; y < colors.Length; y++) {
				for (var x = 0; x < colors[0].Length; x++) {
					fullcolors[(y * colors[0].Length) + x] = colors[y][x].ToBytes();
				}
			}
			if (Value is null) {
				Load(RTexture2D.FromColors(fullcolors, colors[0].Length, colors.Length, true));
				TextValueChanged();
				return;
			}
			Value.SetColors(colors[0].Length, colors.Length, fullcolors);
		}

		public void UpdateTexture(Colorb[][] colors) {
			var fullcolors = new Colorb[colors[0].Length * colors.Length];
			for (var y = 0; y < colors.Length; y++) {
				for (var x = 0; x < colors[0].Length; x++) {
					fullcolors[(y * colors[0].Length) + x] = colors[y][x];
				}
			}
			if (Value is null) {
				Load(RTexture2D.FromColors(fullcolors, colors[0].Length, colors.Length, true));
				TextValueChanged();
				return;
			}
			Value.SetColors(colors[0].Length, colors.Length, fullcolors);
		}
		public void UpdateTexture(Colorf[] colors, int width, int height) {
			var fullColors = new Colorb[colors.Length];
			for (var i = 0; i < colors.Length; i++) {
				fullColors[i] = colors[i].ToBytes();
			}
			if (Value is null) {
				Load(RTexture2D.FromColors(fullColors, width, height, true));
				TextValueChanged();
				return;
			}
			Value.SetColors(width, height, fullColors);
		}

		public void UpdateTexture(Colorb[] colors, int width,int height) {
			if(Value is null) {
				Load(RTexture2D.FromColors(colors, width, height,true));
				TextValueChanged();
				return;
			}
			Value.SetColors(width, height, colors);
		}

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Vector2i> Size;

		public override void OnAttach() {
			base.OnAttach();
			Size.Value = new Vector2i(128);
		}

		public abstract void Generate();

		public override void OnLoaded() 
		{
			ComputeTexture();
		}

		public void ComputeTexture() 
		{
			if (!Engine.EngineLink.CanRender) 
			{
				return;
			}

			RWorld.ExecuteOnEndOfFrame(this, () => {
				try {
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
