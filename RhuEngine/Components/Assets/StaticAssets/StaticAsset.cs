using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;

using RhuEngine.Linker;
using System.Security.Policy;
using System.Net.Http;
using System.IO;

namespace RhuEngine.Components
{
	public abstract class StaticAsset<T> : AssetProvider<T> where T : class
	{
		[OnChanged(nameof(StartLoadAsset))]
		public readonly Sync<string> url;

		private bool _staticAssetLoaded = false;

		private static async Task<byte[]> LoadAssetInWebUrl(Uri targetUri) {
			var HttpClientHandler = new HttpClientHandler {
				AllowAutoRedirect = true,
			};
			using var client = new HttpClient(HttpClientHandler);
			using var response = await client.GetAsync(targetUri, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();
			var totalBytes = response.Content.Headers.ContentLength;
			return await response.Content.ReadAsByteArrayAsync();
		}

		private async Task LoadAssetIn() {
			var targetUri = new Uri(url);
			try {
				if (targetUri.Scheme == "local") {
					World.RequestAsset(targetUri);
					return;
				}
				else if (targetUri.Scheme == "rdb") {
					var recAcess = await Engine.netApiManager.Client.GetRecordAccesses(Guid.Parse(targetUri.Host));
					if (recAcess.Error) {
						World.RequestAsset(targetUri);
						return;
					}
					var data = await LoadAssetInWebUrl(new Uri(recAcess.Data.TempURL));
					File.WriteAllBytes(Engine.assetManager.GetCachedPath(targetUri), data);
					LoadAsset(data);
					_staticAssetLoaded = true;
					OnLoadedAsset?.Invoke(targetUri);
				}
				else {
					var data = await LoadAssetInWebUrl(targetUri);
					File.WriteAllBytes(Engine.assetManager.GetCachedPath(targetUri), data);
					LoadAsset(data);
					_staticAssetLoaded = true;
					OnLoadedAsset?.Invoke(targetUri);
				}
				lock (tryingToLoad) {
					tryingToLoad.Remove(targetUri);
				}
			}
			catch {
				lock (tryingToLoad) {
					tryingToLoad.Remove(targetUri);
				}
			}
		}

		public static event Action<Uri> OnLoadedAsset;
		public static readonly HashSet<Uri> tryingToLoad = new();

		public virtual void StartLoadAsset() {
			_staticAssetLoaded = false;
			if (url.Value == null) {
				return;
			}
			try {
				RLog.Info("Starting load of static Asset");
				//Todo set up system to use cache headers on http and https
				var uri = new Uri(url);
				if (uri.Scheme == "http" | uri.Scheme == "https" | uri.Scheme == "rdb" | uri.Scheme == "local") {
					var data = Engine.assetManager.GetCached(uri);
					if (data is null) {
						lock (tryingToLoad) {
							if (!tryingToLoad.Contains(uri)) {
								tryingToLoad.Add(uri);
								Task.Run(LoadAssetIn);
							}
						}
					}
					else {
						RLog.Info("loaded static Asset from cache");
						LoadAsset(data);
						_staticAssetLoaded = true;
					}
				}
				else {
					RLog.Info("loaded static Asset no Cache");
					LoadAsset(null);
					_staticAssetLoaded = false;
				}
			}
			catch (Exception e) {
				RLog.Info($"Error Loading static Asset {e}");
			}
		}


		protected override void OnLoaded() {
			base.OnLoaded();
			World.OnPremoteLocalAssets += World_OnPremoteLocalAssets;
			World.OnLoadedLocalAssets += World_OnLoadedLocalAssets;
			OnLoadedAsset += StaticAsset_OnLoadedAsset;
			StartLoadAsset();
		}

		private void StaticAsset_OnLoadedAsset(Uri obj) {
			if (_staticAssetLoaded) {
				return;
			}
			if (url.Value == obj.ToString()) {
				var data = Engine.assetManager.GetCached(obj);
				LoadAsset(data);
				_staticAssetLoaded = data is not null;
			}
		}

		private void World_OnLoadedLocalAssets(Uri arg1, byte[] arg2) {
			if (_staticAssetLoaded) {
				return;
			}
			lock (tryingToLoad) {
				if (tryingToLoad.Contains(arg1)) {
					if (url.Value == arg1.ToString()) {
						tryingToLoad.Remove(arg1);
						LoadAsset(arg2);
						_staticAssetLoaded = arg2 is not null;
					}
				}
			}
		}

		private void World_OnPremoteLocalAssets(Uri arg1, Uri arg2) {
			if (url.Value == arg1.ToString()) {
				url.SetValueNoOnChangeAndNetworking(arg2.ToString());
			}
		}

		public abstract void LoadAsset(byte[] data);
	}
}
