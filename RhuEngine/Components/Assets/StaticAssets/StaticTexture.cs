using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;
namespace RhuEngine.Components
{
	[Category(new string[] { "Assets\\StaticAssets" })]
	public class StaticTexture : StaticAsset<Tex>
	{

		[Default(TexSample.Anisotropic)]
		[OnChanged(nameof(TextValueChanged))]
		public Sync<TexSample> sampleMode;

		[Default(TexAddress.Wrap)]
		[OnChanged(nameof(TextValueChanged))]
		public Sync<TexAddress> addressMode;

		[Default(3)]
		[OnChanged(nameof(TextValueChanged))]
		public Sync<int> anisoptropy;

		private void TextValueChanged() {
			if(Value is null) {
				return;
			}
			Value.Anisoptropy = anisoptropy;
			Value.AddressMode = addressMode;
			Value.SampleMode = sampleMode;
		}

		public override void LoadAsset(byte[] data) {
			try {
				Load(new ImageSharpTexture(new MemoryStream(data), true).CreateTexture());
				TextValueChanged();
			}
			catch(Exception err) {
				Log.Err($"Failed to load Static Texture Error {err}");
			}
		}
	}
}
