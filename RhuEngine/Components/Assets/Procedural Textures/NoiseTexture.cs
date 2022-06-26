using System;
using System.Collections.Generic;
using System.Text;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Textures" })]
	public class NoiseTexture : ProceduralTexture
	{
		private static readonly FastNoiseLite _noise = new FastNoiseLite();

		[Default(128)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> SizeX;

		[Default(128)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> SizeY;

		[Default(FastNoiseLite.NoiseType.OpenSimplex2)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<FastNoiseLite.NoiseType> NoiseType;

		public override void Generate() 
		{
			var _clampedSizeX = MathUtil.Clamp(SizeX, 2, int.MaxValue);
			var _clampedSizeY = MathUtil.Clamp(SizeY, 2, int.MaxValue);
			_noise.SetNoiseType(NoiseType);
			var noiseData = new Colorb[_clampedSizeX * _clampedSizeY];

			var index = 0;
			for (var y = 0; y < _clampedSizeX; y++) {
				for (var x = 0; x < _clampedSizeY; x++) {
					var noisePixel = (_noise.GetNoise(x, y) + 1) / 2; // Remap -1/1 to 0/1
					noiseData[index++] = new Colorb(
						noisePixel,
						noisePixel,
						noisePixel,
						1f);
				}
			}

			Load(RTexture2D.FromColors(noiseData, _clampedSizeX, _clampedSizeY, true));
		}
	}
}