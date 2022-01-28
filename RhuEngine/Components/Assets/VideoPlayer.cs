using System.Threading.Tasks;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using StereoKit;
using LibVLCSharp;
using System;
using LibVLCSharp.Shared;
using System.Threading;
using System.Runtime.InteropServices;

namespace RhuEngine.Components
{
	public class VideoPlayer : AssetProvider<Tex>
	{

		public class AudioVideoPlayer: SyncObject, IAssetProvider<Sound>
		{

			public event Action<Sound> OnAssetLoaded;

			public Sound Value { get; private set; }

			public void Load(Sound data) {
				Value = data;
				Loaded = data != null;
				OnAssetLoaded?.Invoke(data);
			}

			public bool Loaded { get; private set; } = false;

			public override void Dispose() {
				Load(null);
				base.Dispose();
			}

			public Sound audio;

			public override void OnLoaded() {
				audio = Sound.CreateStream(5f);
				Load(audio);
			}

			public void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts) {
				if (audio is null) {
					return;
				};
				var Count = (int)count;
				var buffer = new float[Count];
				Marshal.Copy(samples, buffer, 0, Count);
				audio.WriteSamples(buffer);
			}

		}

		[OnChanged(nameof(StartLoadVideo))]
		public Sync<string> Url;

		public AudioVideoPlayer audioPlayer;

		LibVLC _libVLC;
		MediaPlayer _mediaPlayer;

		Tex _texer;

		public void LoadTexture() {
			if (_texer is null) {
				_texer = new Tex(TexType.Image);
				Load(_texer);
			}
		}

		int AudioSetup(ref IntPtr opaque, ref IntPtr format, ref uint rate, ref uint channels) {
			channels = 1;
			rate = 48000;
			return 0;
		}

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
				_mediaPlayer.SetAudioFormatCallback(AudioSetup,null);
				_mediaPlayer.SetAudioFormat("F32L", 48000, 1);
				_mediaPlayer.SetAudioCallbacks(audioPlayer.PlayAudio, null, null,null, null);
				//_mediaPlayer.SetVideoFormat("RGBA", 1920, 1080, sizeof(float) * 4);
				//_mediaPlayer.SetVideoCallbacks(LibVLCVideoLockCb, null, null);
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
