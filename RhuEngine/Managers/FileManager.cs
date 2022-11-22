
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
					Drives.Add(new SystemDrive(drive));
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
