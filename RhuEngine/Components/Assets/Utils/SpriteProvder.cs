using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.Linker;
using TextCopy;
using RNumerics;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Utils" })]
	public sealed class SpriteProvder : Component
	{
		public readonly AssetRef<RTexture2D> Texture;

		public readonly Sync<Vector2i> GridSize;
		
		[Exposed]
		public Vector2i GetSizeOfSprite(Vector2i min, Vector2i max) {
			max += Vector2i.One;
			var size = max - min;
			var x = Texture.Asset.Width / GridSize.Value.x;
			var y = Texture.Asset.Height / GridSize.Value.y;
			return new Vector2i((int)x, (int)y) * size;
		}

		[Exposed]
		public (Vector2f, Vector2f) GetSpriteSizePoints(Vector2i min, Vector2i max) {
			max += Vector2i.One;
			var size = (Vector2f)(max - min) / (Vector2f)GridSize.Value;
			var bottomleft = (Vector2f)min / (Vector2f)GridSize.Value;
			return (bottomleft, size);
		}
	}
}
