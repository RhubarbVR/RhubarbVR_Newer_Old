using System;
using System.Runtime.CompilerServices;

namespace RNumerics.Voronoi
{
	public static class Voronoi
	{
		public static Colorb[] Generate(int height, int width, int cells, int seed, Colorf tint, Colorf startingColor) {
			var _tex = new Colorb[width * height];
			var _cell = new Vector2i[cells];
			var rand = new Random(seed);
			PlaceCellOrigins(ref _cell, rand, height, width);
			FillPixelsAccordingToCell(ref _tex, ref _cell, height, width, tint, startingColor);
			// Creates pixel artifacts
			// StrokeCellPixel(ref _tex, ref _cell);
			return _tex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void PlaceCellOrigins(ref Vector2i[] _cell, Random rand, int height, int width) {
			for (var i = 0; i < _cell.Length; i++) {
				_cell[i] = new Vector2i(rand.Next(0, width), rand.Next(0, height)); // GetRandomPixel
			}
		}

		private static void FillPixelsAccordingToCell(ref Colorb[] _tex, ref Vector2i[] _cell, int height, int width, Colorf tint, Colorf startingColor) {
			for (var y = 0; y < height; y++) {
				for (var x = 0; x < width; x++) {
					var dist = new float[_cell.Length];
					for (var c = 0; c < _cell.Length; c++) {
						Vector2i temp = new(x, y);
						dist[c] = ((Vector2f)_cell[c]).Distance((Vector2f)temp);
					}
					Array.Sort(dist);
					_tex[(x * width) + y] = (startingColor * ColourIntensity(dist[0], width, tint)).ToBytes();
				}
			}
		}

		private static Colorf ColourIntensity(float distance, int width, Colorf tint) {
			// var n = Mathf.Abs(map(x, 0, size, 0, 1));
			var n = distance / width;
			return new Colorf(n, n, n) * tint;
		}

		private static void StrokeCellPixel(ref Colorb[] _tex, ref Vector2i[] _cell) {
			for (var i = 0; i < _cell.Length; i++) {
				_tex[(byte)_cell[i].x + (byte)_cell[i].y] = new Colorb(0, 1, 0);
			}
		}
	}
}
