using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public class SpriteProvider : AssetProvider<Sprite>
	{
		[OnAssetLoaded(nameof(ReloadSprite))]
		public AssetRef<Tex> Image;

		[Default(SpriteType.Atlased)]
		[OnChanged(nameof(ReloadSprite))]
		public Sync<SpriteType> Type;

		[Default("default")]
		[OnChanged(nameof(ReloadSprite))]
		public Sync<string> Atlasid;

		private Sprite _sprite;

		public void ReloadSprite() {
			if(Image.Asset is null) {
				_sprite = null;
				Load(null);
				return;
			}
			_sprite = Sprite.FromTex(Image.Asset, Type,Atlasid);
			Load(_sprite);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			ReloadSprite();
		}
	}
}
