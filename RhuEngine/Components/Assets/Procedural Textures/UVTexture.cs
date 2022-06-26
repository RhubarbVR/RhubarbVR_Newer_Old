using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.Linker;
using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.Components
{
	public class UVTexture : ProceduralTexture
	{
		[Default(128)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> SizeX;

		[Default(128)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> SizeY;

		public override void Generate() 
		{
			var _clampedSizeX = MathUtil.Clamp(SizeX, 2, int.MaxValue);
			var _clampedSizeY = MathUtil.Clamp(SizeY, 2, int.MaxValue);

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
