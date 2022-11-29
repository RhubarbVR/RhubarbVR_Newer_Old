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
	public sealed class AssetManager : IManager
	{
		public AssetManager(string cachePath) {
			CacheDir = cachePath is null ? Engine.BaseDir + "/Cache" : cachePath;
		}

		public string CacheDir = "";

		public string LocalPath => Path.Combine(CacheDir, "local");

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

		public async Task SaveNew(string sessionID, ushort localUserID, Guid newID, Stream data) {
			if (!Directory.Exists(LocalPath)) {
				Directory.CreateDirectory(LocalPath);
			}
			var createFile = File.Create(Path.Combine(LocalPath, $"{sessionID}-{localUserID}-{newID}"));
			await data.CopyToAsync(createFile);
			createFile.Close();
		}

		public void SaveNew(string sessionID, ushort localUserID, Guid newID, byte[] bytes) {
			if (!Directory.Exists(LocalPath)) {
				Directory.CreateDirectory(LocalPath);
			}
			File.WriteAllBytes(Path.Combine(LocalPath, $"{sessionID}-{localUserID}-{newID}"), bytes);
		}
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
		public byte[] GetCached(Uri uri) {
			var path = GetCachedPath(uri);
			return File.Exists(path) ? File.ReadAllBytes(path) : null;
		}

		public void PremoteAsset(Uri dataUrl, Uri uri) {
			var data = GetCached(dataUrl);
			if (data is not null) {
				File.WriteAllBytes(GetCachedPath(uri), data);
			}
		}
	}
}
