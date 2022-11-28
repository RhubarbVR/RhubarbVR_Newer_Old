using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient;

using RhuEngine.Commads;
using RhuEngine.Components.PrivateSpace.Programs.OverlayDialogues;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RNumerics;

using TextCopy;

namespace RhuEngine.Components
{
	public static class ImportStatics
	{
		public enum FileTypes
		{
			Unknown = Image | Mesh | Video | Audio | Text,
			None = 0,
			Image = 1,
			Mesh = 2,
			Video = 4,
			Audio = 8,
			Text = 16,
		}

		public static FileTypes GetFileTypes(string fileName, string mimeType) {
			if (string.IsNullOrEmpty(fileName) && string.IsNullOrEmpty(mimeType)) {
				return FileTypes.Unknown;
			}
			var fileTypes = FileTypes.None;
			if (IsTextImport(fileName, mimeType)) {
				fileTypes |= FileTypes.Text;
			}
			if (IsAudioImport(fileName, mimeType)) {
				fileTypes |= FileTypes.Audio;
			}
			if (IsVideoImport(fileName, mimeType)) {
				fileTypes |= FileTypes.Video;
			}
			if (IsMeshImport(fileName, mimeType)) {
				fileTypes |= FileTypes.Mesh;
			}
			if (IsTextureImport(fileName, mimeType)) {
				fileTypes |= FileTypes.Image;
			}
			return fileTypes == FileTypes.None ? FileTypes.Unknown : fileTypes;
		}

		public static bool ChechMimeType(string mimeType, string target) {
			return "application/octet-stream" == mimeType || (mimeType?.Contains(target) ?? false);
		}

		public static bool IsTextImport(string fileName, string mimeType) {
			return ChechMimeType(mimeType, "text") || ChechMimeType(mimeType, "json") || ChechMimeType(mimeType, "xml");
		}
		public static bool IsAudioImport(string fileName, string mimeType) {
			return ChechMimeType(mimeType, "audio");
		}


		public static bool IsVideoImport(string fileName, string mimeType) {
			return ChechMimeType(mimeType, "video") || ChechMimeType(mimeType, "audio") || IsValidVideoImport(fileName);
		}

		public static bool IsMeshImport(string fileName, string mimeType) {
			return ChechMimeType(mimeType, "model") || IsValidMeshImport(fileName);
		}

		public static bool IsTextureImport(string fileName, string mimeType) {
			return ChechMimeType(mimeType, "image") || IsValidTextureImport(fileName);
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

		public static bool IsValidVideoImport(string path) {
			if(string.IsNullOrEmpty(path)) {
				return false;
			}
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

		public static bool IsValidTextureImport(string path) {
			if (string.IsNullOrEmpty(path)) {
				return false;
			}
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

		public static bool IsValidMeshImport(string path) {
			if (string.IsNullOrEmpty(path)) {
				return false;
			}
			path = path.ToLower();
			return
				path.EndsWith(".fbx") ||
				path.EndsWith(".dea") ||
				path.EndsWith(".gltf") || path.EndsWith(".glb") ||
				path.EndsWith(".blend") ||
				path.EndsWith(".3ds") ||
				path.EndsWith(".ase") ||
				path.EndsWith(".obj") ||
				path.EndsWith(".ifc") ||
				path.EndsWith(".xgl") || path.EndsWith(".zgl") ||
				path.EndsWith(".ply") ||
				path.EndsWith(".dxf") ||
				path.EndsWith(".lwo") ||
				path.EndsWith(".lws") ||
				path.EndsWith(".lxo") ||
				path.EndsWith(".stl") ||
				path.EndsWith(".x") ||
				path.EndsWith(".ac") ||
				path.EndsWith(".ms3d") ||
				path.EndsWith(".cob") || path.EndsWith(".scn") ||
				path.EndsWith(".bvh") ||
				path.EndsWith(".csm") ||
				path.EndsWith(".mdl") ||
				path.EndsWith(".md2") ||
				path.EndsWith(".md3") ||
				path.EndsWith(".pk3") ||
				path.EndsWith(".mdc") ||
				path.EndsWith(".md5") ||
				path.EndsWith(".smd") || path.EndsWith(".vta") ||
				path.EndsWith(".ogex") ||
				path.EndsWith(".b3d") ||
				path.EndsWith(".q3d") ||
				path.EndsWith(".q3s") ||
				path.EndsWith(".nff") ||
				path.EndsWith(".off") ||
				path.EndsWith(".raw") ||
				path.EndsWith(".ter") ||
				path.EndsWith(".hmp") ||
				path.EndsWith(".ndo");
		}


	}
}
