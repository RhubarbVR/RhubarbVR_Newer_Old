using System;
using RNumerics;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Textures" })]
	public class CheckerboardTexture : ProceduralTexture
	{
		[Default(10.0f)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<float> tileWidth;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> CheckerOne;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> CheckerTwo;
		public override void OnAttach() {
			base.OnAttach();
			CheckerOne.Value = Colorf.Black;
			CheckerTwo.Value = Colorf.White;
		}

		public override void Generate() {
			if (tileWidth.Value * 2 >= Size.Value.x || tileWidth.Value * 2 >= Size.Value.y) {
				throw new Exception($"tileWidth {tileWidth.Value} cannot be larger than texture size {Size.Value.x}x {Size.Value.y}y.");
			}

			var checkerBoardPixels = new Colorf[Size.Value.x * Size.Value.y];
			var index = 0;

			for (var y = 0; y < Size.Value.y; y++) {
				for (var x = 0; x < Size.Value.x; x++) {
					checkerBoardPixels[index++] = GetCheckerboardPixel(x, y);
				}
			}

			UpdateTexture(checkerBoardPixels, Size.Value.x, Size.Value.y);
		}

		private Colorf GetCheckerboardPixel(int x, int y) {
			var valueX = x % (tileWidth * 2) / (tileWidth * 2);
			var vX = 1;
			if (valueX < 0.5f) {
				vX = 0;
			}

			var valueY = y % (tileWidth * 2) / (tileWidth * 2);
			var vY = 1;
			if (valueY < 0.5f) {
				vY = 0;
			}

			var color = CheckerOne.Value;
			if (vX == vY) {
				color = CheckerTwo.Value;
			}

			return color;
		}
	}
}
