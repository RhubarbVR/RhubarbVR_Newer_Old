using System;
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
			Engine.assetManager.SaveNew(SessionID.Value, LocalUserID, newID, RhubarbFileManager.SaveFile(Engine.netApiManager.Client.User?.Id ?? Guid.Empty, amesh));
			var uri = new Uri($"local://{SessionID.Value}-{LocalUserID}-{newID}");
			AssetMimeType.Add(uri, "application/rhubarbvr_mesh");
			return uri;
		}
		public readonly HashSet<Uri> WaitingOnAssets = new();
		public void RequestAsset(Uri target) {
			WaitingOnAssets.Add(target);
			_netManager.SendToAll(Serializer.Save<INetPacked>(new RequestAsset { URL = target.ToString() }), 2, ASSET_DELIVERY_METHOD);
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
			_netManager.SendToAll(Serializer.Save<INetPacked>(new PremoteAsset { URL = target.ToString(), NewURL = newUrl.ToString() }), 2, ASSET_DELIVERY_METHOD);
			OnPremoteLocalAssets?.Invoke(target, newUrl);
		}

		public void PremoteAsset(Uri target, Guid? arreay = null) {
			Task.Run(async () => await PremoteAssetAsync(target, arreay));
		}

		public static readonly Dictionary<Uri, string> AssetMimeType = new();

		public int SizeOfEachPart = 1024 * 35;

		public static readonly Dictionary<Uri, (int, MemoryStream)> assetSaving = new();

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
					tag.SendAsset(Serializer.Save<INetPacked>(new AssetResponse { URL = assetRequest.URL, PartBytes = null, MimeType = null }), ASSET_DELIVERY_METHOD);
					return;
				}
				var size = data.LongLength;
				var amountOfSections = (size / SizeOfEachPart) + ((size % SizeOfEachPart == 0) ? 0 : 1);
				var lastSize = (amountOfSections * SizeOfEachPart) - size;
				for (var i = 0; i < amountOfSections; i++) {
					var partData = new byte[SizeOfEachPart];
					if ((i + 1) == amountOfSections) {
						partData = new byte[SizeOfEachPart - lastSize];
					}
					Array.Copy(data, i * SizeOfEachPart, partData, 0, partData.Length);
					tag.SendAsset(Serializer.Save<INetPacked>(new AssetResponse { URL = assetRequest.URL, PartBytes = partData, MimeType = dataMime, CurrentPart = i, SizeOfData = size, SizeOfPart = SizeOfEachPart }), ASSET_DELIVERY_METHOD);
				}
				RLog.VerBoseInfo($"Asset Sent to {tag.User.ID} PartAmount:{amountOfSections} Size:{size}");
				return;
			}
			if (!firstData.StartsWith($"{SessionID.Value}-{tag.User.ID}-")) {
				RLog.Err($"Asset from {tag.User.ID} some one how does not own asset");
				return;
			}
			if (assetRequest is AssetResponse assetData) {
				RLog.VerBoseInfo($"assetData Asset Resived from {tag.User.ID} Part:{assetData.CurrentPart} SizeOfData:{assetData.SizeOfData} MimeType:{assetData.MimeType}");
				if (!WaitingOnAssets.Contains(dataUrl)) {
					RLog.Err($"assetData Not waiting for data");
					return;
				}
				if (assetData.PartBytes is null) {
					RLog.Err($"assetData HadNoData");
					return;
				}
				if (assetSaving.ContainsKey(dataUrl)) {
					RLog.VerBoseInfo($"Stream asset AddData Asset Resived from {tag.User.ID} Part:{assetData.CurrentPart} SizeOfData:{assetData.SizeOfData} MimeType:{assetData.MimeType}");
					var data = assetSaving[dataUrl];
					data.Item1++;
					data.Item2.Position = assetData.CurrentPart * assetData.SizeOfPart;
					data.Item2.Write(assetData.PartBytes, 0, assetData.PartBytes.Length);
					assetSaving[dataUrl] = data;
					
					if (data.Item1 == ((assetData.SizeOfData / assetData.SizeOfPart) + ((assetData.SizeOfData % assetData.SizeOfPart == 0) ? 0 : 1))) {
						RLog.VerBoseInfo($"Stream asset ALL Asset Resived from {tag.User.ID} Part:{assetData.CurrentPart} SizeOfData:{assetData.SizeOfData} MimeType:{assetData.MimeType}");
						var buffer = data.Item2.GetBuffer();
						data.Item2.Dispose();
						AssetMimeType.Add(dataUrl, assetData.MimeType);
						OnLoadedLocalAssets?.Invoke(dataUrl, buffer);
						WaitingOnAssets.Remove(dataUrl);
					}
				}
				else {
					if (assetData.SizeOfData < assetData.SizeOfPart) {
						RLog.VerBoseInfo($"One File Asset Resived from {tag.User.ID} SizeOfData:{assetData.SizeOfData} MimeType:{assetData.MimeType}");
						AssetMimeType.Add(dataUrl, assetData.MimeType);
						OnLoadedLocalAssets?.Invoke(dataUrl, assetData.PartBytes);
					}
					else {
						RLog.VerBoseInfo($"Stream asset Create Asset Resived to {tag.User.ID} Part:{assetData.CurrentPart} SizeOfData:{assetData.SizeOfData} MimeType:{assetData.MimeType}");
						var data = new MemoryStream(new byte[(int)assetData.SizeOfData], 0, (int)assetData.SizeOfData, true, true);
						data.Write(assetData.PartBytes, assetData.CurrentPart * assetData.SizeOfPart, assetData.PartBytes.Length);
						assetSaving.Add(dataUrl, (1, data));
					}
				}
			}
			else if (assetRequest is PremoteAsset premote) {
				RLog.VerBoseInfo($"premote Asset Resived from {tag.User.ID} URL:{premote.URL} NewURL:{premote.NewURL}");
				Engine.assetManager.PremoteAsset(dataUrl, new Uri(premote.NewURL));
				OnPremoteLocalAssets?.Invoke(dataUrl, new Uri(premote.NewURL));
			}
			else {
				RLog.Err($"AssetType Not known");
			}
		}

		public event Action<Uri, byte[]> OnLoadedLocalAssets;
		public event Action<Uri, Uri> OnPremoteLocalAssets;

	}
}
