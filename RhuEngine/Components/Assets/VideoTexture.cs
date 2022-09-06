using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using LibVLCSharp.Shared;

using RhuEngine.VLC;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public class VideoTexture : StaticAsset<RTexture2D>
	{
		public VlcVideoSourceProvider vlcVideoSourceProvider = null;

		[OnChanged(nameof(UpdateVideo))]
		public readonly SyncObjList<AudioOutput> AudioChannels;
		[OnChanged(nameof(PlayBackUpdate))]
		public readonly SyncPlayback Playback;

		LibVLC _libVLC;
		MediaPlayer _mediaPlayer;

		private void PlayBackUpdate() {
			if(_mediaPlayer is not null) {
				if (Playback.Playing) {
					_mediaPlayer.Play();
				}
				else {
					_mediaPlayer.Pause();
					_mediaPlayer.NextFrame();
				}
				_mediaPlayer.SetRate(Playback.Value.Speed);
				_mediaPlayer.Position = (float)(1 / Playback.ClipLength * Playback.Position);
			}
		}

		protected override void OnAttach() {
			base.OnAttach();
			AudioChannels.Add();
		}

		public void UpdateVideo() {
			LoadVideoPlayer();
		}

		protected override void OnLoaded() {
			Playback.StateChange += Playback_StateChange;
			Load(null);
			UpdateVideo();
			base.OnLoaded();
		}

		private double Playback_StateChange() {
			return (double)(_mediaPlayer?.Length ?? -1L) / 1000;
		}

		private void LoadVideoPlayer() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			try {
				Load(null);
				if (vlcVideoSourceProvider != null) {
					RLog.Info("Reloading Loading Video Player");
					vlcVideoSourceProvider.Dispose();
					vlcVideoSourceProvider = null;
					_libVLC = null;
					_mediaPlayer = null;
				}
				else {
					RLog.Info("Loading Video Player");
				}
				vlcVideoSourceProvider = new VlcVideoSourceProvider {
					ChannelCount = (uint)AudioChannels.Count
				};
				Core.Initialize();
				_libVLC = new LibVLC(enableDebugLogs: false);
				_mediaPlayer = new MediaPlayer(_libVLC);
				_mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
				_mediaPlayer.Buffering += MediaPlayer_Buffering;
				vlcVideoSourceProvider.LoadPlayer(_mediaPlayer);
				vlcVideoSourceProvider.RelaodTex += VlcVideoSourceProvider_RelaodTex;
				vlcVideoSourceProvider.LoadAudio += (samps, chan) => {
					if (!Engine.EngineLink.CanAudio) {
						return;
					}
					if (AudioChannels.Count <= chan) {
						return;
					}
					AudioChannels[chan].WriteAudio(samps);
				};
				Task.Run(LoadMedia);
			}
			catch (Exception ex) 
			{
				RLog.Err($"Failed to start Video Player Error:{ex}");
			}
		}

		private void MediaPlayer_Buffering(object sender, MediaPlayerBufferingEventArgs e) {
			if(_mediaPlayer is not null) {
				return;
			}
			_mediaPlayer.Position = (float)Playback.Position;
		}

		private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e) {
			Playback.ClipLength = (double)e.Length /1000;
		}

		private void VlcVideoSourceProvider_RelaodTex() {
			Load(null);
			Load(vlcVideoSourceProvider.VideoSource);
		}

		public bool StopLoad = false;//Stops a asset loading over each other

		private async Task LoadMedia() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}
			if (_mediaPlayer == null) {
				return;
			}
			if (url.Value == null) {
				return;
			}
			StopLoad = true;
			Load(null);
			var uri = new Uri(url);
			if ((useCache || uri.Scheme.ToLower() == "local") && !VideoImporter.IsVideoStreaming(uri)) {
				RLog.Info("Loading static video");
				var lastTask = CurrentTask;
				CurrentTask = World.assetSession.AssetLoadingTask((data) => {
					lastTask?.Stop();
					if (data != null) {
						var media = new Media(_libVLC, Engine.assetManager.GetAssetFile(uri));
						while (media.State == VLCState.Buffering) {
							Thread.Sleep(10);
						}
						VlcVideoSourceProvider_RelaodTex();
						_mediaPlayer?.Play(media);
						PlayBackUpdate();
					}
					else {
						RLog.Err("Failed to load assets");
					}
				}, uri, true);
					
			}
			else {
				RLog.Info("Loading stream video");
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
				VlcVideoSourceProvider_RelaodTex();
				_mediaPlayer.Play(media);
				PlayBackUpdate();
			}
		}

		public override void StartLoadAsset() {
			if (url.Value == null) {
				return;
			}
			try {
				RLog.Info("Starting load of static Asset Video");
				Task.Run(LoadMedia);
			}
			catch (Exception e) {
				RLog.Info($"Error Loading static Asset {e}");
			}
		}
	}
}
