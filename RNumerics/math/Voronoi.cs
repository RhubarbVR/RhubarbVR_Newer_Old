using System;

namespace RNumerics.Voronoi
{
	public class Voronoi
	{
		private int _numberOfCells;
		private int _width;
		private int _height;
		private Random _rand;
		private Colorf _tint;
		private Colorf _startingColor;

		public Colorb[] Generate(int height, int width, int cells, int seed, Colorf tint, Colorf startingColor) {
			var _tex = new Colorb[width * height];
			var _cell = new Vector2i[cells];
			_height = height;
			_width = width;
			_numberOfCells = cells;
			_rand = new Random(seed);
			_tint = tint;
			_startingColor = startingColor;

			FillBackground(ref _tex);
			PlaceCellOrigins(ref _cell);
			FillPixelsAccordingToCell(ref _tex, ref _cell);
			// Creates pixel artifacts
			// StrokeCellPixel(ref _tex, ref _cell);

			return _tex;
		}

		private void FillBackground(ref Colorb[] _tex) {
			for (var x = 0; x < _width * _height; x++) {
				_tex[x] = _startingColor.ToBytes();
			}
		}

		private void PlaceCellOrigins(ref Vector2i[] _cell) {
			for (var i = 0; i < _numberOfCells; i++) {
				_cell[i] = new Vector2i(_rand.Next(0, _width), _rand.Next(0, _height)); // GetRandomPixel
			}
		}

		private void FillPixelsAccordingToCell(ref Colorb[] _tex, ref Vector2i[] _cell) {

			for (var y = 0; y < _height; y++) {
				for (var x = 0; x < _width; x++) {
					var dist = new float[_cell.Length];
					for (var c = 0; c < _cell.Length; c++) {
						Vector2i temp = new(x, y);
						dist[c] = ((Vector2f)_cell[c]).Distance((Vector2f)temp);
					}

					Array.Sort(dist);
					_tex[(x * _width) + y] = ColourIntensity(dist[0]);
				}
			}
		}

		private Colorb ColourIntensity(float distance) {
			// var n = Mathf.Abs(map(x, 0, size, 0, 1));
			var n = distance / _width;
			return (new Colorf(n, n, n) * _tint).ToBytes();
		}

		private void StrokeCellPixel(ref Colorb[] _tex, ref Vector2i[] _cell) {
			for (var i = 0; i < _numberOfCells; i++) {
				_tex[(byte)_cell[i].x + (byte)_cell[i].y] = new Colorb(0, 1, 0); 
			}
		}
	}
}
