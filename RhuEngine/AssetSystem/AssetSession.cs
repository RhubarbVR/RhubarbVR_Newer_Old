using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.Managers;
using RhuEngine.WorldObjects;

using RNumerics;

namespace RhuEngine.AssetSystem
{
	public class AssetSession: IDisposable
	{
		public readonly AssetManager Manager;

		public World World { get; private set; }

		public AssetTask AssetLoadingTask(Action<byte[]> action,Uri asset,bool useCache) {
			var task = new AssetTask(action,this,asset,useCache);
			Manager.tasks.Add(task);
			task.Start();
			return task;
		}

		public byte[] GetAsset(Uri uri,bool useCache, Action<float> ProgressUpdate = null) {
			return uri.Scheme.ToLower()=="local" ? GetLocalAsset(uri, useCache, ProgressUpdate) : Manager.GetAsset(uri, useCache, ProgressUpdate);
		}

		private byte[] GetLocalAsset(Uri uri, bool useCache, Action<float> ProgressUpdate = null) {
			byte[] asset = null;
			if (useCache) {
				asset = Manager.GetCacheAsset(uri);
			}
			return asset is null ? GetLocalAsset(uri) : asset;
		}

		private byte[] GetLocalAsset(Uri uri) {
			return World.RequestAssets(uri);
		}

		public AssetSession(AssetManager manager,World world) {
			World = world;
			Manager = manager;
			Manager.assetSessions.Add(this);
		}

		public void Dispose() {
			Manager.assetSessions.Remove(this);
		}
	}
}
