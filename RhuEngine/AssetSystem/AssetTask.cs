using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using RhuEngine.Linker;

namespace RhuEngine.AssetSystem
{
	public class AssetTask
	{
		public Task runningTask;
		public Action<byte[]> Action;
		public AssetSession assetSession;
		public Uri assetUri;
		public bool useCache;
		public float Progress = 0;
		public CancellationTokenSource cancellationToken = new();
		public AssetTask(Action<byte[]> action, AssetSession assetSession, Uri assetUri, bool useCache) {
			Action = action;
			this.assetSession = assetSession;
			this.assetUri = assetUri;
			this.useCache = useCache;
		}

		public void Start() {
			runningTask = Task.Run(() => {
				try {
					RLog.Info($"Loading asset {assetUri}");
					var assetdata = assetSession.GetAsset(assetUri, useCache, (floats) => {
						Progress = floats;
						cancellationToken.Token.ThrowIfCancellationRequested();
					});
					Action(assetdata);
					assetSession.Manager.tasks.Remove(this);
				}
				catch (Exception ex) {
					RLog.Err($"Failed to run asset task Error:{ex}");
				}
			});
		}

		public void Stop() {
			RLog.Info($"Stoping loading of asset {assetUri}");
			cancellationToken.Cancel();
			runningTask.Wait();
			runningTask.Dispose();
			assetSession.Manager.tasks.Remove(this);
		}
	}
}
