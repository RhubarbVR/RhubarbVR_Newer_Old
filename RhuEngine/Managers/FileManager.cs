
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Jint;

using RhuEngine.Linker;

namespace RhuEngine.Managers
{
	public sealed class FileManager : IManager
	{
		public readonly List<SystemDrive> Drives = new();
		public readonly List<NetworkedDrive> NetDrives = new();

		public IEnumerable<IDrive> GetDrives() {
			lock (Drives) {
				foreach (var item in Drives) {
					yield return item;
				}
			}
		}

		public void ReloadSystemDrives() {
			lock (Drives) {
				Drives.Clear();
				foreach (var drive in DriveInfo.GetDrives()) {
					Drives.Add(new SystemDrive(_engine, drive));
				}
			}
		}
		public void ReloadNetDrives() {
			Task.Run(AsyncReloadNetDrives);
		}

		public void ReloadAllDrives() {
			ReloadSystemDrives();
			ReloadNetDrives();
		}

		public async Task ReloadAllDrivesAsync() {
			ReloadSystemDrives();
			await AsyncReloadNetDrives();
		}

		public bool TryGetDataFromPath(string path, out IFolder folder, out IFile file) {
			if (Directory.Exists(path)) {
				folder = path.EndsWith("\\") || path.EndsWith("/") ? GetSystemFolder(path) : (IFolder)GetSystemFolder(path + "\\");
				file = null;
				return true;
			}
			if (File.Exists(path)) {
				folder = null;
				file = GetSystemFile(path);
				return true;
			}
			foreach (var item in NetDrives) {
				if ((item.Path.ToLower() == path.ToLower()) || (item._target.ToString() == path)) {
					return item.TryGetDataFromPath(path, out folder, out file);
				}
			}
			folder = null;
			file = null;
			return false;
		}
		private SystemDrive GetSystemDrive(string pathRoot) {
			ReloadSystemDrives();
			foreach (var item in Drives) {
				if (item.Path == pathRoot) {
					return item;
				}
			}
			return null;
		}

		private SystemFile GetSystemFile(string path) {
			var drive = GetSystemDrive(Path.GetPathRoot(path));
			return drive.GetFile(path);
		}
		private SystemFolder GetSystemFolder(string path) {
			var drive = GetSystemDrive(Path.GetPathRoot(path));
			RLog.Info(path);
			return drive.GetFolder(path);
		}

		public async Task AsyncReloadNetDrives() {
			var data = await _engine.netApiManager.Client.GetRootFolders();
			if (data.Error) {
				return;
			}
			lock (NetDrives) {
				NetDrives.Clear();
				foreach (var drive in data.Data) {
					NetDrives.Add(new NetworkedDrive(drive.Id, _engine.netApiManager, drive.Id != _engine.netApiManager.Client.User.Id));
				}
			}
		}

		public void Dispose() {

		}

		private Engine _engine;

		public void Init(Engine engine) {
			_engine = engine;
			ReloadSystemDrives();
			ReloadNetDrives();

		}

		public void RenderStep() {

		}

		public void Step() {

		}
	}
}
