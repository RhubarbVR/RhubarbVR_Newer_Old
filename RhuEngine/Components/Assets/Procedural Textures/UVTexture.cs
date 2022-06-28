using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Textures" })]
	public class UVTexture : ProceduralTexture
	{
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Vector2i> Size;

		public override void OnAttach() {
			base.OnAttach();
			Size.Value = new Vector2i(128);
		}

		public override void Generate() 
		{
			var _clampedSizeX = MathUtil.Clamp(Size.Value.x, 2, int.MaxValue);
			var _clampedSizeY = MathUtil.Clamp(Size.Value.y, 2, int.MaxValue);

			var uvData = new Colorb[_clampedSizeX * _clampedSizeY];
			var index = 0;

			for (var y = 0; y < _clampedSizeY; ++y) 
			{
				for (var x = 0; x < _clampedSizeX; ++x)
				{
					uvData[index++] = new Colorb(
						(float) x / (float) _clampedSizeX,
						(float) y / (float) _clampedSizeY,
						0f,
						1f);
				}
			}

			Load(RTexture2D.FromColors(uvData, _clampedSizeX, _clampedSizeY, true));
		}
	}
}
