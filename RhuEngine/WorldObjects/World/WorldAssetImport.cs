using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using RhuEngine.Components;
using System.IO;
using RNumerics;
using RhuEngine.Linker;

namespace RhuEngine.WorldObjects
{
	public partial class World : IWorldObject {
	
		public enum AssetType {
			Unknown,
			Texture,
			Video,
			Model,
		}

		public static AssetType GetAssetTypeFromPath(string path) {
			if (TextureImporter.IsValidImport(path)) {
				return AssetType.Texture;
			}
			if (VideoImporter.IsValidImport(path)) {
				return AssetType.Video;
			}
			if (AssimpImporter.IsValidImport(path)) {
				return AssetType.Model;
			}
			return AssetType.Unknown;
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
				return VideoImporter.IsVideoStreaming(uri) ? AssetType.Video : GetAssetTypeFromPath(uri.AbsolutePath);
			}
			WasUri = false;
			return AssetType.Unknown;
		}

		public void ImportString(string data) {
			if (string.IsNullOrEmpty(data)) {
				RLog.Err("Import string was empty");
				return;
			}
			var spawnroot = GetLocalUser()?.userRoot.Target?.Entity?.parent.Target??RootEntity;
			var assetEntity = spawnroot.AddChild("Asset Importer");
			if(GetLocalUser() is not null) {
				assetEntity.GlobalTrans = Matrix.TR(Vector3f.Forward * 0.35f, Quaternionf.Pitched) * GetLocalUser().userRoot.Target?.head.Target?.GlobalTrans ?? Matrix.Identity;
			}
			switch (GetAssetTypeOfString(ref data, out var wasUri)) {
				case AssetType.Unknown:
					assetEntity.AttachComponent<UnknownImporter>().Import(data, wasUri, null);
					break;
				case AssetType.Texture:
					assetEntity.AttachComponent<TextureImporter>().Import(data, wasUri,null);
					break;
				case AssetType.Video:
					assetEntity.AttachComponent<VideoImporter>().Import(data, wasUri, null);
					break;
				case AssetType.Model:
					assetEntity.AttachComponent<AssimpImporter>().Import(data, wasUri, null);
					break;
				default:
					break;
			}
		} 
	}
}
