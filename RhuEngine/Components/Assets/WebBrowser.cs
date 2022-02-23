using System.Threading.Tasks;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using StereoKit;
using LibVLCSharp;
using System;
using LibVLCSharp.Shared;
using System.Threading;
using System.Runtime.InteropServices;
using System.Linq;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Platform;

namespace RhuEngine.Components
{
	public class WebBrowser : AssetProvider<Tex>
	{

		Tex _texer;

		[OnChanged(nameof(UrlChanged))]
		public Sync<string> Url;

		public RawAudioClip audio;

		private void UrlChanged() {
		}


		private void UpdateWebBrowser() {
			if (_texer is not null) {
				return;
			}
			_texer = new Tex(TexType.Image);
			Load(_texer);
			

		}

		public override void OnLoaded() {
			base.OnLoaded();
			UpdateWebBrowser();
		}

	}
}
