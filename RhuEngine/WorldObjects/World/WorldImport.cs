using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using StereoKit;
using RhuEngine.Components;
using System.IO;

namespace RhuEngine.WorldObjects
{
	public partial class World : IWorldObject
	{

		public enum AssetType
		{
			Unknown,
			Texture,
			Video
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

		public AssetType GetAssetTypeFromPath(string path) {
			path = path.ToLower();
			if (path.EndsWith(".mp4")) {
				return AssetType.Video;
			}
			if (path.EndsWith(".mlv")) {
				return AssetType.Video;
			}
			if (path.EndsWith(".png")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".jpeg")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".jpg")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".bmp")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".pdm")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".gif")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".tiff")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".tga")) {
				return AssetType.Texture;
			}
			if (path.EndsWith(".webp")) {
				return AssetType.Texture;
			}
			return path.EndsWith(".bmp") ? AssetType.Texture : AssetType.Unknown;
		}

		public AssetType GetAssetTypeOfString(ref string data,out bool WasUri) {
			if (data == null) {
				WasUri = false;
				return AssetType.Unknown;
			}
			if (File.Exists(data)) {
				WasUri = false;
				return GetAssetTypeFromPath(data);
			}
			if (Uri.TryCreate(data, UriKind.Absolute, out var uri)) {
				WasUri = true;
				return IsVideoStreaming(uri) ? AssetType.Video : GetAssetTypeFromPath(uri.AbsolutePath);
			}
			WasUri = false;
			return AssetType.Unknown;
		} 

		private void BuildTextureString(Entity target,string data, byte[] rawdata,bool wasUri) {
			Log.Info($"Loaded Texture Data {data} Uri{wasUri}");
			if (wasUri) {
				target.position.Value = new Vec3(0, 0.25f, -0.5f);
				target.rotation.Value = Quat.FromAngles(90, 0, 0);
				target.scale.Value = new Vec3(0.33f);
				var user = GetLocalUser()?.userRoot.Target?.Entity;
				if (user != null) {
					target.position.Value += user.position.Value;
				}
				var (pmesh, mit, prender) = target.AttachMeshWithMeshRender<PlaneMesh, UnlitShader>();
				var scaler = target.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.dimensions);
				var textur = target.AttachComponent<StaticTexture>();
				scaler.texture.Target = textur;
				textur.url.Value = data;
				mit.faceCull.Value = Cull.None;
				mit.SetPram("diffuse", textur);
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = LoadLocalAsset(File.ReadAllBytes(data),data);
						BuildTextureString(target, newuri.ToString(), null, true);
					}
					else {
						Log.Err("Texture Load Uknown" + data);
					}
				} else {
					var newuri = LoadLocalAsset(rawdata, data);
					BuildTextureString(target, newuri.ToString(), null, true);
				}
			}
		}

		private void BuildVideoString(Entity target, string data,byte[] rawdata, bool wasUri) {
			if (wasUri) {
				target.position.Value = new Vec3(0, 0.25f, -0.5f);
				target.rotation.Value = Quat.FromAngles(90, 0, 0);
				target.scale.Value = new Vec3(0.33f);
				var user = GetLocalUser()?.userRoot.Target?.Entity;
				if (user != null) {
					target.position.Value += user.position.Value;
				}
				Log.Info("Build video");
				var (pmesh, mit, prender) = target.AttachMeshWithMeshRender<PlaneMesh, UnlitShader>();
				var scaler = target.AttachComponent<TextureScaler>();
				scaler.scale.SetLinkerTarget(pmesh.dimensions);
				var textur = target.AttachComponent<VideoPlayer>();
				var soundSource = target.AttachComponent<SoundSource>();
				soundSource.sound.Target = textur.audioPlayer;
				scaler.texture.Target = textur;
				textur.Url.Value = data;
				mit.faceCull.Value = Cull.None;
				mit.SetPram("diffuse", textur);
			}
			else {
				if (rawdata == null) {
					if (File.Exists(data)) {
						var newuri = LoadLocalAsset(File.ReadAllBytes(data), data);
						BuildVideoString(target, newuri.ToString(), null, true);
					}
					else {
						Log.Err("Video Load Uknown" + data);
					}
				}
				else {
					var newuri = LoadLocalAsset(rawdata, data);
					BuildVideoString(target, newuri.ToString(), null, true);
				}
			}
		}

		public void ImportString(string data) {
			if (string.IsNullOrEmpty(data)) {
				Log.Err("Import string was empty");
				return;
			}
			var spawnroot = GetLocalUser()?.userRoot.Target?.Entity?.parent.Target??RootEntity;
			var assetEntity = spawnroot.AddChild("Imported Asset");
			switch (GetAssetTypeOfString(ref data, out var wasUri)) {
				case AssetType.Unknown:
					Log.Err($"Do not know what to do with {data}");
					break;
				case AssetType.Texture:
					BuildTextureString(assetEntity,data,null,wasUri);
					break;
				case AssetType.Video:
					BuildVideoString(RootEntity,data,null,wasUri);
					break;
				default:
					break;
			}
		} 
	}
}
