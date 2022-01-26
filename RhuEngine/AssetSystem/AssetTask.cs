using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RhuEngine.AssetSystem
{
	public class AssetTask
	{
		public Task runningTask;
		public Action<byte[]> Action;
		public AssetSession assetSession;
		public Uri assetUri;
		public bool useCache;
		public AssetTask(Action<byte[]> action, AssetSession assetSession, Uri assetUri, bool useCache) {
			Action = action;
			this.assetSession = assetSession;
			this.assetUri = assetUri;
			this.useCache = useCache;
		}

		public void Start() {
			runningTask = Task.Run(() => {
				var assetdata = assetSession.GetAsset(assetUri, useCache);
				Action(assetdata);
				assetSession.Manager.tasks.Remove(this);
			});
		}

		public void Stop() {
			runningTask.Dispose();
			assetSession.Manager.tasks.Remove(this);
		}
	}
}
