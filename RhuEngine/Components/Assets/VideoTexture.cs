using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using LibVLCSharp.Shared;

using RhuEngine.VLC;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using NAudio.Wave;
using NYoutubeDL;
using NYoutubeDL.Models;
using System.Linq;
using System.Runtime.InteropServices;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets" })]
	public sealed class VideoTexture : StaticAsset<RTexture2D>, IAssetProvider<IWaveProvider>
	{
		public VlcVideoSourceProvider vlcVideoSourceProvider = null;

		[Default(1)]
		[OnChanged(nameof(UpdateVideo))]
		public readonly Sync<uint> ChannelCount;

		[OnChanged(nameof(PlayBackUpdate))]
		public readonly SyncPlayback Playback;


		LibVLC _libVLC;
		MediaPlayer _mediaPlayer;

		private void PlayBackUpdate() {
			if (_mediaPlayer is not null) {
				_mediaPlayer.SetAudioDelay(-50000);
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


		public void UpdateVideo() {
			Task.Run(() => LoadVideoPlayer());
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
				lock (this) {
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
						ChannelCount = ChannelCount.Value > 0 ? ChannelCount.Value : 1
					};
					Core.Initialize();
					_libVLC = new LibVLC(enableDebugLogs: false);
					_mediaPlayer = new MediaPlayer(_libVLC);
					_mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
					_mediaPlayer.Buffering += MediaPlayer_Buffering;
					_mediaPlayer.EndReached += MediaPlayer_EndReached;
					vlcVideoSourceProvider.LoadPlayer(_mediaPlayer);
					vlcVideoSourceProvider.RelaodTex += VlcVideoSourceProvider_RelaodTex;
					Task.Run(LoadMedia);
				}
			}
			catch (Exception ex) {
				RLog.Err($"Failed to start Video Player Error:{ex}");
			}
		}

		private void MediaPlayer_EndReached(object sender, EventArgs e) {

		}

		private void MediaPlayer_Buffering(object sender, MediaPlayerBufferingEventArgs e) {
			if (_mediaPlayer is not null) {
				return;
			}
			_mediaPlayer.Position = (float)Playback.Position;
		}

		private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e) {
			Playback.ClipLength = (double)e.Length / 1000;
		}

		private void VlcVideoSourceProvider_RelaodTex() {
			Load(null);
			Load(vlcVideoSourceProvider.VideoSource);
			LoadAudio(vlcVideoSourceProvider.Audio);
		}

		public bool StopLoad = false;//Stops a asset loading over each other

		private YoutubeDL _youtubeDL;

		private static int RateAudioCodec(string codec) {
			if (string.IsNullOrEmpty(codec)) {
				return int.MinValue;
			}
			codec = codec.ToLower();
			return codec.Contains("none") ? int.MinValue : codec.Contains("mp4a") ? 200 : 100;
		}

		private static int RateVideoCodec(string codec) {
			if (string.IsNullOrEmpty(codec)) {
				return int.MinValue;
			}
			codec = codec.ToLower();
			return codec.Contains("none") ? int.MinValue : codec.Contains("avc") ? 100 : codec.Contains("h264") ? 100 : 100;
		}

		private async Task LoadMedia() {
			try {
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
				if (!ImportStatics.IsVideoStreaming(uri)) {
					RLog.Info("Loading static video");
					var media = new Media(_libVLC, Engine.assetManager.GetCachedPath(uri));
					while (media.State == VLCState.Buffering) {
						Thread.Sleep(10);
					}
					VlcVideoSourceProvider_RelaodTex();
					_mediaPlayer?.Play(media);
					PlayBackUpdate();
				}
				else {
					RLog.Info("Loading stream video");
					StopLoad = false;
					Media media;
					if (ImportStatics.IsVideoStreaming(uri)) {
						RLog.Info($"Loadeding youtubeDL");
						if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
							RLog.Info($"YoutubeDL Loading Windows");
							_youtubeDL ??= new YoutubeDL("./YT-DLP/Windows/yt-dlp-win.exe");
						}
						if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
							RLog.Info($"YoutubeDL Loading Linux");
							_youtubeDL ??= new YoutubeDL("./YT-DLP/Linux/yt-dlp_linux");
						}
						if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
							RLog.Info($"YoutubeDL Loading OSX");
							_youtubeDL ??= new YoutubeDL("./YT-DLP/MacOS/yt-dlp_macos");
						}
						if (_youtubeDL is null) {
							RLog.Info($"YoutubeDL is not loaded");
							return;
						}
						// Would like to add playlist support could not get it working every playlest video was null
						_youtubeDL.Options.VideoSelectionOptions.NoPlaylist = true;
						_youtubeDL.VideoUrl = uri.ToString();
						var data = await _youtubeDL.GetDownloadInfoAsync();
						VideoDownloadInfo videoDownloadInfo = null;
						if (data is VideoDownloadInfo _videoDownloadInfo) {
							videoDownloadInfo = _videoDownloadInfo;
						}
						if (data is PlaylistDownloadInfo playlistDownloadInfo) {
							videoDownloadInfo = playlistDownloadInfo.CurrentVideo;
						}
						if (videoDownloadInfo is null) {
							RLog.Info($"Loaded youtubedl failed no video");
							return;
						}
						// This is a bad thing
						FormatDownloadInfo formatDownloadInfo = null;
						var lastRateing = 0L;
						foreach (var item in videoDownloadInfo.Formats) {
							var audioCodec = item.Acodec;
							var videoCodec = item.Vcodec;
							var currentRating = 0L;
							currentRating += RateVideoCodec(videoCodec);
							currentRating += RateAudioCodec(audioCodec);
							currentRating += (item?.Width ?? int.MinValue) / 1000;
							if (currentRating >= lastRateing | formatDownloadInfo is null) {
								formatDownloadInfo = item;
								lastRateing = currentRating;
							}
							RLog.Info($"Format {item.Resolution} {item.Acodec} {item.Vcodec} rating was {currentRating}");
						}
						if (formatDownloadInfo is null) {
							RLog.Info($"Loaded youtubedl failed");
							return;
						}
						RLog.Info($"Best Format was {formatDownloadInfo.Resolution} {formatDownloadInfo.Acodec} {formatDownloadInfo.Vcodec}");

						RLog.Info($"Loaded youtubedl {formatDownloadInfo.Url}");
						media = new Media(_libVLC, new Uri(formatDownloadInfo.Url));
					}
					else {
						media = new Media(_libVLC, uri);
					}
					if (StopLoad) {
						return;
					}
					VlcVideoSourceProvider_RelaodTex();
					_mediaPlayer.Play(media);
					PlayBackUpdate();
				}
			}
			catch (Exception e) {
				RLog.Err($"Video Error {e}");
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

		private Action<IWaveProvider> _audioaction;

		event Action<IWaveProvider> IAssetProvider<IWaveProvider>.OnAssetLoaded
		{
			add => _audioaction += value;
			remove => _audioaction -= value;
		}

		bool _audioLoaded;
		IWaveProvider _waveProvider;
		bool IAssetProvider<IWaveProvider>.Loaded => _audioLoaded;


		IWaveProvider IAssetProvider<IWaveProvider>.Value => _waveProvider;
		public void LoadAudio(IWaveProvider data) {
			_waveProvider = data;
			_audioLoaded = data != null;
			_audioaction?.Invoke(data);
		}

		public override void LoadAsset(byte[] data) {
			StartLoadAsset();
		}
	}
}
