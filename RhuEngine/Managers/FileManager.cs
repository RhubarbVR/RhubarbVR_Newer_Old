
using System;
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
		public readonly Dictionary<Guid, NetworkedDrive> NetDrives = new();

		public IEnumerable<IDrive> GetDrives() {
			lock (_engine.netApiManager.Client.RootFolders) {
				foreach (var item in _engine.netApiManager.Client.RootFolders) {
					if (NetDrives.TryGetValue(item.ID, out var drive)) {
						yield return drive;
					}
					else {
						var newDrive = new NetworkedDrive(item, _engine);
						NetDrives.Add(item.ID, newDrive);
						yield return newDrive;
					}

				}
			}
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

		public void ReloadAllDrives() {
			ReloadSystemDrives();
			Task.Run(_engine.netApiManager.Client.GetRootFolders);
		}
		public async Task ReloadAllDrivesAsync() {
			ReloadSystemDrives();
			await _engine.netApiManager.Client.GetRootFolders();
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

		public void Dispose() {

		}

		private Engine _engine;

		public void Init(Engine engine) {
			_engine = engine;
			_engine.netApiManager.Client.OnLogin += (x) => ReloadAllDrives();
			_engine.netApiManager.Client.OnLogout += ReloadAllDrives;
			ReloadAllDrives();
		}

		public void RenderStep() {

		}

		public void Step() {

		}
	}
}
