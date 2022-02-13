using System;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;

using RhuEngine.Managers;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.AssetSystem;
using StereoKit;
using RhuEngine.AssetSystem.RequestStructs;
using LiteNetLib;
using System.Threading.Tasks;
using SharedModels;
using System.Collections.Generic;

namespace RhuEngine.WorldObjects
{
	public partial class World
	{
		public const DeliveryMethod ASSET_DELIVERY_METHOD = DeliveryMethod.ReliableUnordered;

		public Dictionary<string, LocalAssetLoadTask> loadTasks = new();

		public class LocalAssetLoadTask
		{
			public bool ReadyToStart { get; private set; } = false;
			private readonly World _world;
			public bool IsLoading { get; private set; } = true;

			public byte[] Data { get; private set; }

			public LocalAssetLoadTask(World world,string url) {
				_world = world;
				Url = url;
				try {
					_world.loadTasks.Add(url, this);
					ReadyToStart = true;
				}
				catch {
					ReadyToStart = false;
				}
			}

			public byte[] WaitForByteArray() {
				while (IsLoading) {
					Thread.Sleep(10);
				}
				return Data;
			}

			public byte[] Load(Uri uri) {
				var path = uri.AbsolutePath;
				var userID = path.Substring(0, path.IndexOf('/'));
				var user = _world.GetUserFromID(userID);
				if (user == null) {
					Log.Err($"User was null when loadeding LocalAsset UserID: {userID} Path {path}");
					return null;
				}
				if (user.CurrentPeer == null) {
					Log.Err("User Peer was null when loadeding LocalAsset");
					return null;
				}
				user.CurrentPeer.Send(Serializer.Save<IAssetRequest>(new RequestAsset { URL = path }), ASSET_DELIVERY_METHOD);
				while (true) {
					Thread.Sleep(10);
				}

				IsLoading = false;
			}

			public string Url { get; private set; }
		}

		public List<LocalAssetSendTask> sendTasks = new();

		public class LocalAssetSendTask
		{
			private readonly World _world;

			public Task Task { get; private set; }

			public LocalAssetSendTask(World world,string url,Peer requester) {
				_world = world;
				Url = url;
				Requester = requester;
				world.sendTasks.Add(this);
				Task = Task.Run(SendLoop);
			}

			public uint maxChunkSizeBytes = 1024 * 15;

			public void SendLoop() {
				var asset = _world.assetSession.GetAsset(new Uri($"local:///{Url}"), true);
				double devis = 0;
				for (var i = maxChunkSizeBytes; i >= 0; i--) {
					devis = ((double)asset.LongLength) / i;
					if((devis % 1) == 0) {
						break;
					}
				}
				var chunksize = (uint)devis;
				var chunkAmount = asset.LongLength/chunksize;
				Log.Info($"Sending asset with chunkSize:{chunksize} ChunkAmount:{chunkAmount} assetSize{asset.LongLength}");
				if (asset != null) {
					Requester.Send(Serializer.Save<IAssetRequest>(new AssetResponse { URL = Url,ChunkAmount = chunkAmount,ChunkSizeBytes = chunksize}), ASSET_DELIVERY_METHOD);
					var chunkBuffer = new byte[chunksize];
					var remainingChunks = chunkAmount;
					while (remainingChunks == 0) {
						Array.Copy(asset,(remainingChunks - 1)*chunksize, chunkBuffer,0, chunkBuffer.Length);
						Requester.Send(Serializer.Save<IAssetRequest>(new AssetChunk { URL = Url,ChunkID = remainingChunks, data =  chunkBuffer}), ASSET_DELIVERY_METHOD);
						remainingChunks--;
						Thread.Sleep(10);
					}
					Log.Info("Sent all Chunks");
				}
			}

			public string Url { get; }
			public Peer Requester { get; private set; }
		}

		private void AssetResponses(IAssetRequest assetRequest,Peer peer, DeliveryMethod deliveryMethod) {
			if (assetRequest is RequestAsset request) {
				Log.Info("User Wants LocalAsset" + request.URL);
				new LocalAssetSendTask(this, request.URL, peer);
			}
			else if (assetRequest is AssetChunk assetChunk) {

			}
			else if (assetRequest is AssetResponse response) {
				Log.Info($"Asset Resived with chunkSize:{response.ChunkSizeBytes} ChunkAmount:{response.ChunkAmount} assetSize{response.ChunkAmount * response.ChunkSizeBytes}");
			}
		}

		

		public byte[] RequestAssets(Uri uri) {
			var loadTask = new LocalAssetLoadTask(this, uri.AbsolutePath);
			if (loadTask.ReadyToStart) {
				return loadTask.Load(uri);
			}
			else {
				if (loadTasks.TryGetValue(uri.AbsolutePath, out loadTask)) {
					return loadTask.WaitForByteArray();
				}
				else {
					Log.Err("Asset Load Task Not found");
					return null;
				}
			}
		}

		public Uri LoadLocalAsset(byte[] data,string fileExs) {
			Log.Info("Loadeding localAsset " + fileExs);
			var addedEnd = "";
			var indexofpoint = fileExs.IndexOf('.');
			if (indexofpoint > -1) {
				addedEnd = fileExs.Substring(indexofpoint);
			}
			var user = GetLocalUser();
			var id = Guid.NewGuid().ToString();
			var uri = new Uri($"local:///{user.userID.Value}/{id}{addedEnd}");
			Engine.assetManager.CacheAsset(uri, data);
			return uri;
		}
	}
}
