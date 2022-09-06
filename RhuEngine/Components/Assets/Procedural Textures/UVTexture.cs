using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Textures" })]
	public class UVTexture : ProceduralTexture
	{
		protected override void Generate() 
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

			UpdateTexture(uvData, _clampedSizeX, _clampedSizeY);
		}
	}
}
