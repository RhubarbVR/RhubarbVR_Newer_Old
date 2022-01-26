using System.Threading.Tasks;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using StereoKit;
using LibVLCSharp;
using System;
using LibVLCSharp.Shared;
using System.Threading;

namespace RhuEngine.Components
{
	public class VideoPlayer : AssetProvider<Tex>
	{

		[OnChanged(nameof(StartLoadVideo))]
		public Sync<string> Url;

		LibVLC _libVLC;
		MediaPlayer _mediaPlayer;

		private async Task LoadVideo() {
			if(Uri.TryCreate(Url.Value,UriKind.RelativeOrAbsolute,out var uri)) {
				Core.Initialize();
				_libVLC = new LibVLC();
				_mediaPlayer = new MediaPlayer(_libVLC);
				var media = new Media(_libVLC, uri);
				if (WorldObjects.World.IsVideoStreaming(uri)) {
					await media.Parse(MediaParseOptions.ParseNetwork);
					while (media.State == VLCState.Buffering) {
						Thread.Sleep(10);
					}
				}
				_mediaPlayer.Play(media.SubItems[0]);
			}
		}

		private void StartLoadVideo() {
			LoadVideo().ConfigureAwait(false);
		}

		public override void OnLoaded() {
			base.OnLoaded();
			StartLoadVideo();
		}

	}
}
