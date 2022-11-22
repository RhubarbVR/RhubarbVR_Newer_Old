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
		public NetworkedDrive(Guid target, NetApiManager netApiManager, bool isGroup) {
			_target = target;
			_netApiManager = netApiManager;
			_isGroup = isGroup;
			UpdateInfo();
		}
		private readonly NetApiManager _netApiManager;

		private readonly Guid _target;
		private readonly bool _isGroup;

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

		public SyncFolder syncFolder;

		private void UpdateInfo() {
			Task.Run(UpdateInfoAsync);
		}
		private async Task UpdateInfoAsync() {
			await _netApiManager.Client.GetFolder(_target);
			if (_isGroup) {
				//Todo Group Info
			}
			else {
				UsedBytes = _netApiManager.Client.User.UsedBytes;
				TotalBytes = _netApiManager.Client.User.TotalBytes;
			}
		}
	}
}
