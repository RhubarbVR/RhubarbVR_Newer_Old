using System;
using System.Collections.Generic;
using System.Text;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Textures" })]
	public sealed partial class EdgeTexture : ProceduralTexture
	{
		[Default(1)]
		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<int> EdgeWidth;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> BackgroundColor;

		[OnChanged(nameof(ComputeTexture))]
		public readonly Sync<Colorf> InnerColor;

		protected override void Generate() {

			if (EdgeWidth.Value * 2 >= Size.Value.x || EdgeWidth.Value * 2 >= Size.Value.y) {
				throw new Exception($"Edge width {EdgeWidth.Value} cannot be larger than texture size {Size.Value.x}x {Size.Value.y}y.");
			}

			var arr = new Colorf[Size.Value.x * Size.Value.y];

			// Background
			for (var i = 0; i < arr.Length; i++) {
				arr[i] = BackgroundColor.Value;
			}

			// Inner
			for (var i = EdgeWidth.Value; i < Size.Value.x - EdgeWidth.Value; i++) {
				for (var j = EdgeWidth.Value; j < Size.Value.y - EdgeWidth.Value; j++) {
					arr[(i * Size.Value.x) + j] = InnerColor.Value;
				}
			}

			UpdateTexture(arr, Size.Value.x, Size.Value.y);
		}
	}
}
