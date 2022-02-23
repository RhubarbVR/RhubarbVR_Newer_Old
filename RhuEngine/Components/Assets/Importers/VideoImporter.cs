using System;
using System.IO;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Importers" })]
	public class VideoImporter : Importer
	{
		public static bool IsValidImport(string path) {
			path = path.ToLower();
			return
				path.EndsWith(".png") ||
				path.EndsWith(".jpeg") ||
				path.EndsWith(".jpg") ||
				path.EndsWith(".bmp") ||
				path.EndsWith(".pdm") ||
				path.EndsWith(".gif") ||
				path.EndsWith(".tiff") ||
				path.EndsWith(".tga") ||
				path.EndsWith(".webp");
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

		public override void Import(string data, bool wasUri, byte[] rawdata) {
			if (wasUri) {
				Log.Info("Build video");
				var (pmesh, mit, prender) = Entity.AttachMeshWithMeshRender<PlaneMesh, UnlitShader>();
				var scaler = Entity.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.dimensions);
				scaler.scaleMultiplier.Value = 0.1f;
				var textur = Entity.AttachComponent<VideoPlayer>();
				var soundSource = Entity.AttachComponent<SoundSource>();
				soundSource.sound.Target = textur.audio;
				scaler.texture.Target = textur;
				textur.Url.Value = data;
				mit.faceCull.Value = Cull.None;
				mit.SetPram("diffuse", textur);
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = World.LoadLocalAsset(File.ReadAllBytes(data), data);
						Import(newuri.ToString(), true,null);
					}
					else {
						Log.Err("Video Load Uknown" + data);
					}
				}
				else {
					var newuri = World.LoadLocalAsset(rawdata, data);
					Import(newuri.ToString(), true,null);
				}
			}
		}
	}
}
