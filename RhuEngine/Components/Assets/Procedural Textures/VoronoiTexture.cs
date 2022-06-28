using RNumerics.Voronoi;
using RhuEngine.WorldObjects;
using RNumerics;
using RhuEngine.Linker;
using System;

namespace RhuEngine.Components
{
	public class VoronoiTexture : ProceduralTexture
	{
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Vector2i> Size;

		[Default(12)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> Cells;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> Seed;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> Tint;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> StartingColor;

		public override void OnAttach() {
			base.OnAttach();
			Tint.Value = Colorf.Black;
			Tint.Value = Colorf.White;
			Size.Value = new Vector2i(128);
			Seed.Value = new Random().Next();
		}

		public override void Generate() {
			var _voronoi = new Voronoi();
			var _clampedSizeX = MathUtil.Clamp(Size.Value.x, 2, int.MaxValue);
			var _clampedSizeY = MathUtil.Clamp(Size.Value.y, 2, int.MaxValue);
			var _clapmedSizeCells = MathUtil.Clamp(Cells, 2, 16);

			Load(RTexture2D.FromColors(
				_voronoi.Generate(_clampedSizeX, _clampedSizeY, _clapmedSizeCells, Seed, Tint, StartingColor),
				_clampedSizeX,
				_clampedSizeY,
				true));
		}
	}
}
