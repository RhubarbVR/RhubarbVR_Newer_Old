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
				//			Witness
				//				267102006613639180 (humbletim)
				//			you:
				//				F32L!
				//			vlc:
				//				S16N.
				//			you:
				//				F32L!!!!!!
				//			vlc:
				//				S16N.
				//			you:
				//				S16N(FL32).
				//			vlc:
				//				correct.
				if (audio is null) {
					return;
				};
				var newbuff = new float[count];
				for (var i = 0; i < count; i++) {
					newbuff[i] = (Marshal.ReadInt16(samples + (sizeof(short) * i)) * (1 / 32768.0f));
				}
				audio.WriteSamples(newbuff);
			}


			public float[] GetAudio(short[] data) {
				return (from samp in data.AsParallel()
					   select (float)samp * (1 / 32768.0f)).ToArray();
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
		public static IntPtr formatData;
		static unsafe int AudioSetup(ref IntPtr opaque, ref IntPtr format, ref uint rate, ref uint channels) {
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
				_mediaPlayer.SetAudioFormatCallback(AudioSetup, null);
				_mediaPlayer.SetAudioCallbacks(audioPlayer.PlayAudio, null, null,null, null);
				_mediaPlayer.SetVideoFormatCallbacks(LibVLCVideoFormatCb, null);
				_mediaPlayer.SetVideoCallbacks(LibVLCVideoLockCb, LibVLCVideoUnlockCb, null);
				_mediaPlayer.Play(media.SubItems[0]);
			}
		}

		private enum Chroma
		{
			Uknown,
			I420,
			I411,
			I422,
			YUYV,
			UYVY,
			RV24,
		    RV32,
			I42N,
			I41N,
			GRAW,
		}

		private Chroma _loadedChroma = Chroma.Uknown;

		private uint _pitches;

		private uint _lines;
		uint LibVLCVideoFormatCb(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines) {
			Log.Info($"Format:{Marshal.PtrToStringAnsi(chroma)} width{width} height{height} pitches{pitches}  lines{lines}");
			_pitches = pitches;
			_lines = lines;
			_loadedChroma = Marshal.PtrToStringAnsi(chroma) switch {
				"I420" => Chroma.I420,
				"I411" => Chroma.I411,
				"I422" => Chroma.I422,
				"YUYV" => Chroma.YUYV,
				"UYVY" => Chroma.UYVY,
				"RV24" => Chroma.RV24,
				"RV32" => Chroma.RV32,
				"I42N" => Chroma.I42N,
				"I41N" => Chroma.I41N,
				"GRAW" => Chroma.GRAW,
				_ => Chroma.Uknown,
			};
			var format = TexFormat.Rgba32;
			if (_loadedChroma == Chroma.Uknown) {
				return 1;
			}
			var e = new Tex(TexType.ImageNomips, format);
			e.SetSize((int)width, (int)height);
			Load(e);
			return 1;
		}

		private void RenderI420(IntPtr data) {

		}

		void LibVLCVideoUnlockCb(IntPtr opaque, IntPtr picture, IntPtr planes) {
			if(Value is null) {
				return;
			}
			switch (_loadedChroma) {
				case Chroma.Uknown:
					break;
				case Chroma.I420:
					RenderI420(planes);
					break;
				case Chroma.I411:
					break;
				case Chroma.I422:
					break;
				case Chroma.YUYV:
					break;
				case Chroma.UYVY:
					break;
				case Chroma.RV24:
					break;
				case Chroma.RV32:
					break;
				case Chroma.I42N:
					break;
				case Chroma.I41N:
					break;
				case Chroma.GRAW:
					break;
				default:
					break;
			}
			//need to load colors
			//Value.SetColors(Value.Width, Value.Height, planes);
		}

		IntPtr LibVLCVideoLockCb(IntPtr opaque, IntPtr planes) {
			return IntPtr.Zero;
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
