
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Jint;

using RhuEngine.Linker;

namespace RhuEngine.Managers
{
	/// <summary>
	/// Manages all the drives, physical and networked.
	/// </summary>
	public sealed class FileManager : IManager
	{
		public readonly List<SystemDrive> Drives = new();
		public readonly Dictionary<Guid, NetworkedDrive> NetDrives = new();
		public FakeStaticDrive fakeStaticFolder;

		/// <summary>
		/// Gets the specified drive.
		/// </summary>
		/// <param name="driveName">Name of the drive.</param>
		/// <returns></returns>
		public IDrive GetDrive(string driveName) {
			foreach (var item in GetDrives()) {
				if (item.Name == driveName) {
					return item;
				}
				if (item.Path == driveName) {
					return item;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the drives.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IDrive> GetDrives() {
			yield return fakeStaticFolder;
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

		/// <summary>
		/// Reloads all System Drives
		/// </summary>
		public void ReloadSystemDrives() {
			lock (Drives) {
				Drives.Clear();
				foreach (var drive in DriveInfo.GetDrives()) {
					Drives.Add(new SystemDrive(_engine, drive));
				}
			}
		}

		/// <summary>
		/// Reloads all drives, including networked drives
		/// </summary>
		public void ReloadAllDrives() {
			ReloadSystemDrives();
			Task.Run(_engine.netApiManager.Client.GetRootFolders);
		}

		/// <summary>
		/// This is a async version of <see cref="ReloadAllDrives"/>
		/// </summary>
		/// <returns></returns>
		public async Task ReloadAllDrivesAsync() {
			ReloadSystemDrives();
			await _engine.netApiManager.Client.GetRootFolders();
		}

		/// <summary>
		/// Tries to get the Folder/File from a Path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="folder"></param>
		/// <param name="file"></param>
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

		/// <summary>
		/// Initializes the FileManager
		/// </summary>
		/// <param name="engine">Instance of the Engine</param>
		public void Init(Engine engine) {
			_engine = engine;
			fakeStaticFolder = new FakeStaticDrive(engine);
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
