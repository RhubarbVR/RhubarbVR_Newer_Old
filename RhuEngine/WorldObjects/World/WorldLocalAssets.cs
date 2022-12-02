﻿using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using LiteNetLib;
using System.Threading.Tasks;
using SharedModels;
using System.Collections.Generic;
using SharedModels.GameSpecific;
using RhuEngine.Linker;
using RNumerics;
using RhuEngine.AssetSystem;
using RhuEngine.AssetSystem.RequestStructs;
using RhuEngine.DataStructure;
using System.IO;
using RhubarbCloudClient.Model;

namespace RhuEngine.WorldObjects
{
	public sealed partial class World
	{
		public const DeliveryMethod ASSET_DELIVERY_METHOD = DeliveryMethod.ReliableUnordered;

		public async Task<Uri> CreateLocalAsset(Stream data, string mimeType) {
			var newID = Guid.NewGuid();
			await Engine.assetManager.SaveNew(SessionID.Value, LocalUserID, newID, data);
			var uri = new Uri($"local://{SessionID.Value}-{LocalUserID}-{newID}");
			AssetMimeType.Add(uri, mimeType);
			return uri;
		}

		public Uri CreateLocalAsset(byte[] data, string mimeType) {
			var newID = Guid.NewGuid();
			Engine.assetManager.SaveNew(SessionID.Value, LocalUserID, newID, data);
			var uri = new Uri($"local://{SessionID.Value}-{LocalUserID}-{newID}");
			AssetMimeType.Add(uri, mimeType);
			return uri;
		}
		public Uri CreateLocalAsset(RTexture2D newtexture) {
			var newID = Guid.NewGuid();
			Engine.assetManager.SaveNew(SessionID.Value, LocalUserID, newID, newtexture.Image.SaveWebp(false, 1f));
			var uri = new Uri($"local://{SessionID.Value}-{LocalUserID}-{newID}");
			AssetMimeType.Add(uri, "image/webp");
			return uri;
		}

		public Uri CreateLocalAsset(ComplexMesh amesh) {
			var newID = Guid.NewGuid();
			Engine.assetManager.SaveNew(SessionID.Value, LocalUserID, newID, RhubarbFileManager.SaveFile(Engine.netApiManager.Client.User?.Id??Guid.Empty, amesh));
			var uri = new Uri($"local://{SessionID.Value}-{LocalUserID}-{newID}");
			AssetMimeType.Add(uri, "application/rhubarbvr_mesh");
			return uri;
		}
		public readonly HashSet<Uri> WaitingOnAssets = new();
		public void RequestAsset(Uri target) {
			WaitingOnAssets.Add(target);
			_netManager.SendToAll(Serializer.Save<IAssetRequest>(new RequestAsset { URL = target.ToString() }), 2, ASSET_DELIVERY_METHOD);
		}

		public async Task PremoteAssetAsync(Uri target, Guid? arreay) {
			if (!Engine.netApiManager.Client.IsLogin) {
				return;
			}
			var data = Engine.assetManager.GetCached(target);
			if (data is null || !AssetMimeType.TryGetValue(target, out var dataMime)) {
				return;
			}
			var returndata = await (arreay is null
				? Engine.netApiManager.Client.UploadRecord(new MemoryStream(data), dataMime)
				: Engine.netApiManager.Client.UploadRecordGroup(arreay ?? Guid.Empty, new MemoryStream(data), dataMime));
			if (returndata.Error) {
				RLog.Err($"Failed to premote {target} Error:{returndata.Error}");
				return;
			}
			var createdrec = returndata.Data;
			var newUrl = new Uri($"rdb://{createdrec.RecordID}");
			Engine.assetManager.PremoteAsset(target, newUrl);
			_netManager.SendToAll(Serializer.Save<IAssetRequest>(new PremoteAsset { URL = target.ToString(), NewURL = newUrl.ToString() }), 2, ASSET_DELIVERY_METHOD);
			OnPremoteLocalAssets?.Invoke(target, newUrl);
		}

		public void PremoteAsset(Uri target, Guid? arreay = null) {
			Task.Run(async () => await PremoteAssetAsync(target, arreay));
		}

		public static readonly Dictionary<Uri, string> AssetMimeType = new();

		private void AssetResponses(IAssetRequest assetRequest, Peer tag, DeliveryMethod deliveryMethod) {
			var dataUrl = new Uri(assetRequest.URL);
			var firstData = dataUrl.Host;
			if (assetRequest is RequestAsset reqwest) {
				if (dataUrl.Scheme == "local") {
					if (!firstData.StartsWith($"{SessionID.Value}-{LocalUserID}")) {
						return;
					}
				}
				var data = Engine.assetManager.GetCached(dataUrl);
				if (data is null || !AssetMimeType.TryGetValue(dataUrl, out var dataMime)) {
					tag.SendAsset(Serializer.Save<IAssetRequest>(new AssetResponse { URL = assetRequest.URL, Bytes = null, MimeType = null }), ASSET_DELIVERY_METHOD);
					return;
				}
				tag.SendAsset(Serializer.Save<IAssetRequest>(new AssetResponse { URL = assetRequest.URL, Bytes = data, MimeType = dataMime }), ASSET_DELIVERY_METHOD);
				return;
			}
			if (!firstData.StartsWith($"{SessionID.Value}-{tag.User.ID}")) {
				return;
			}
			if (assetRequest is AssetResponse assetData) {
				if (!WaitingOnAssets.Contains(dataUrl)) {
					return;
				}
				WaitingOnAssets.Remove(dataUrl);
				if (assetData.Bytes is null) {
					return;
				}
				AssetMimeType.Add(dataUrl, assetData.MimeType);
				OnLoadedLocalAssets?.Invoke(dataUrl, assetData.Bytes);
			}
			else if (assetRequest is PremoteAsset premote) {
				Engine.assetManager.PremoteAsset(dataUrl, new Uri(premote.NewURL));
				OnPremoteLocalAssets?.Invoke(dataUrl, new Uri(premote.NewURL));
			}
		}

		public event Action<Uri, byte[]> OnLoadedLocalAssets;
		public event Action<Uri, Uri> OnPremoteLocalAssets;

	}
}
