using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using RhuEngine.Linker;
using RNumerics;
using System.Runtime.InteropServices.ComTypes;

namespace RhuEngine.Managers
{
	/// <summary>
	/// Asset manager - Loads assets, caches them and more
	/// </summary>
	public sealed class AssetManager : IManager
	{
		/// <summary>
		/// Creates AssetManager with default cache path:
		/// Engine.BaseDir + "/Cache"
		/// </summary>
		public AssetManager() {
			CacheDir = Engine.BaseDir + "/Cache";
		}
		/// <summary>
		/// Creates AssetManager with custom cache path
		/// </summary>
		/// <param name="cachePath"></param>
		public AssetManager(string cachePath) {
			CacheDir = cachePath is null ? Engine.BaseDir + "/Cache" : cachePath;
		}

		/// <summary>
		/// Path to the cache directory
		/// </summary>
		public string CacheDir = "";

		/// <summary>
		/// The Local Path for Assets, such as User Generated assets and Temp Assets
		/// </summary>
		public string LocalPath => Path.Combine(CacheDir, "local");

		/// <summary>
		/// Disposes of this instance
		/// </summary>
		public void Dispose() {
			try {
				if (Directory.Exists(LocalPath)) {
					Directory.Delete(LocalPath, true);
				}
			}
			catch (Exception e) {
				RLog.Err($"Failed to clear Local cache {e}");
			}
		}

		public void Init(Engine engine) {

		}

		public void Step() {
		}
		public void RenderStep() {
		}
		/// <summary>
		/// Saves a new asset to the local cache async
		/// </summary>
		/// <param name="sessionID"></param>
		/// <param name="localUserID"></param>
		/// <param name="newID"></param>
		/// <param name="data"></param>
		public async Task SaveNew(string sessionID, ushort localUserID, Guid newID, Stream data) {
			if (!Directory.Exists(LocalPath)) {
				Directory.CreateDirectory(LocalPath);
			}
			var createFile = File.Create(Path.Combine(LocalPath, $"{sessionID}-{localUserID}-{newID}"));
			await data.CopyToAsync(createFile);
			createFile.Close();
		}

		/// <summary>
		/// Saves a new asset to the local cache
		/// </summary>
		/// <param name="sessionID"></param>
		/// <param name="localUserID"></param>
		/// <param name="newID"></param>
		/// <param name="data"></param>
		public void SaveNew(string sessionID, ushort localUserID, Guid newID, byte[] bytes) {
			if (!Directory.Exists(LocalPath)) {
				Directory.CreateDirectory(LocalPath);
			}
			File.WriteAllBytes(Path.Combine(LocalPath, $"{sessionID}-{localUserID}-{newID}"), bytes);
		}

		/// <summary>
		/// Get the path for a cached asset
		/// </summary>
		/// <param name="uri">the uri of the asset</param>
		/// <returns>The path to the cached asset</returns>
		public string GetCachedPath(Uri uri) {
			if(uri.Scheme is "local" or "rdb") {
				return Path.Combine(CacheDir, uri.Scheme, uri.Host);
			}
			var fileName = uri.ToString();
			foreach (var c in Path.GetInvalidFileNameChars()) {
				fileName = fileName.Replace(c, '-');
			}
			var startPath = Path.Combine(CacheDir, "web");
			if (!Directory.Exists(startPath)) {
				Directory.CreateDirectory(startPath);
			}
			var other = Path.Combine(startPath, uri.Scheme);
			if (!Directory.Exists(other)) {
				Directory.CreateDirectory(other);
			}
			return Path.Combine(other, fileName);

		}

		/// <summary>
		/// Gets the cached asset.
		/// </summary>
		/// <returns>The cached asset.</returns>
		/// <param name="uri">URI.</param>
		public byte[] GetCached(Uri uri) {
			var path = GetCachedPath(uri);
			return File.Exists(path) ? File.ReadAllBytes(path) : null;
		}

		/// <summary>
		/// Moves the asset from dataUrl to a new Uri
		/// </summary>
		/// <param name="dataUrl"></param>
		/// <param name="uri"></param>
		public void PremoteAsset(Uri dataUrl, Uri uri) {
			var data = GetCached(dataUrl);
			if (data is not null) {
				File.WriteAllBytes(GetCachedPath(uri), data);
			}
		}
	}
}
