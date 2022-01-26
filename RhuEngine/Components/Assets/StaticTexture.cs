using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public class StaticTexture : StaticAsset<Tex>
	{
		public override void LoadAsset(byte[] data) {
			Load(new ImageSharpTexture(new MemoryStream(data), true).CreateTexture());
		}
	}
}
