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

namespace RhuEngine.WorldObjects
{
	public partial class World
	{
		private void AssetResponses(IAssetRequest assetRequest,Peer peer, DeliveryMethod deliveryMethod) {

		}

		

		public byte[] RequestAssets(Uri uri) {
			var userID = uri.AbsolutePath.Substring(0,uri.AbsolutePath.IndexOf('/'));
			var user = GetUserFromID(userID);
			if (user == null) {
				Log.Err("User was null when loadeding LocalAsset");
				return null;
			}
			if(user.CurrentPeer == null) {
				Log.Err("User Peer was null when loadeding LocalAsset");
				return null;
			}
			user.CurrentPeer.Send(Serializer.Save<IAssetRequest>(new RequestAsset { URL = uri.AbsolutePath }), DeliveryMethod.ReliableSequenced);
			while (true) {
				Thread.Sleep(10);
			}
			return null;
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
