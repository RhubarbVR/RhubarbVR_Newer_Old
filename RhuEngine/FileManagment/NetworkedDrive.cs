using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RhubarbCloudClient.Model;

using RhuEngine.Managers;

namespace RhuEngine
{
	public sealed class NetworkedDrive : IDrive
	{
		public NetworkedDrive(SyncFolder target, NetApiManager netApiManager, bool isGroup) {
			_target = target.Id;
			_netApiManager = netApiManager;
			_isGroup = isGroup;
			Engine = netApiManager.WorldManager.Engine;
			syncFolder = target;
			UpdateInfo();
		}
		public NetworkedDrive(Guid target, NetApiManager netApiManager, bool isGroup) {
			_target = target;
			_netApiManager = netApiManager;
			_isGroup = isGroup;
			Engine = netApiManager.WorldManager.Engine;
			UpdateInfo();
		}
		private readonly NetApiManager _netApiManager;

		public readonly Guid _target;
		public readonly bool _isGroup;

		public string Path => Name;

		public string Name
		{
			get => syncFolder.Name;
			set => Task.Run(async () => {
				await _netApiManager.Client.SetFolderName(_target, value);
				await UpdateInfoAsync();
			});
		}

		public long UsedBytes { get; set; }

		public long TotalBytes { get; set; }

		public IFolder Root => new NetworkedFolder(_target, _netApiManager, null, this);

		public Engine Engine { get; }

		public SyncFolder syncFolder;

		private void UpdateInfo() {
			Task.Run(UpdateInfoAsync);
		}
		private async Task UpdateInfoAsync() {
			var data = await _netApiManager.Client.GetFolder(_target);
			syncFolder = data.Data;
			if (_isGroup) {
				//Todo Group Info
			}
			else {
				UsedBytes = _netApiManager.Client.User.UsedBytes;
				TotalBytes = _netApiManager.Client.User.TotalBytes;
			}
		}

		public bool TryGetDataFromPath(string path, out IFolder folder, out IFile file) {
			var data = path.Split('/', '\\');
			var parrentFolder = Root;
			for (var i = 1; i < data.Length; i++) {
				var currentPathPoint = data[i];
				var foundParrent = false;
				foreach (var item in parrentFolder.Folders) {
					if (item.Name == currentPathPoint) {
						parrentFolder = item;
						foundParrent = true;
						if (data.Length == i + 1) {
							folder = item;
							file = null;
							return true;
						}
						break;
					}
				}
				if (foundParrent) {
					continue;
				}
				foreach (var item in parrentFolder.Files) {
					if (item.Name == currentPathPoint) {
						folder = null;
						file = item;
						return true;
					}
				}
			}
			folder = null;
			file = null;
			return false;
		}
	}
}
