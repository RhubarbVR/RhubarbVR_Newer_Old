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
	public sealed class StaticTexture : StaticAsset<RTexture2D>,IRImageProvider
	{
		public RImage Image { get; private set; }

		private RImageTexture2D _rImageTexture2D;

		protected override void OnLoaded() {
			base.OnLoaded();
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			_rImageTexture2D = new RImageTexture2D(null);
			Load(_rImageTexture2D);
		}

		public override void LoadAsset(byte[] data) {
			try {
				if (!Engine.EngineLink.CanRender) {
					return;
				}
				if(data is null) {
					Load(null);
					return;
				}
				if (data.Length == 0) {
					Load(null);
					return;
				}
				Image?.Dispose();
				Image = new RImage(null);
				Image.LoadWebp(data);
				_rImageTexture2D.SetImage(Image);
				Load(_rImageTexture2D);
			}
			catch (Exception err) {
				RLog.Err($"Failed to load Static Texture Error {err}");
				Load(null);
			}
		}
	}
}
