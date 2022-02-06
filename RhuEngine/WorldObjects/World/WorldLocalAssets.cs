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

namespace RhuEngine.WorldObjects
{
	public partial class World
	{
		private void AssetResponses(IAssetRequest assetRequest,Peer peer, DeliveryMethod deliveryMethod) {

		}

		public void RequestAssets(Uri uri) {

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
