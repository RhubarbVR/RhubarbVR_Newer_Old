using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RhuEngine.AssetSystem;
using RhuEngine.AssetSystem.AssetProtocals;
using System.IO;
using System.Threading.Tasks;
using RhuEngine.Linker;
using RNumerics;

namespace RhuEngine.Managers
{
	public class AssetManager : IManager
	{
		public AssetManager(string cachePath) {
			CacheDir = cachePath is null ? Engine.BaseDir + "/Cache" : cachePath;
		}

		public string CacheDir = "";

		public SynchronizedCollection<AssetSession> assetSessions = new();

		public IAssetProtocol[] protocols;

		public SynchronizedCollection<AssetTask> tasks = new();

		public byte[] GetAsset(Uri asset, bool useCache, Action<float> ProgressUpdate = null) {
			byte[] assetData = null;
			var lowerScema = asset.Scheme.ToLower();
			if (useCache) {
				assetData = GetCacheAsset(asset);
			}
			if (assetData is null) {
				foreach (var item in protocols) {
					if (item.Schemes.Contains(lowerScema) && assetData is null) {
						assetData = item.ProccessAsset(asset, ProgressUpdate).Result;
					}
				}
			}
			if (assetData is not null && useCache) {
				CacheAsset(asset, assetData);
			}
			return assetData;
		}

		public byte[] GetCacheAsset(Uri asset) {
			return IsCache(asset) ? File.ReadAllBytes(GetAssetFile(asset)) : null;
		}

		public string GetAssetFile(Uri asset) {
			return $"{GetAssetDir(asset)}{asset.AbsolutePath.Replace('/', '_').Replace('/', '_').Replace('%', 'P')}.RAsset";
		}

		public string GetAssetDir(Uri asset) {
			return asset.Scheme.ToLower() == "local" ? $"{CacheDir}/local/" : $"{CacheDir}/{asset.Host}{asset.Port}/";
		}

		public bool IsCache(Uri asset) {
			return File.Exists(GetAssetFile(asset));
		}

		public void CacheAsset(Uri asset, byte[] data) {
			if (IsCache(asset)) {
				return;
			}
			try {
				Directory.CreateDirectory(GetAssetDir(asset));
			}
			catch (Exception e) {
				RLog.Err("Error creating Asset Cache Dir Error:" + e.ToString());
			}
			try {
				File.WriteAllBytes(GetAssetFile(asset), data);
			}
			catch (Exception e) {
				RLog.Err("Error creating Asset Cache File Error:" + e.ToString());
			}
		}

		public void Dispose() {
			try {
				if (Directory.Exists($"{CacheDir}/local/")) {
					Directory.Delete($"{CacheDir}/local/", true);
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to clear Local cache {e}");
			}
		}

		public void Init(Engine engine) {
			protocols = new IAssetProtocol[] { new HttpHttpsProtocol(this), new FtpFtpsProtocol(this) };
		}

		public void Step() {
		}
		public void RenderStep() {
		}

	}
}
