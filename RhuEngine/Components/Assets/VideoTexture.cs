using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using LibVLCSharp.Shared;

using RhuEngine.VLC;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;


namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public class VideoTexture : StaticAsset<Tex>
	{
		public VlcVideoSourceProvider vlcVideoSourceProvider = null;

		[OnChanged(nameof(UpdateVideo))]
		public SyncObjList<AudioOutput> AudioChannels;

		LibVLC _libVLC;
		MediaPlayer _mediaPlayer;

		public override void OnAttach() {
			base.OnAttach();
			AudioChannels.Add();
		}

		public void UpdateVideo() {
			LoadVideoPlayer();
		}

		public override void OnLoaded() {
			UpdateVideo();
			base.OnLoaded();
		}

		private void LoadVideoPlayer() {
			try {
				if (vlcVideoSourceProvider != null) {
					Log.Info("Reloading Loading Video Player");
					Load(null);
					vlcVideoSourceProvider.Dispose();
					vlcVideoSourceProvider = null;
					_libVLC = null;
					_mediaPlayer = null;
				}
				else {
					Log.Info("Loading Video Player");
				}
				vlcVideoSourceProvider = new VlcVideoSourceProvider {
					ChannelCount = (uint)AudioChannels.Count
				};
				Load(vlcVideoSourceProvider.VideoSource);
				Core.Initialize();
				_libVLC = new LibVLC(enableDebugLogs: false);
				_mediaPlayer = new MediaPlayer(_libVLC);
				vlcVideoSourceProvider.LoadPlayer(_mediaPlayer);
				vlcVideoSourceProvider.RelaodTex += VlcVideoSourceProvider_RelaodTex;
				vlcVideoSourceProvider.LoadAudio += (samps, chan) => {
					if (AudioChannels.Count <= chan) {
						return;
					}
					AudioChannels[chan].WriteAudio(samps);
				};
				LoadMedia().ConfigureAwait(false);
			}
			catch (Exception ex) 
			{
				Log.Err($"Failed to start Video Player Error:{ex}");
			}
		}

		private void VlcVideoSourceProvider_RelaodTex() {
			Load(null);
			Load(vlcVideoSourceProvider.VideoSource);
		}

		public bool StopLoad = false;//Stops a asset loading over each other

		private async Task LoadMedia() {
			StopLoad = true; 
			if (_mediaPlayer == null) {
				return;
			}
			if (url.Value == null) {
				return;
			}
			var uri = new Uri(url);
			if ((useCache || uri.Scheme.ToLower() == "local") && !VideoImporter.IsVideoStreaming(uri)) {
				Log.Info("Loading static video");
				CurrentTask?.Stop();
				CurrentTask = World.assetSession.AssetLoadingTask((data) => {
					if (data != null) {
						var media = new Media(_libVLC, Engine.assetManager.GetAssetFile(uri));
						while (media.State == VLCState.Buffering) {
							Thread.Sleep(10);
						}
						_mediaPlayer.Play(media);
					}
					else {
						Log.Err("Failed to load assets");
					}
				}, uri, true);
					
			}
			else {
				Log.Info("Loading stream video");
				StopLoad = false;
				var media = new Media(_libVLC, uri);
				if (VideoImporter.IsVideoStreaming(uri)) {
					await media.Parse(MediaParseOptions.ParseNetwork);
				}
				while (media.State == VLCState.Buffering) {
					Thread.Sleep(10);
				}
				if (StopLoad) {
					return;
				}
				_mediaPlayer.Play(media);
			}
		}

		public override void StartLoadAsset() {
			if (url.Value == null) {
				return;
			}
			try {
				Log.Info("Starting load of static Asset Video");
				LoadMedia().ConfigureAwait(false);
			}
			catch (Exception e) {
				Log.Info($"Error Loading static Asset {e}");
			}
		}
	}
}
