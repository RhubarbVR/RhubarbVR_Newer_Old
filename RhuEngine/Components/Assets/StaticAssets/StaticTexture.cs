using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/StaticAssets" })]
	public sealed partial class StaticTexture : StaticAsset<RTexture2D>, IRImageProvider
	{
		public RImage Image => Value?.Image;

		protected override void OnLoaded() {
			base.OnLoaded();
			if (!Engine.EngineLink.CanRender) {
				return;
			}
		}

		public override void LoadAsset(byte[] data) {
			try {
				if (!Engine.EngineLink.CanRender) {
					return;
				}
				if (data is null) {
					Load(null);
					return;
				}
				if (data.Length == 0) {
					Load(null);
					return;
				}
				var Image = new RImage(null);
				Image.LoadWebp(data);
				Load(new RImageTexture2D(Image));
			}
			catch (Exception err) {
				RLog.Err($"Failed to load Static Texture Error {err}");
				Load(null);
			}
		}
	}
}
