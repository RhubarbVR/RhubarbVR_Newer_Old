using System;
using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using RNumerics;
using System.Threading.Tasks;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public sealed class VideoImporter : Importer
	{
		public static bool IsValidImport(string path) {
			path = path.ToLower();
			return
				path.EndsWith(".asx") ||
				path.EndsWith(".dts") ||
				path.EndsWith(".gxf") ||
				path.EndsWith(".m2v") ||
				path.EndsWith(".m3u") ||
				path.EndsWith(".m4v") ||
				path.EndsWith(".mpeg1") ||
				path.EndsWith(".mpeg2") ||
				path.EndsWith(".mts") ||
				path.EndsWith(".mxf") ||
				path.EndsWith(".ogm") ||
				path.EndsWith(".pls") ||
				path.EndsWith(".bup") ||
				path.EndsWith(".a52") ||
				path.EndsWith(".aac") ||
				path.EndsWith(".b4s") ||
				path.EndsWith(".cue") ||
				path.EndsWith(".divx") ||
				path.EndsWith(".dv") ||
				path.EndsWith(".flv") ||
				path.EndsWith(".m1v") ||
				path.EndsWith(".m2ts") ||
				path.EndsWith(".mkv") ||
				path.EndsWith(".mov") ||
				path.EndsWith(".mpeg4") ||
				path.EndsWith(".oma") ||
				path.EndsWith(".spx") ||
				path.EndsWith(".ts") ||
				path.EndsWith(".vlc") ||
				path.EndsWith(".vob") ||
				path.EndsWith(".xspf") ||
				path.EndsWith(".dat") ||
				path.EndsWith(".bin") ||
				path.EndsWith(".ifo") ||
				path.EndsWith(".part") ||
				path.EndsWith(".avi") ||
				path.EndsWith(".mpeg") ||
				path.EndsWith(".mpg") ||
				path.EndsWith(".flac") ||
				path.EndsWith(".m4a") ||
				path.EndsWith(".mp1") ||
				path.EndsWith(".ogg") ||
				path.EndsWith(".wav") ||
				path.EndsWith(".xm") ||
				path.EndsWith(".3gp") ||
				path.EndsWith(".srt") ||
				path.EndsWith(".wmv") ||
				path.EndsWith(".ac3") ||
				path.EndsWith(".asf") ||
				path.EndsWith(".mod") ||
				path.EndsWith(".mp2") ||
				path.EndsWith(".mp3") ||
				path.EndsWith(".mp4") ||
				path.EndsWith(".wma") ||
				path.EndsWith(".mka") ||
				path.EndsWith(".m4p") ||
				path.EndsWith(".3g2");
		}
		public static bool IsStreamingProtocol(string scheme) {
			return scheme.ToLower() switch {
				"rtp" or "mms" or "rtsp" or "rtmp" => true,
				_ => false,
			};
		}
		public static bool IsVideoStreaming(Uri url) {
			return IsStreamingProtocol(url.Scheme) || url.Host.Contains("youtube.") || url.Host.Contains("youtu.be")
|| url.Host.Contains("vimeo.") || url.Host.Contains("twitch.tv") || url.Host.Contains("twitter.") || url.Host.Contains("soundcloud.")
|| url.Host.Contains("reddit.") || url.Host.Contains("dropbox.") || url.Host.Contains("mixer.") || url.Host.Contains("dailymotion.")
|| url.Host.Contains("streamable.") || url.Host.Contains("drive.google.") || url.Host.Contains("tiktok.") || url.Host.Contains("niconico.")
|| url.Host.Contains("nicovideo.") || url.Host.Contains("lbry.tv") || url.Host.Contains("nicovideo.jp");
		}

		public void ImportAsync(string data, bool wasUri, byte[] rawdata) {
			if (wasUri) {
				RLog.Info("Building video");
				Entity.AttachComponent<Grabbable>();
				Entity.AttachComponent<BoxShape>();
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<RectangleMesh, UnlitMaterial>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.Dimensions);
				scaler.scaleMultiplier.Value = 0.5f;
				var textur = Entity.AttachComponent<VideoTexture>();
				Entity.AttachComponent<SoundSource>().sound.Target = textur;
				textur.url.Value = data;
				scaler.texture.Target = textur;
				mit.MainTexture.Target = textur;
				Destroy();
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = World.CreateLocalAsset(File.ReadAllBytes(data));
						Import(newuri.ToString(), true, null);
					}
					else {
						RLog.Err("Video Load Uknown" + data);
					}
				}
				else {
					var newuri = World.CreateLocalAsset(rawdata);
					Import(newuri.ToString(), true, null);
				}
			}
		}

		public override void Import(string data, bool wasUri, byte[] rawdata) {
			Task.Run(() => ImportAsync(data, wasUri, rawdata));
		}
	}
}
