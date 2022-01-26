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
			if (path.EndsWith(".mp4")) {
				return AssetType.Video;
			}
			if (path.EndsWith(".mlv")) {
				return AssetType.Video;
			}
			return AssetType.Texture;
		}

		public AssetType GetAssetTypeOfString(string data,out bool WasUri) {
			if (data == null) {
				WasUri = false;
				return AssetType.Unknown;
			}
			if (Uri.TryCreate(data, UriKind.Absolute, out var uri)) {
				WasUri = true;
				return IsVideoStreaming(uri) ? AssetType.Video : GetAssetTypeFromPath(uri.AbsolutePath);
			}
			else {
				WasUri = false;
			}	
			return AssetType.Unknown;
		} 

		private void BuildTextureString(Entity target,string data,bool wasUri) {
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
				Log.Err("Not support raw textures");
			}
		}

		private void BuildVideoString(Entity target, string data, bool wasUri) {
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
				scaler.texture.Target = textur;
				textur.Url.Value = data;
				mit.faceCull.Value = Cull.None;
				mit.SetPram("diffuse", textur);
			}
			else {
				Log.Err("Not support raw Video");
			}
		}

		public void ImportString(string data) {
			var spawnroot = GetLocalUser()?.userRoot.Target?.Entity?.parent.Target??RootEntity;
			var assetEntity = spawnroot.AddChild("Imported Asset");
			switch (GetAssetTypeOfString(data, out var wasUri)) {
				case AssetType.Unknown:
					Log.Err($"Do not know what to do with {data}");
					break;
				case AssetType.Texture:
					BuildTextureString(assetEntity,data,wasUri);
					break;
				case AssetType.Video:
					BuildVideoString(RootEntity,data,wasUri);
					break;
				default:
					break;
			}
		} 
	}
}
