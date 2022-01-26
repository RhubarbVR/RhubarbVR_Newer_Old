using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using StereoKit;

namespace RhuEngine.Components
{
	public abstract class StaticAsset<T> : AssetProvider<T> where T : class
	{
		[OnChanged(nameof(StartLoadAsset))]
		public Sync<string> url;

		[Default(true)]
		[OnChanged(nameof(StartLoadAsset))]
		public Sync<bool> useCache;

		public void StartLoadAsset() {
			if(url.Value == null) {
				return;
			}
			try {
				Log.Info("Starting load of static Asset");
				World.assetSession.AssetLoadingTask(LoadAsset, new Uri(url), useCache);
			}
			catch (Exception e) {
				Log.Info($"Error Loading static Asset {e}");
			}
		}


		public override void OnLoaded() {
			base.OnLoaded();
			StartLoadAsset();
		}

		public abstract void LoadAsset(byte[] data);
	}
}
