using RNumerics.Voronoi;
using RhuEngine.WorldObjects;
using RNumerics;
using RhuEngine.Linker;
using System;
using RhuEngine.WorldObjects.ECS;


namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Textures" })]
	public sealed class VoronoiTexture : ProceduralTexture
	{

		[Default(12)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> Cells;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> Seed;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> Tint;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> StartingColor;

		protected override void OnAttach() {
			base.OnAttach();
			StartingColor.Value = Colorf.Black;
			Tint.Value = Colorf.White;
			Seed.Value = new Random().Next();
		}

		protected override void Generate() {
			var _clampedSizeX = MathUtil.Clamp(Size.Value.x, 2, int.MaxValue);
			var _clampedSizeY = MathUtil.Clamp(Size.Value.y, 2, int.MaxValue);
			var _clapmedSizeCells = MathUtil.Clamp(Cells, 2, 16);

			UpdateTexture(
				Voronoi.Generate(_clampedSizeX, _clampedSizeY, _clapmedSizeCells, Seed, Tint, StartingColor),
				_clampedSizeX,
				_clampedSizeY);
		}
	}
}
