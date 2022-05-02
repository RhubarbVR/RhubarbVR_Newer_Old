using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.AssetSystem;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;

namespace RhuEngine.Components
{
	public abstract class StaticAsset<T> : AssetProvider<T> where T : class
	{
		[OnChanged(nameof(StartLoadAsset))]
		public readonly Sync<string> url;

		[Default(true)]
		[OnChanged(nameof(StartLoadAsset))]
		public readonly Sync<bool> useCache;

		public AssetTask CurrentTask = null;

		public virtual void StartLoadAsset() {
			if(url.Value == null) {
				return;
			}
			try {
				RLog.Info("Starting load of static Asset");
				var uri = new Uri(url);
				var lastTask = CurrentTask;
				CurrentTask = World.assetSession.AssetLoadingTask((data)=> { lastTask?.Stop(); LoadAsset(data); }, uri, useCache || uri.Scheme.ToLower() == "local");
			}
			catch (Exception e) {
				RLog.Info($"Error Loading static Asset {e}");
			}
		}


		public override void OnLoaded() {
			base.OnLoaded();
			StartLoadAsset();
		}

		public virtual void LoadAsset(byte[] data) {
			throw new NotImplementedException();
		}
	}
}
